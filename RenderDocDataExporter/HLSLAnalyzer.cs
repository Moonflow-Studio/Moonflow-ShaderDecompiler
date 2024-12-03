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
        public void Setup(CaptureAnalyzer cap, ShaderCodePair codepairValue)
        {
            _captureAnalyzer = cap;
            shaderCodePair = codepairValue;
        }

        public void Analyze()
        {
            vsCode = AnalyzeSinglePass(ShaderPassDef.Vertex, shaderCodePair.vsFilePath);
        }

        private string AnalyzeSinglePass(ShaderPassDef passDef, string filePath)
        {
            string output = "";
            using (System.IO.StreamReader file = new System.IO.StreamReader(filePath))
            {
                int indent = 0;
                while (!file.EndOfStream)
                {
                    string line = file.ReadLine();

                    if (line.StartsWith("#"))
                    {
                        //这里是define
                    }
                    else if (line.StartsWith("static"))
                    {
                        //这里是固定输入
                    }
                    else if (line.StartsWith("cbuffer"))
                    {
                        //这里是cbuffer
                        StartExtractCBufferDefinition(file, ref line);
                    }
                    else if (line.StartsWith("struct"))
                    {
                        //这里是结构体
                    }
                    else if (line.StartsWith("void"))
                    {
                        //这里是函数
                    }
                    else if (line == "\n")
                    {
                        //这里是间隔
                    }
                }
            }

            return output;
        }

        private void StartExtractCBufferDefinition(StreamReader file, ref string line)
        {
            string[] split = line.Split(" ");
            string[] cbufferNameSplit = split[1].Split("_");
            string uniformIndex = cbufferNameSplit[^1];
        }
    }
}