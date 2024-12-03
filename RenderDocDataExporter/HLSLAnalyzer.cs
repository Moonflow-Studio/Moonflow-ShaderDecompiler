using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
        public void Setup(CaptureAnalyzer cap, ShaderCodePair codepairValue)
        {
            _captureAnalyzer = cap;
            shaderCodePair = codepairValue;
        }

        public void Analyze()
        {
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
                        //这里是固定输入
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
                        output += line + '\n';
                    }
                    else if (line.StartsWith("void"))
                    {
                        //这里是函数
                        
                        output += line + '\n';
                    }
                    else if (line == "\n")
                    {
                        //这里是间隔
                        output += line + '\n';
                    }
                    else
                    {
                        output += ReplaceSame(line) + '\n';
                    }
                }
            }
            return output;
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