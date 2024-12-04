using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Moonflow
{
    public class HLSLAnalyzer
    {
        public ShaderCodePair shaderCodePair;
        public string vsCode;
        public string psCode;
        private CaptureAnalyzer _captureAnalyzer;
        private Dictionary<BufferLinker, BufferLinker> _bufferLinkerMapper = new Dictionary<BufferLinker, BufferLinker>();
        private Tuple<string, string>[] _replaceTuple;
        private Dictionary<string, string> _varingTuple;
        public void Setup(CaptureAnalyzer cap, ShaderCodePair codepairValue)
        {
            _captureAnalyzer = cap;
            shaderCodePair = codepairValue;
        }

        public void Analyze()
        {
            _varingTuple = new Dictionary<string, string>();
            AnalyzeCommonBuffer();
            vsCode = AnalyzeSinglePass(ShaderPassDef.Vertex, shaderCodePair.vsFilePath);
            psCode = AnalyzeSinglePass(ShaderPassDef.Pixel, shaderCodePair.psFilePath);
        }

        public void SaveAsFile(string path)
        {
            string fileName = $"Shader_VS{shaderCodePair.id.vsid}_PS{shaderCodePair.id.psid}.hlsl";
            File.WriteAllText(Path.Combine(path, fileName), "#####################    VSFILE\n"+vsCode + "\n\n\n\n" + "#####################    PSFILE\n" + psCode);
            Debug.Log($"Shader {shaderCodePair.id.vsid} {shaderCodePair.id.psid} saved.");
        }

        private void AnalyzeCommonBuffer()
        {
            var bufferlinkers = shaderCodePair.bufferLinkersExample;
            var vsLinkers = new List<BufferLinker>();
            var psLinkers = new List<BufferLinker>();
            foreach (var linker in bufferlinkers)
            {
                if (linker.passDef == ShaderPassDef.Vertex)
                    vsLinkers.Add(linker);
                else if (linker.passDef == ShaderPassDef.Pixel)
                    psLinkers.Add(linker);    
            }

            for (int i = 0; i < vsLinkers.Count; i++)
            {
                var vsLinker = vsLinkers[i];
                for (int j = 0; j < psLinkers.Count; j++)
                {
                    var psLinker = psLinkers[j];
                    if (vsLinker.bufferDec.bufferName == psLinker.bufferDec.bufferName &&
                        vsLinker.bufferDec.offset == psLinker.bufferDec.offset)
                    {
                        _bufferLinkerMapper.TryAdd(psLinker, vsLinker);
                    }
                }
            }
            _replaceTuple = new Tuple<string, string>[_bufferLinkerMapper.Count];
            int t = 0;
            foreach (var mapper in _bufferLinkerMapper)
            {
                _replaceTuple[t] = new Tuple<string, string>($"_{mapper.Key.uniformIndex.ToString()}_m", $"_{mapper.Value.uniformIndex.ToString()}_m");
                t++;
            }
        }

        private string AnalyzeSinglePass(ShaderPassDef passDef, string filePath)
        {
            string output = "";
            string prefix = passDef == ShaderPassDef.Pixel ? "p" : "v";
            bool endDefinition = false;
            Dictionary<string, string> tempVarName = new Dictionary<string, string>();
            Dictionary<string, string> tempSamplerName = new Dictionary<string, string>();
            HashSet<string> _v2fIndex = new HashSet<string>();
            if (String.IsNullOrEmpty(filePath)) return "";
            using (System.IO.StreamReader file = new System.IO.StreamReader(filePath))
            {
                int indent = 0;
                while (!file.EndOfStream)
                {
                    string line = file.ReadLine();

                    if (line.StartsWith("#"))
                    {
                        //这里是define
                        output += line + '\n';
                    }
                    else if (line.StartsWith("static"))
                    {
                        //这里是临时变量
                        string[] split = line.Split(" ");
                        if (split.Length == 3)
                        {
                            string old = split[2].Replace(";", "");
                            line = line.Replace(old, $"{prefix}{old}");
                            tempVarName.TryAdd(old, $"{prefix}{old}");
                        }
                        output += line + '\n';
                    }
                    else if (line.StartsWith("cbuffer"))
                    {
                        //这里是cbuffer
                        output += StartExtractCBufferDefinition(passDef == ShaderPassDef.Pixel, file, filePath,ref line);
                    }
                    else if (line.StartsWith("struct"))
                    {
                        //这里是结构体
                        if (passDef == ShaderPassDef.Vertex && line.Contains("SPIRV_Cross_Output"))
                        {
                            output += line + '\n';
                            while (!line.Contains("}"))
                            {
                                line = file.ReadLine();
                                string[] split = line.Trim().Split(" ");
                                if (split.Length == 4)
                                {
                                    if (split[3].Contains("TEXCOORD"))
                                    {
                                        line = line.Replace(split[1], "v2f" + split[1]);
                                        _varingTuple.TryAdd(split[3], split[1]);
                                        _v2fIndex.Add(split[1]);
                                    }
                                }
                                output += line + '\n';
                            }
                            line = file.ReadLine();
                        }
                        if (passDef == ShaderPassDef.Pixel && line.Contains("SPIRV_Cross_Input"))
                        {
                            // output += line + '\n';
                            while (!line.Contains("}"))
                            {
                                line = file.ReadLine();
                                string[] split = line.Trim().Split(" ");
                                if (split.Length == 4)
                                {
                                    _varingTuple.TryGetValue(split[3], out string oldName);
                                    _varingTuple.Remove(split[3]);
                                    _varingTuple.Add(split[1], oldName);
                                }
                            }
                            line = file.ReadLine();
                        }
                        output += line + '\n';
                    }
                    else if (line.StartsWith("void"))
                    {
                        //这里是函数
                        endDefinition = true;
                        output += line + '\n';
                    }
                    else if (line.StartsWith("uniform"))
                    {
                        //这里是采样器定义
                        string[] split = line.Split(" ");
                        string old = split[2].Replace(";", "");
                        line = line.Replace(old, $"{prefix}_s{old}");
                        tempSamplerName.TryAdd(old, $"{prefix}_s{old}");
                        output += line + '\n';
                    }
                    else if (line == "\n")
                    {
                        //这里是间隔
                        output += line + '\n';
                    }
                    else
                    {
                        if (endDefinition)
                        {
                            line = ReplaceDefinition(ref line);
                        }
                        output += ReplaceSame(line) + '\n';
                    }
                }
            }

            output = output.Replace("f.xxxx", "").Replace("f.xxx", "").Replace("f.xx", "");
            if (passDef == ShaderPassDef.Vertex)
            {
                output = output.Replace("SPIRV_Cross_Input", "Attribute");
                output = output.Replace("SPIRV_Cross_Output", "Varying");
                foreach (var oldName in _v2fIndex)
                {
                    output = output.Replace("." + oldName, ".v2f" + oldName);
                }
            }
            else
            {
                output = output.Replace("SPIRV_Cross_Input", "Varying");
                output = output.Replace("SPIRV_Cross_Output", "Output");
                output = output.Replace("gl_FragCoord", "gl_Position");
                foreach (var vaying in _varingTuple)
                {
                    output = output.Replace("."+vaying.Key, ".v2f"+vaying.Value);
                }
            }
            return output;

            string ReplaceDefinition(ref string line)
            {
                foreach (var pair in tempVarName)
                {
                    line = line.Replace(" "+pair.Key, " "+pair.Value);
                    line = line.Replace("("+pair.Key, "("+pair.Value);
                    line = line.Replace("-"+pair.Key, "-"+pair.Value);
                }

                foreach (var pair in tempSamplerName)
                {
                    line = line.Replace(" "+pair.Key, " "+pair.Value);
                    line = line.Replace("("+pair.Key, "("+pair.Value);
                    line = line.Replace("-"+pair.Key, "-"+pair.Value);
                }

                return line;
            }
        }
        
        

        private string ReplaceSame(string line)
        {
            for (int i = 0; i < _replaceTuple.Length; i++)
            {
                line = line.Replace(_replaceTuple[i].Item1, _replaceTuple[i].Item2);
            }
            return line;
        }

        private string StartExtractCBufferDefinition(bool needReplaceBufferName, StreamReader file, string filePath, ref string line)
        {
            string output = "";
            //skip first line, but use to find the buffer name
            string[] split = line.Split(" ");
            string[] bufferName = split[1].Split("_");
            string linkedBufferName = "";
            if (bufferName.Length != 3)
            {
                Debug.LogError($"Unknown cbuffer name defined, from:{filePath}, line:{line}");
            }
            else
            {
                linkedBufferName = bufferName[2];
            }
            
            bool skip = false;
            foreach (var mapper in _bufferLinkerMapper)
            {
                if (mapper.Key.uniformIndex == int.Parse(linkedBufferName))
                {
                    skip = true;
                    Debug.Log($" Pixelshader {filePath} has same cbuffer with it's binded vertex shader, skip output. index:F{mapper.Key.uniformIndex}->V{mapper.Value.uniformIndex}");
                    break;
                }
            }
            if(!skip)output += line + '\n';

            while (!line.Contains("}"))
            {
                line = file.ReadLine();
                if(!skip) output += line + '\n';
            }
            return output;
        }
    }
}