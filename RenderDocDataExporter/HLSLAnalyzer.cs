using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
        private Dictionary<string, string> _texSamplerTuple;
        private List<string> _cbufferName;
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
            // string fileName = $"Shader_VS{shaderCodePair.id.vsid}_PS{shaderCodePair.id.psid}.hlsl";
            string fileName = $"Shader_VS{shaderCodePair.id.vsid}_PS{shaderCodePair.id.psid}";
            // File.WriteAllText(Path.Combine(path, fileName), "#####################    VSFILE\n"+vsCode + "\n\n\n\n" + "#####################    PSFILE\n" + psCode);
            File.WriteAllText(Path.Combine(path, fileName+".shader"), GetCombinedURPShader(fileName));
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
            _texSamplerTuple = new Dictionary<string, string>();
            HashSet<string> _v2fIndex = new HashSet<string>();
            _cbufferName = new List<string>();
            
            if (String.IsNullOrEmpty(filePath)) return "";
            int bindingIndexCounter = 0;
            using (System.IO.StreamReader file = new System.IO.StreamReader(filePath))
            {
                int indent = 0;
                while (!file.EndOfStream)
                {
                    string line = file.ReadLine();

                    if (line.StartsWith("#"))
                    {
                        //这里是define
                        output += "         "+line + '\n';
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
                        output += "         "+line + '\n';
                    }
                    else if (line.StartsWith("cbuffer"))
                    {
                        //这里是cbuffer
                        output += StartExtractCBufferDefinition(passDef == ShaderPassDef.Pixel, file, filePath, bindingIndexCounter, ref line, out string cbufferName);
                        _cbufferName.Add(cbufferName);
                        bindingIndexCounter++;
                    }
                    else if (line.StartsWith("struct"))
                    {
                        //这里是结构体
                        if (passDef == ShaderPassDef.Vertex && line.Contains("SPIRV_Cross_Output"))
                        {
                            output += "         "+line + '\n';
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
                                output += "         "+line + '\n';
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
                                    if (split[3].Contains("TEXCOORD"))
                                    {
                                        _varingTuple.TryGetValue(split[3], out string oldName);
                                        _varingTuple.Remove(split[3]);
                                        _varingTuple.Add(split[1], oldName);
                                    }
                                }
                            }
                            line = file.ReadLine();
                        }
                        output += "         "+line + '\n';
                    }
                    else if (line.StartsWith("void"))
                    {
                        //这里是函数
                        endDefinition = true;
                        output += "         "+"inline " + line + '\n';
                    }
                    else if (line.StartsWith("uniform"))
                    {
                        //这里是采样器定义(旧版本)
                        string[] split = line.Split(" ");
                        string old = split[2].Replace(";", "");
                        line = line.Replace(old, $"{prefix}_s{old}");
                        tempSamplerName.TryAdd(old, $"{prefix}_s{old}");
                        output += "         "+line + '\n';
                    }
                    else if (line.StartsWith("Texture2D"))
                    {
                        //这里是纹理定义
                        string[] splitTexLine = line.Split(" ");
                        string newTexDef = prefix + "_t" + splitTexLine[1];
                        line = line.Replace(splitTexLine[1], newTexDef);
                        output += "         "+line + '\n';
                        
                        line = file.ReadLine();
                        string[] splitSamplerLine = line.Split(" ");
                        // string newSamplerDef = prefix + "_s" + splitSamplerLine[1];
                        // line = line.Replace(splitSamplerLine[1], newSamplerDef);
                        output += "         "+line + '\n';
                        _texSamplerTuple.Add(splitTexLine[1], splitSamplerLine[1]);
                    }
                    else if (line == "\n")
                    {
                        //这里是间隔
                        output += "         "+line + '\n';
                    }
                    else if (line.Contains(" main("))
                    {
                        output += "         "+line.Replace(" main(", $" {passDef}_main(");
                    }
                    else
                    {
                        if (endDefinition)
                        {
                            line = ReplaceSame(line);
                            // for (int i = 0; i < _replaceTuple.Length; i++)
                            // {
                                // line = line.Replace(_replaceTuple[i].Item1, _replaceTuple[i].Item2);
                            // }
                            line = ReplaceDefinition(ref line);
                        }
                        output += "         "+line + '\n';
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

            foreach (var pair in _texSamplerTuple)
            {
                output = output.Replace(" "+pair.Key+".", " "+prefix + "_t" + pair.Key+".");
                output = output.Replace(pair.Value, " sampler_"+prefix + "_t" + pair.Key);
            }
            return output;

            string ReplaceDefinition(ref string line)
            {
                foreach (var pair in tempVarName)
                {
                    // string pattern = $@"([\w' '()-,=]*[\w' '()-,=]*){pair.Key}([\w' '()-.;,=]*)";
                    // string replacement = "$1"+pair.Value+"$2";
                    // line = Regex.Replace(line, pattern, replacement);
                    line = line.Replace(" "+pair.Key+".", " "+pair.Value+".");
                    line = line.Replace(" "+pair.Key+" ", " "+pair.Value+" ");
                    line = line.Replace(" "+pair.Key+";", " "+pair.Value+";");
                    line = line.Replace(" "+pair.Key+")", " "+pair.Value+")");
                    line = line.Replace(" "+pair.Key+",", " "+pair.Value+",");
                    line = line.Replace(" "+pair.Key+"[", " "+pair.Value+"[");
                    line = line.Replace("   "+pair.Key+"[", "   "+pair.Value+"[");
                    line = line.Replace("-"+pair.Key+".", "-"+pair.Value+".");
                    line = line.Replace("-"+pair.Key+" ", "-"+pair.Value+" ");
                    line = line.Replace("-"+pair.Key+")", "-"+pair.Value+")");
                    line = line.Replace("-"+pair.Key+";", "-"+pair.Value+";");
                    line = line.Replace("("+pair.Key+".", "("+pair.Value+".");
                    line = line.Replace("("+pair.Key+" ", "("+pair.Value+" ");
                    line = line.Replace("("+pair.Key+",", "("+pair.Value+",");
                    line = line.Replace("("+pair.Key+")", "("+pair.Value+")");
                    line = line.Replace("["+pair.Key+".", "["+pair.Value+".");
                    line = line.Replace("["+pair.Key+" ", "["+pair.Value+" ");
                    line = line.Replace("!"+pair.Key+" ", "!"+pair.Value+" ");
                    line = line.Replace("!"+pair.Key+")", "!"+pair.Value+")");
                    line = line.Replace("!"+pair.Key+";", "!"+pair.Value+";");
                    line = line.Replace("!"+pair.Key+".", "!"+pair.Value+".");
                    line = line.Replace("!"+pair.Key+",", "!"+pair.Value+",");
                }

                foreach (var pair in tempSamplerName)
                {
                    line = line.Replace(" "+pair.Key, " "+pair.Value);
                    line = line.Replace("("+pair.Key, "("+pair.Value);
                    line = line.Replace("-"+pair.Key, "-"+pair.Value);
                }
                
                for (int i = 0; i < _cbufferName.Count; i++)
                {
                    if (passDef == ShaderPassDef.Pixel && isLinkedCBuffer(_cbufferName[i]))
                    {
                        line = line.Replace("_"+_cbufferName[i] + "_m", "v_"+_cbufferName[i] + "_m");
                    }
                    else
                    {
                        line = line.Replace("_"+_cbufferName[i] + "_m", prefix + "_"+_cbufferName[i] + "_m");
                    }
                }
                return line;
            }
        }

        private bool isLinkedCBuffer(string name)
        {
            foreach (var mapper in _bufferLinkerMapper)
            {
                if (mapper.Key.uniformIndex == int.Parse(name)) return true;
            }
            return false;
        }
        

        private string ReplaceSame(string line)
        {
            for (int i = 0; i < _replaceTuple.Length; i++)
            {
                line = line.Replace(_replaceTuple[i].Item1, "v"+_replaceTuple[i].Item2);
            }
            return line;
        }

        private string StartExtractCBufferDefinition(bool needReplaceBufferName, StreamReader file, string filePath, int bindingIndexCounter,  ref string line, out string cbufferName)
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

            cbufferName = bufferName[2];
            bool skip = false;
            foreach (var mapper in _bufferLinkerMapper)
            {
                //uniformIndex一致不代表内容一致，buffer序号可能也是一致的，但是bufferRange和offer可能不一致
                if (mapper.Key.uniformIndex == int.Parse(linkedBufferName))
                {
                    skip = true;
                    Debug.Log($" Pixelshader {filePath} has same cbuffer with it's binded vertex shader, skip output. index:F{mapper.Key.uniformIndex}->V{mapper.Value.uniformIndex}");
                    break;
                }
            }

            if (!skip)
            {
                line = line.Replace(split[3], $"register(b{bindingIndexCounter})");
                output += "         "+line + '\n';
            }

            string prefix = needReplaceBufferName ? "p" : "v";
            while (!line.Contains("}"))
            {
                line = file.ReadLine();
                if (!skip)
                {
                    line = line.Replace("_"+cbufferName + "_m", prefix + "_"+cbufferName + "_m");
                    output += "         "+line + '\n';
                }
            }
            return output;
        }
        
        string GetCombinedURPShader(string fileName)
        {
            string template = "";
            template += "Shader\"Capture/"+ fileName+"\"\n"+
                        "{\n";
            template += "    Properties\n" +
                        "    {\n" + 
                        "        _MainTex (\"Texture\", 2D) = \"white\" {}\n" + 
                        "    }\n";
            template += "    SubShader\n" + 
                        "    {\n";
            template += "        Tags { \"RenderType\"=\"Opaque\" }\n" +
                        "        LOD 100\n\n";
            template += "        Pass\n" + 
                        "        {\n";
            template += "            HLSLPROGRAM\n" + 
                        "            #pragma vertex Vertex_main\n" + 
                        "            #pragma fragment Pixel_main\n" + 
                        "            #include \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl\"\n\n";
            template += "               //########### VSCODE ##########\n";
            template += vsCode + '\n';
            template += "               //########### PSCODE ##########\n";
            template += psCode + '\n';
            template += "               //########### END ##########\n";
            template += "            ENDHLSL\n" +
                        "        }\n" +
                        "    }\n" +
                        "}\n";
            return template;
        }
    }
}