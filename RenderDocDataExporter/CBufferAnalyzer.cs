using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Moonflow
{
    [Serializable]
    public class CBufferAnalyzer : ScriptableObject, IResourceReceiver
    {
        public bool UEVer = false;
        public List<BufferLinker> bufferLinkers = new List<BufferLinker>();
        public List<BufferData> buffers = new List<BufferData>();
        // public List<BufferDeclaration> vBuffers = new List<BufferDeclaration>();
        private string _path;
        public void AddResource(string path)
        {
            _path = path;
            Debug.Log($"Adding resource as CBufferFile: {path}");
            //get file name from path
            string fileName = System.IO.Path.GetFileName(path);
            //get file name without extension
            fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            
            //file name format is "CBuffer_<Pixel/Vertex>_s<SetIndex>_b<BindingIndex>_<uniformIndex>_o<byteOffset>_<bufferID>_<bufferName>.txt"
            string[] nameArray = fileName.Split('_');
            string bindingIndex = nameArray[3].Substring(1);
            if (Convert.ToInt32(bindingIndex) > 32)
            {
                Debug.Log("Binding index is too high, skipping file: " + path);
                return;
            }
            BufferData buffer = new BufferData();
            BufferLinker linker = new BufferLinker();
            buffer.linkedFile = path;
            buffer.dec.offset = int.Parse(nameArray[5].Replace("o",""));
            buffer.dec.bufferId = int.Parse(nameArray[6]);
            buffer.dec.bufferName = nameArray[7];
            buffer.variables = AnalyzeCBufferFile(path);
            if (nameArray[1] == "Pixel")
            {
                buffer.passDef = ShaderPassDef.Pixel;
            }else if (nameArray[1] == "Vertex")
            {
                buffer.passDef = ShaderPassDef.Vertex;
            }
            else
            {
                Debug.LogError($"Unknown buffer type: {nameArray[1]}, path: {path}");
            }

            bool hasBuffer = false;
            for (int i = 0; i < buffers.Count; i++)
            {
                if (buffers[i].dec.bufferName == buffer.dec.bufferName && buffers[i].dec.offset == buffer.dec.offset)
                {
                    hasBuffer = true;
                    break;
                }
            }
            if (!hasBuffer)
            {
                buffers.Add(buffer);
            }
            
            linker.setIndex = int.Parse(nameArray[2].Substring(1));
            linker.bindingIndex = int.Parse(nameArray[3].Substring(1));
            try
            {
                linker.uniformIndex = int.Parse(nameArray[4].Replace("uniforms",""));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // throw;
            }
            linker.bufferDec = buffer.dec;
            linker.passDef = buffer.passDef;
            bufferLinkers.Add(linker);
        }

        private List<ShaderVariable> AnalyzeCBufferFile(string path)
        {
            List<ShaderVariable> variables = new List<ShaderVariable>();
            Debug.Log("Analyzing CBuffer file: " + path);
            using (System.IO.StreamReader file = new System.IO.StreamReader(path))
            {
                Stack<ShaderVariable> stack = new Stack<ShaderVariable>();
                int indent = 0;
                // ShaderVariable tempVariable = null;
                while (!file.EndOfStream)
                {
                    if (!UEVer)
                    {
                        if (UnityLBL(path, file, stack, variables, ref indent, out var shaderVariables)) return shaderVariables;
                    }
                    else
                    {
                        if (UELBL(path, file, stack, variables, ref indent, out var shaderVariables)) return shaderVariables;
                    }
                } 
            }
            return variables;
        }

        private bool UnityLBL(string path, StreamReader file, Stack<ShaderVariable> stack, List<ShaderVariable> variables, ref int indent,
            out List<ShaderVariable> shaderVariables)
        {
            string line = file.ReadLine();
            string[] splitDef = line.Split("_child");
            //缩进数量与标记一致，说明该行没有变为父项或者子项
            if (splitDef[0].Length / 4 == indent)
            {
                stack.TryPop(out ShaderVariable last);
                //没嵌套就是新的variable
                if (splitDef[0].Length == 0)
                {
                    ShaderVariable variable = new ShaderVariable();
                    if (line.Contains("MEMBERS:"))
                    {
                        variable.name = line.Trim().Split("  ")[0];
                        variable.sub ??= new List<ShaderVariable>();
                    }
                    else
                    {
                        AddItem(line.Trim(), ref variable);
                    }

                    var newVariable = variable.Clone() as ShaderVariable;
                    stack.Push(newVariable);
                    variables.Add(newVariable);
                }
                //有嵌套就是为当前临时项（父项）增加一个子项
                else
                {
                    ShaderVariable variable = new ShaderVariable();
                    if (line.Contains("MEMBERS:"))
                    {
                        variable.name = line.Trim().Split("  ")[0];
                        variable.sub ??= new List<ShaderVariable>();
                    }
                    else
                    {
                        AddItem(line.Trim(), ref variable);
                    }
                    try
                    {
                        stack.Peek().sub.Add(variable);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(path+"CBuffer有问题，stack已空但是仍然读取（增加一个子项）");
                        shaderVariables = variables;
                        return true;
                    }
                    stack.Push(variable);
                }
            }
            else
            {
                //产生了子项
                if (splitDef[0].Length / 4 > indent)
                {
                    ShaderVariable subVariable = new ShaderVariable();
                    try
                    {
                        stack.Peek().sub ??= new List<ShaderVariable>();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(path+"CBuffer有问题，stack已空但是仍然读取（产生了子项）");
                        shaderVariables = variables;
                        return true;
                    }
                    if (line.Contains("MEMBERS:"))
                    {
                        subVariable.name = line.Trim().Split("  ")[0];
                        subVariable.sub ??= new List<ShaderVariable>();
                    }
                    else
                    {
                        AddItem(line.Trim(), ref subVariable);
                    }
                    try
                    {
                        stack.Peek().sub.Add(subVariable);
                        stack.Push(subVariable);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(path+"CBuffer有问题，stack已空但是仍然读取（产生了子项）");
                        shaderVariables = variables;
                        return true;
                    }
                }
                //回到了父项
                else
                {
                    ShaderVariable subVariable = new ShaderVariable();
                    if (line.Contains("MEMBERS:"))
                    {
                        subVariable.name = line.Trim().Split("  ")[0];
                        subVariable.sub ??= new List<ShaderVariable>();
                    }
                    else
                    {
                        AddItem(line.Trim(), ref subVariable);
                    }

                    if (stack.Count == 0)
                    {
                        variables.Add(subVariable);
                    }
                    else
                    {
                        int currentIndent = splitDef[0].Length / 4;
                        while (currentIndent <= indent)
                        {
                            stack.Pop();
                            indent--;
                        }

                        if (currentIndent != 0)
                        {
                            stack.Peek().sub.Add(subVariable);
                        }
                        else
                        {
                            variables.Add(subVariable);
                        }
                        stack.Push(subVariable);
                    }
                }
                indent = splitDef[0].Length / 4;
            }

            void AddItem(string trimmedLine, ref ShaderVariable variable)
            {
                string[] splitDef = trimmedLine.Split("  ");
                string def = splitDef[0];
                string data = splitDef[1];
                string[] subValues = data.Split(" ");
                variable.name = def;
                if (subValues.Length > 0)
                {
                    variable.sub ??= new List<ShaderVariable>();
                    for (int i = 0; i < subValues.Length; i++)
                    {
                        try
                        {
                            float value = subValues[i] == "nan" ? Mathf.Infinity : Convert.ToSingle(subValues[i]);
                            variable.sub.Add(new ShaderVariable()
                            {
                                value = value
                            });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                                
                    }
                }
            }

            shaderVariables = variables;
            return false;
        }
        
        private bool UELBL(string path, StreamReader file, Stack<ShaderVariable> stack, List<ShaderVariable> variables, ref int indent,
            out List<ShaderVariable> shaderVariables)
        {
            string line = file.ReadLine();
            string[] splitDef = line.Split("  ");
            //缩进数量与标记一致，说明该行没有变为父项或者子项
            if (!(splitDef[0].Length ==2 && splitDef[1].Contains("MEMBERS:")))
            {
                stack.TryPop(out ShaderVariable last);
                //没嵌套就是新的variable
                if (splitDef[0].Length == 0)
                {
                    ShaderVariable variable = new ShaderVariable();
                    if (line.Contains("MEMBERS:"))
                    {
                        variable.name = line.Trim().Split("  ")[0];
                        variable.sub ??= new List<ShaderVariable>();
                    }
                    else
                    {
                        AddItem(line.Trim(), ref variable);
                    }

                    var newVariable = variable.Clone() as ShaderVariable;
                    stack.Push(newVariable);
                    variables.Add(newVariable);
                }
                //有嵌套就是为当前临时项（父项）增加一个子项
                else
                {
                    ShaderVariable variable = new ShaderVariable();
                    if (line.Contains("MEMBERS:"))
                    {
                        variable.name = line.Trim().Split("  ")[0];
                        variable.sub ??= new List<ShaderVariable>();
                    }
                    else
                    {
                        AddItem(line.Trim(), ref variable);
                    }
                    try
                    {
                        stack.Peek().sub.Add(variable);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(path+"CBuffer有问题，stack已空但是仍然读取（增加一个子项）");
                        shaderVariables = variables;
                        return true;
                    }
                    stack.Push(variable);
                }
            }
            else
            {
                //产生了子项
                if (splitDef[0].Length / 4 > indent)
                {
                    ShaderVariable subVariable = new ShaderVariable();
                    try
                    {
                        stack.Peek().sub ??= new List<ShaderVariable>();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(path+"CBuffer有问题，stack已空但是仍然读取（产生了子项）");
                        shaderVariables = variables;
                        return true;
                    }
                    if (line.Contains("MEMBERS:"))
                    {
                        subVariable.name = line.Trim().Split("  ")[0];
                        subVariable.sub ??= new List<ShaderVariable>();
                    }
                    else
                    {
                        AddItem(line.Trim(), ref subVariable);
                    }
                    try
                    {
                        stack.Peek().sub.Add(subVariable);
                        stack.Push(subVariable);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(path+"CBuffer有问题，stack已空但是仍然读取（产生了子项）");
                        shaderVariables = variables;
                        return true;
                    }
                }
                //回到了父项
                else
                {
                    ShaderVariable subVariable = new ShaderVariable();
                    if (line.Contains("MEMBERS:"))
                    {
                        subVariable.name = line.Trim().Split("  ")[0];
                        subVariable.sub ??= new List<ShaderVariable>();
                    }
                    else
                    {
                        AddItem(line.Trim(), ref subVariable);
                    }

                    if (stack.Count == 0)
                    {
                        variables.Add(subVariable);
                    }
                    else
                    {
                        int currentIndent = splitDef[0].Length / 4;
                        while (currentIndent <= indent)
                        {
                            stack.Pop();
                            indent--;
                        }

                        if (currentIndent != 0)
                        {
                            stack.Peek().sub.Add(subVariable);
                        }
                        else
                        {
                            variables.Add(subVariable);
                        }
                        stack.Push(subVariable);
                    }
                }
                indent = splitDef[0].Length / 4;
            }

            void AddItem(string trimmedLine, ref ShaderVariable variable)
            {
                string[] splitDef = trimmedLine.Split("  ");
                string def = splitDef[0];
                string data = splitDef[1];
                string[] subValues = data.Split(" ");
                variable.name = def;
                if (subValues.Length > 0)
                {
                    variable.sub ??= new List<ShaderVariable>();
                    for (int i = 0; i < subValues.Length; i++)
                    {
                        try
                        {
                            float value = subValues[i] == "nan" ? Mathf.Infinity : Convert.ToSingle(subValues[i]);
                            variable.sub.Add(new ShaderVariable()
                            {
                                value = value
                            });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                                
                    }
                }
            }

            shaderVariables = variables;
            return false;
        }
    }
}