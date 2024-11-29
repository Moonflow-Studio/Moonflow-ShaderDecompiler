using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonflow
{
    [Serializable]
    public class CBufferAnalyzer : ScriptableObject, IResourceReceiver
    {
        public List<BufferDeclaration> vBuffers = new List<BufferDeclaration>();
        public List<BufferDeclaration> pBuffers = new List<BufferDeclaration>();
        
        [Serializable]
        public struct BufferDeclaration
        {
            public int setIndex;
            public int bindingIndex;
            public string linkedFile;
            public string bufferName;
            public List<ShaderVariable> variables;
        }
        public void AddResource(string path)
        {
            Debug.Log($"Adding resource as CBufferFile: {path}");
            //get file name from path
            string fileName = System.IO.Path.GetFileName(path);
            //get file name without extension
            fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            
            //file name format is "CBuffer_<Pixel/Vertex>_s<SetIndex>_b<BindingIndex>_<bufferIndex>_<bufferID>_<bufferName>.txt"
            string[] nameArray = fileName.Split('_');
            BufferDeclaration buffer = new BufferDeclaration();
            buffer.linkedFile = path;
            buffer.setIndex = int.Parse(nameArray[2].Substring(1));
            buffer.bindingIndex = int.Parse(nameArray[3].Substring(1));
            buffer.bufferName = nameArray[6];
            buffer.variables = AnalyzeCBufferFile(path);
            if (nameArray[1] == "Pixel")
            {
                pBuffers.Add(buffer);
            }else if (nameArray[1] == "Vertex")
            {
                vBuffers.Add(buffer);
            }
            else
            {
                Debug.LogError($"Unknown buffer type: {nameArray[1]}, path: {path}");
            }
        }

        private List<ShaderVariable> AnalyzeCBufferFile(string path)
        {
            List<ShaderVariable> variables = new List<ShaderVariable>();
            Debug.Log("Analyzing CBuffer file: " + path);
            using (System.IO.StreamReader file = new System.IO.StreamReader(path))
            {
                bool subflag = false;
                ShaderVariable tempVariable = null;
                while (!file.EndOfStream)
                {
                    string line = file.ReadLine();
                    Debug.Log("ReadingLine: " + line);
                    if(line != null && line.Length ==0 && tempVariable!=null) variables.Add(tempVariable.Clone() as ShaderVariable);
                    // is sub
                    if (line.StartsWith("    "))
                    {
                        if (!subflag)
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
                                tempVariable.values[index] = new Vector4(
                                    Convert.ToSingle(subValues[0]),
                                    Convert.ToSingle(subValues[1]),
                                    Convert.ToSingle(subValues[2]),
                                    Convert.ToSingle(subValues[3])
                                );
                            }
                        }
                    }
                    else
                    {
                        if(tempVariable!=null)variables.Add(tempVariable.Clone() as ShaderVariable);
                        if (line.Contains("MEMBERS:"))
                        {
                            if (subflag)
                            {
                                Debug.LogError($"父项出现在子项中, path: {path}, line: {line}");
                            }
                            tempVariable = new ShaderVariable();
                            tempVariable.name = line.Split("  ")[0].Trim();
                            tempVariable.values = new Vector4[Convert.ToInt32(line.Split(":")[1].Trim())];
                            
                            subflag = true;
                        }
                        else
                        {
                            subflag = false;
                            tempVariable = new ShaderVariable();
                            string[] splitted = line.Split("  ");
                            tempVariable.name = splitted[0].Trim();
                            string[] data = splitted[1].Split(" ");
                            if (data.Length == 1)
                            {
                                tempVariable.value = Convert.ToSingle(data[0]);
                            }
                            else
                            {
                                tempVariable.values = new Vector4[1]
                                {
                                    new Vector4(
                                        Convert.ToSingle(data[0]),
                                        Convert.ToSingle(data[1]),
                                        Convert.ToSingle(data[2]),
                                        Convert.ToSingle(data[3])
                                        )
                                };
                            }
                        }
                        
                    }
                    
                } 
            }
            return variables;
        }
    }
}