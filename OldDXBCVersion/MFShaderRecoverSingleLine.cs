using System;
using System.Collections.Generic;

namespace moonflow_system.Tools.MFUtilityTools
{
    public class SingleLine
    {
        public int lineIndex;
        public string str;
        public shaderPropUsage result;
        public int opIndex;
        public bool selfCal;
        public bool saturate;
        public bool elipsised;
        public int combineState;//0:没动 1:省略了 2：合并了
        
        public bool empty;
        public shaderPropUsage[] localVar;
        public bool opArranged;
        public bool noEqualSign;
    }
    
        public struct ShaderData
    {
        public List<List<shaderPropDefinition>> vertProps;
        public List<List<shaderPropDefinition>> fragProps;
        public List<shaderPropDefinition> attribute;
        public List<shaderPropDefinition> buffer;
        // public List<shaderPropDefinition> v2g;
        // public List<shaderPropDefinition> g2f;
        public List<shaderPropDefinition> v2f;
        public List<shaderPropDefinition> gbuffer;
        public List<shaderPropDefinition> tex;
        public List<SingleLine> vert;
        public string geometry;
        public List<SingleLine> frag;
        public List<shaderPropUsage> tempVertexVar;
        public List<shaderPropUsage> tempPixelVar;
    }

    public class shaderPropDefinition
    {
        public string type;
        public string name;
        public string def;
    }

    

    public class shaderPropUsage
    {
        public shaderPropDefinition linkedVar;
        public string channel;
        public bool negative;
        public int inlineOp;
        public bool additional;
        public string GetDisplayVar(bool needNegative = true)
        {
            string result = "";
            if (negative && needNegative) result += "-";
            if (inlineOp == -1)
            {
                try
                {
                    result += $"{linkedVar.name}.{channel}";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }else if (inlineOp == 0)
            {
                if (linkedVar == null)
                {
                    // if (channel.Contains('.'))
                    // {
                    //     result += channel.TrimEnd('0').TrimEnd('.');
                    // }
                    // else
                    // {
                        string[] split = channel.Split(",");
                        result += (split.Length > 1) ? $"float{split.Length}({channel})":$"{channel}" ;
                    // }
                }
                else
                {
                    string[] split = channel.Split(",");
                    var single= "";
                    for (int i = 0; i < split.Length; i++)
                    {
                        split[i] = split[i].TrimEnd('0').TrimEnd('.');
                        single += $"{split[i]}";
                        if (i < split.Length - 1)
                        {
                            single += ",";
                        }
                    }
                    // channel += ")";
                    result += $"{linkedVar.type}({single})";
                }
            }else if (inlineOp == 1)
            {
                result += $"abs({linkedVar.name}.{channel})";
            }

            return result;
        }
    }
}