using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonflow
{
    [Serializable]
    public class CBufferAnalyzer : ScriptableObject, IResourceReceiver
    {
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
            linker.uniformIndex = int.Parse(nameArray[4].Replace("uniforms",""));
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
                int subflag = 0;
                ShaderVariable tempVariable = null;
                while (!file.EndOfStream)
                {
                    string line = file.ReadLine();
                    // Debug.Log("ReadingLine: " + line);
                    if(line != null && line.Length ==0 && tempVariable!=null) variables.Add(tempVariable.Clone() as ShaderVariable);
                    // is sub
                    if (line.StartsWith(" "))
                    {
                        if (subflag == 0)
                        {
                            Debug.LogError($"子项出现在非列表变量中, path: {path}, line: {line}");
                            continue;
                        }
                        string trimline = line.Trim();
                        string[] splitted = trimline.Split("  ");
                        if (splitted.Length != 2)
                        {
                            Debug.LogError($"子项格式错误, path: {path}, line: {line}");
                        }
                        else
                        {
                            string subName = splitted[0];
                            string subValue = splitted[1];
                            //subName looks like _childName[{index}], so we need to get the index
                            int index = Convert.ToInt32(subName.Split("[")[1].Split("]")[0]);
                            //subValue splitted with ' ', so we need to get the value
                            string[] subValues = subValue.Split(" ");
                            if (subValues.Length != 4 && subValues.Length!= 1)
                            {
                                Debug.LogError($"子项值格式错误, path: {path}, line: {line}");
                            }
                            else
                            {
                                // tempVariable.values = new Vector4[1];
                                if (subValues.Length > 0)
                                {
                                    tempVariable.values[index].x = subValues[0] == "nan" ? Mathf.Infinity : Convert.ToSingle(subValues[0]);
                                    if (subValues.Length > 1)
                                    {
                                        tempVariable.values[index].y = subValues[1] == "nan" ? Mathf.Infinity :  Convert.ToSingle(subValues[1]);
                                        if (subValues.Length > 2)
                                        {
                                            tempVariable.values[index].z = subValues[2] == "nan" ? Mathf.Infinity :  Convert.ToSingle(subValues[2]);
                                            if (subValues.Length > 3)
                                            {
                                                tempVariable.values[index].w = subValues[3] == "nan" ? Mathf.Infinity :  Convert.ToSingle(subValues[3]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        subflag = 0;
                        if(tempVariable!=null)variables.Add(tempVariable.Clone() as ShaderVariable);
                        if (line.Contains("MEMBERS:"))
                        {
                            if (subflag != 0)
                            {
                                
                                Debug.Log($"存在嵌套");
                            }
                            tempVariable = new ShaderVariable();
                            tempVariable.name = line.Split("  ")[0].Trim();
                            int listCount = Convert.ToInt32(line.Trim().Split(":")[1]);
                            if (listCount > 64)
                            {
                                Debug.LogError($"Buffer seems like a bone matrix buffer, SKIP. path:{path}");
                                return new List<ShaderVariable>();
                            }
                            tempVariable.values = new Vector4[listCount];
                            subflag += 1;
                        }
                        else
                        {
                            tempVariable = new ShaderVariable();
                            string[] splitted = line.Split("  ");
                            tempVariable.name = splitted[0].Trim();
                            string[] data = splitted[1].Trim().Split(" ");
                            if (data.Length == 1)
                            {
                                tempVariable.value = Convert.ToSingle(data[0]);
                            }
                            else
                            {
                                try
                                {
                                    tempVariable.values = new Vector4[1];
                                    if (data.Length > 0)
                                    {
                                        tempVariable.values[0].x = data[0] == "nan" ? Mathf.Infinity : Convert.ToSingle(data[0]);
                                        if (data.Length > 1)
                                        {
                                            tempVariable.values[0].y = data[1] == "nan" ? Mathf.Infinity :  Convert.ToSingle(data[1]);
                                            if (data.Length > 2)
                                            {
                                                tempVariable.values[0].z = data[2] == "nan" ? Mathf.Infinity :  Convert.ToSingle(data[2]);
                                                if (data.Length > 3)
                                                {
                                                    tempVariable.values[0].w = data[3] == "nan" ? Mathf.Infinity :  Convert.ToSingle(data[3]);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    throw;
                                }
                                
                            }
                        }
                        
                    }
                    
                } 
            }
            return variables;
        }
    }
}