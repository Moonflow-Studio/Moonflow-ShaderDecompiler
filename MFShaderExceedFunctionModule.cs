using System;
using System.Collections.Generic;
using System.Linq;

namespace moonflow_system.Tools.MFUtilityTools
{
    public class MFShaderExceedFunctionModule
    {
        private int searchStack = 0;
        private int startline = 0;
        private int type = 0;
        private int amount = 0;
        
        //type 0
        //  85: add r2.xyz, cb0[9].xyzx, -cb0[10].xyzx
        //  86: mad r2.xyz, r0.zzzz, r2.xyzx, cb0[10].xyzx
        // r2.xyz = lerp(cb0[9].xyz, cb0[10].xyz, r0.z)
        
        //type 1
        // 78: mad r1.y, r1.y, cb0[26].w, -cb0[26].w
        // 79: add r1.y, r1.y, l(1.0000)
        // r1.y = lerp(r1.y, 1, cb0[26].w)
        public void Merge_Lerp(ref List<SingleLine> lines)
        {
            startline = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (searchStack == 1)
                {
                    var lastline = lines[i - 1];
                    if (line.opIndex == 52 && type == 0)
                    {
                       
                        if ((lastline.localVar[0].negative &&
                            ReferenceEquals(lastline.localVar[0].linkedVar, line.localVar[2].linkedVar))
                            || (lastline.localVar[1].negative && ReferenceEquals(lastline.localVar[1].linkedVar,
                                line.localVar[2].linkedVar)))
                        {
                            if (ReferenceEquals(lastline.result.linkedVar, line.result.linkedVar))
                            {
                                if (lastline.result.channel == line.result.channel && line.selfCal)
                                {
                                    Make_Lerp(ref lines);
                                    searchStack = 0;
                                    continue;
                                }
                            }
                        }
                    }else if(line.opIndex == 0 && type == 1)
                    {
                        string text = "";
                        bool matched = false;
                        int constant = -1;
                        if (line.localVar[0].inlineOp == 0)
                        {
                            constant = 0;
                        }else if (line.localVar[1].inlineOp == 0)
                        {
                            constant = 1;
                        }
                        else
                        {
                            searchStack = 0;
                            continue;
                        }

                        try
                        {
                            if (ReferenceEquals(line.localVar[1-constant].linkedVar, lastline.result.linkedVar))
                            {
                                if (lastline.result.channel == line.localVar[1-constant].channel &&
                                    line.localVar[constant].inlineOp == 0)
                                {
                                    text = line.localVar[constant].channel;
                                    matched = true;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                        
                        if (matched)
                        {
                            var split = text.Split(",");
                            string v = split[0].Replace("(", "").Replace(")", "");
                            int value = (int)Convert.ToSingle(v);
                            if (value == 1)
                            {
                                Make_Lerp(ref lines);
                                searchStack = 0;
                                continue;
                            }
                        }
                    }
                }
                
                searchStack = 0;
                type = -1;
                //默认查第一行
                if (line.opIndex == 0 && line.localVar != null)
                {
                    if (line.localVar[0].negative || line.localVar[1].negative)
                    {
                        searchStack = 1;
                        type = 0;
                        startline = line.lineIndex;
                    }
                }else if (line.opIndex == 52 && line.localVar != null)
                {
                    if (line.localVar[2].negative && line.localVar[1].linkedVar == line.localVar[2].linkedVar)
                    {
                        searchStack = 1;
                        type = 1;
                        startline = line.lineIndex;
                    }
                }
                
            }
        }

        private void Make_Lerp(ref List<SingleLine> lines)
        {
            bool line1ValueSafe = SafeValueElipsise(lines, startline, 1, out shaderPropUsage line1Value);
            if (line1ValueSafe)
            {
                var singleLine = lines[startline];
                var line2 = lines[startline + 1];
                var oldvars = singleLine.localVar;
                singleLine.result = lines[startline + 1].result;
                singleLine.opIndex = 100;
                singleLine.localVar = new[] { oldvars[0], oldvars[1], line2.localVar[0] };
                singleLine.localVar[1].negative = false;
                lines[startline + 1].elipsised = true;
            }
        }

        public void Merge_LinearStep(ref List<SingleLine> lines)
        {

        }
        private void Make_LinearStep(ref List<SingleLine> lines)
        {
            
        }
        
        public void Merge_Smoothstep(ref List<SingleLine> lines)
        {
            startline = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (searchStack == 1)
                {
                    if (line.opIndex == 57 && line.localVar.Length == 2)
                    {
                        if (line.localVar[0].linkedVar == line.localVar[1].linkedVar)
                        {
                            searchStack = 2;
                            continue;
                        }
                    }
                }

                if (searchStack == 2)
                {
                    if (/*line.opIndex == 52 ||*/ line.opIndex == 57)
                    {
                        if ((line.localVar[0].linkedVar == lines[i - 1].result.linkedVar
                             && line.localVar[1].linkedVar == lines[i - 2].result.linkedVar)
                            || (line.localVar[1].linkedVar == lines[i - 1].result.linkedVar
                                && line.localVar[0].linkedVar == lines[i - 2].result.linkedVar))
                        {
                            Make_Smoothstep(ref lines);
                        }
                    }
                }
                
                //默认查第一行
                //第一行是mad
                if (line.opIndex == 52
                    && line.localVar.Length == 3)
                {
                    //第二第三个系数是常量
                    if (line.localVar[1].inlineOp == 0 && line.localVar[2].inlineOp == 0)
                    {
                        var text = line.localVar[1].channel;
                        var split = text.Split(",");
                        string v = split[0].Replace("(", "").Replace(")", "");
                        int value = (int)Convert.ToSingle(v);
                        //第二个系数需要为-2
                        if (value != 2 || !line.localVar[1].negative)
                        {
                            searchStack = 0;
                            continue;
                        }

                        bool matched = true;
                        for (int j = 1; j < split.Length; j++)
                        {
                            if ((int)Convert.ToSingle(split[j].Replace("(", "").Replace(")", "")) 
                                != (int)Convert.ToSingle(split[0].Replace("(", "").Replace(")", "")))
                            {
                                matched = false;
                                break;
                            }
                        }

                        if (!matched)
                        {
                            searchStack = 0;
                            continue;
                        }
                        text = line.localVar[2].channel;
                        split = text.Split(",");
                        v = split[0].Replace("(", "").Replace(")", "");
                        value = (int)Convert.ToSingle(v);
                        if (value != 3)
                        {
                            searchStack = 0;
                            continue;
                        }
                        for (int j = 1; j < split.Length; j++)
                        {
                            if ((int)Convert.ToSingle(split[j].Replace("(", "").Replace(")", "")) 
                                != (int)Convert.ToSingle(split[0].Replace("(", "").Replace(")", "")))
                            {
                                matched = false;
                                break;
                            }
                        }
                        if (!matched)
                        {
                            searchStack = 0;
                            continue;
                        }
                        searchStack = 1;
                        startline = line.lineIndex;
                        continue;
                    }
                }
            }
        }

        private void Make_Smoothstep(ref List<SingleLine> lines)
        {
            bool line1ValueSafe = SafeValueElipsise(lines, startline, 2, out shaderPropUsage line1Value);
            bool line2ValueSafe = SafeValueElipsise(lines, startline + 1, 1, out shaderPropUsage line2Value);
            if (line1ValueSafe && line2ValueSafe)
            {
                var singleLine = lines[startline];
                var oldvar = singleLine.localVar[0];
                singleLine.result = lines[startline + 2].result;
                singleLine.opIndex = 102;
                singleLine.localVar = new[] { oldvar };
                lines[startline + 1].elipsised = true;
                lines[startline + 2].elipsised = true;
            }
        }

        public void Merge_Normalize(ref List<SingleLine> lines){}
        private void Make_Normalize(ref List<SingleLine> lines){}

        public void Merge_Pow4(ref List<SingleLine> lines)
        {
            searchStack = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (searchStack == 1)
                {
                    if (line.opIndex == 57)
                    {
                        if (line.localVar[1].channel.Contains(line.localVar[0].channel) &&
                            ReferenceEquals(line.localVar[0].linkedVar, line.localVar[1].linkedVar))
                        {
                            var lastLine = lines[i - 1];
                            if (line.localVar[0].channel == lastLine.result.channel &&
                                ReferenceEquals(line.localVar[0].linkedVar, lastLine.result.linkedVar))
                            {
                                Make_Pow4(ref lines);
                                searchStack = 0;
                                continue;
                            }
                        }
                    }
                }

                searchStack = 0;
                if (line.opIndex == 57)
                {
                    if (line.localVar[1].channel.Contains(line.localVar[0].channel) &&
                        ReferenceEquals(line.localVar[0].linkedVar, line.localVar[1].linkedVar))
                    {
                        searchStack = 1;
                        startline = i;
                    }
                }
                
            }
        }

        private void Make_Pow4(ref List<SingleLine> lines)
        {
            bool line1ValueSafe = SafeValueElipsise(lines, startline, 1, out shaderPropUsage line1Value);
            if (line1ValueSafe)
            {
                var singleLine = lines[startline];
                var oldvar = singleLine.localVar[0];
                singleLine.result = lines[startline + 1].result;
                singleLine.opIndex = 104;
                singleLine.localVar = new[] { oldvar };
                lines[startline + 1].elipsised = true;
            }
        }

        public void Merge_MatrixMultiply(ref List<SingleLine> lines, ref List<List<shaderPropDefinition>> properties)
        {
            startline = 0;
            amount = 0;
            shaderPropDefinition inputVector = null;
            shaderPropUsage[] matrix = new shaderPropUsage[4];
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (searchStack is 1 or 2)
                {
                    if (line.opIndex == 52)
                    {
                        if (ReferenceEquals(line.result.linkedVar, lines[i - 1].result.linkedVar) &&
                            line.result.channel == lines[i - 1].result.channel && line.selfCal)
                        {
                            if (line.localVar[1].channel.Length == 1 &&
                                ReferenceEquals(line.localVar[1].linkedVar, inputVector))
                            {
                                matrix[GetLineIndex(line.localVar[1].channel[0])] = line.localVar[0];
                                searchStack++;
                                continue;
                            }
                        }
                    }
                }

                if (searchStack == 3)
                {
                    if (line.opIndex == 52)
                    {
                        if (ReferenceEquals(line.result.linkedVar, lines[i - 1].result.linkedVar) &&
                            line.result.channel == lines[i - 1].result.channel && line.selfCal)
                        {
                            if (line.localVar[1].channel.Length == 1 &&
                                ReferenceEquals(line.localVar[1].linkedVar, inputVector))
                            {
                                matrix[GetLineIndex(line.localVar[1].channel[0])] = line.localVar[0];
                                Make_MatrixMultiply(ref lines, inputVector, matrix, ref properties);
                                searchStack = 0;
                                continue;
                            }
                        }
                    }

                    if (line.opIndex == 0)
                    {
                        if (ReferenceEquals(line.result.linkedVar, lines[i - 1].result.linkedVar) &&
                            line.result.channel == lines[i - 1].result.channel && line.selfCal)
                        {
                            if (ReferenceEquals(line.localVar[0].linkedVar, line.result.linkedVar))
                            {
                                matrix[3] = line.localVar[1];
                                Make_MatrixMultiply(ref lines, inputVector, matrix, ref properties);
                                searchStack = 0;
                                continue;
                            }
                        }
                    }
                }

                searchStack = 0;
                if (line.opIndex == 57)
                {
                    if (line.localVar[0].channel.Length == 1)
                    {
                        inputVector = line.localVar[0].linkedVar;
                        matrix[GetLineIndex(line.localVar[0].channel[0])] = line.localVar[1];
                        searchStack = 1;
                    }else if (line.localVar[1].channel.Length == 1)
                    {
                        inputVector = line.localVar[1].linkedVar;
                        matrix[GetLineIndex(line.localVar[0].channel[0])] = line.localVar[0];
                        searchStack = 1;
                    }
                    else
                    {
                        inputVector = null;
                        matrix = new shaderPropUsage[4];
                    }
                    startline = i;
                }

                int GetLineIndex(char channel)
                {
                    switch (channel)
                    {
                        case 'x': return 0;
                        case 'y': return 1;
                        case 'z': return 2;
                        case 'w': return 3;
                    }
                    return -1;
                }
            }
        }

        private void Make_MatrixMultiply(ref List<SingleLine> lines, shaderPropDefinition inputVec,
            shaderPropUsage[] matrix, ref List<List<shaderPropDefinition>> properties)
        {
            var singleLine = lines[startline];
            singleLine.opIndex = 105;
            singleLine.localVar = new[]
            {
                new shaderPropUsage() { linkedVar = inputVec, channel = singleLine.result.channel.Length == 3 ? "xyz":"xyzw", inlineOp = -1},
                matrix[0],
                matrix[1],
                matrix[2],
                matrix[3],
            };
            lines[startline + 1].elipsised = true;
            lines[startline + 2].elipsised = true;
            lines[startline + 3].elipsised = true;
            matrix[0].linkedVar.name = $"prop_Matrix_{amount.ToString()}";
            matrix[0].linkedVar.type = "float4x4";
            matrix[0].channel = "_m00_m01_m02_m03";

            RelinkMatrixProp(ref lines, matrix[1].linkedVar, matrix[0].linkedVar,1, ref properties);
            RelinkMatrixProp(ref lines, matrix[2].linkedVar, matrix[0].linkedVar,2, ref properties);
            RelinkMatrixProp(ref lines, matrix[3].linkedVar, matrix[0].linkedVar,3, ref properties);
            // matrix[1].linkedVar = matrix[0].linkedVar;
            // matrix[1].channel = "[1]";
            //
            // matrix[2].linkedVar = matrix[0].linkedVar;
            // matrix[2].channel = "[2]";
            //
            // matrix[3].linkedVar = matrix[0].linkedVar;
            // matrix[3].channel = "[3]";
            
            amount++;
        }

        private void RelinkMatrixProp(ref List<SingleLine> lines, shaderPropDefinition from, shaderPropDefinition to, int i, ref List<List<shaderPropDefinition>> properties)
        {
            for (int j = startline; j < lines.Count; j++)
            {
                var line = lines[j];
                if (line.localVar == null) continue;
                for (int k = 0; k < line.localVar.Length; k++)
                {
                    if (ReferenceEquals(line.localVar[k].linkedVar, from))
                    {
                        string s = i.ToString();
                        line.localVar[k].linkedVar = to;
                        line.localVar[k].channel = $"[_m{s}0_m{s}1_m{s}2_m{s}3]";
                    }
                }
            }

            bool replaced = false;
            for (int j = 0; j < properties.Count; j++)
            {
                for (int k = 0; k < properties[j].Count; k++)
                {
                    if (ReferenceEquals(properties[j][k], from))
                    {
                        properties[j].Remove(from);
                        replaced = true;
                        break;
                    }
                }

                if (replaced)
                {
                    break;
                }
            }
            
        }

        public void Merge_Clamp(ref List<SingleLine> lines)
        {
            searchStack = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.opIndex == 54 && searchStack == 1)
                {
                    var last = lines[i - 1];
                    if (ReferenceEquals(line.localVar[0].linkedVar, last.result.linkedVar) && line.localVar[0].channel == last.result.channel)
                    {
                        Make_Clamp(ref lines, 0);
                        searchStack = 0;
                        continue;
                    }
                    if (ReferenceEquals(line.localVar[1].linkedVar, last.result.linkedVar) && line.localVar[1].channel == last.result.channel)
                    {
                        Make_Clamp(ref lines, 1);
                        searchStack = 0;
                        continue;
                    }
                }

                searchStack = 0;
                if (line.opIndex == 53)
                {
                    searchStack = 1;
                    startline = i;
                }
            }
        }

        private void Make_Clamp(ref List<SingleLine> lines, int replaceSerial)
        {
            if (SafeValueElipsise(lines, startline, 1, out shaderPropUsage e))
            {
                var last = lines[startline];
                var line = lines[startline + 1];
                last.elipsised = true;
                line.opIndex = 106;
                line.localVar = new[] { last.localVar[replaceSerial], line.localVar[1], last.localVar[1-replaceSerial] };
            }
        }
        private static bool SafeValueElipsise(List<SingleLine> lines, int resultLine, int offset, out shaderPropUsage value)
        {
            value = lines[resultLine].result;
            for (int i = resultLine + offset; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.localVar == null) continue;
                for (int j = 0; j < line.localVar.Length; j++)
                {
                    if (ReferenceEquals(line.localVar[j], value))
                    {
                        return false;
                    }
                }
                
                if (ReferenceEquals(line.result, value))
                {
                    break;
                }
            }
            return true;
        }
    }
}