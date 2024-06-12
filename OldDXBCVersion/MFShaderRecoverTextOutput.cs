using System.Collections.Generic;

namespace moonflow_system.Tools.MFUtilityTools
{
    public static class MFShaderRecoverTextOutput
    {
        public static string MakeProperty(ShaderData data, bool isFrag = false)
        {
            string property = "";
            var list = isFrag ? data.fragProps : data.vertProps;
            for (int i = 0; i < list.Count; i++)
            {
                var buffer = list[i];
                for (int j = 0; j < buffer.Count; j++)
                {
                    var prop = buffer[j];
                    property += $"        {prop.name}(\"{prop.name}\", Vector) = (0,0,0,0)\n";
                }
            }

            return property;
        }

        public static string MakePropertyDefine(ShaderData data, bool isFrag = false)
        {
            string property = "";
            var list = isFrag ? data.fragProps : data.vertProps;
            for (int i = 0; i < list.Count; i++)
            {
                var buffer = list[i];
                for (int j = 0; j < buffer.Count; j++)
                {
                    var prop = buffer[j];
                    property += $"            {prop.type} {prop.name};\n";
                }
            }

            return property;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type">0:attribute 1:v2f 2:gbuffer</param>
        /// <returns></returns>
        public static string MakeStruct(ShaderData data, int type)
        {
            string property = "";
            List<shaderPropDefinition> list = null;
            switch (type)
            {
                case 0:
                    list = data.attribute;
                    break;
                case 1:
                    list = data.v2f;
                    break;
                case 2:
                    list = data.gbuffer;
                    break;
            }

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var attr = list[i];
                    property += $"                {attr.type} {attr.name} : {attr.def};\n";
                }
            }

            return property;
        }

        public static string MakeTexProperty(ShaderData data)
        {
            string property = "";
            List<shaderPropDefinition> list = data.tex;
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var tex = list[i];
                    property += $"      {tex.name}(\"{tex.name}\", 2D) = \"white\"\n";
                }
            }

            return property;
        }

        public static string MakeTexBuffer(ShaderData data)
        {
            string property = "";
            List<shaderPropDefinition> list = data.tex;
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var tex = list[i];
                    property += $"            {tex.type} {tex.name};\n";
                    property += $"            SamplerState sampler{tex.name};\n";
                    property += $"\n";
                }
            }

            return property;
        }

        public static string MakeURPText(ShaderData data, string vertText, string fragText, string shaderName)
        {
            string text = "Shader\"" + shaderName + "\"\n";
            text += "{\n";
            text += "    Properties\n";
            text += "    {\n";
            text += MakeProperty(data);
            text += MakeProperty(data, true);
            text += MakeTexProperty(data);
            text += "    }\n" +
                    "    SubShader\n" +
                    "    {\n" +
                    "        Tags{\"RenderType\" = \"Opaque\"}\n\n" +
                    "        Pass\n" +
                    "        {\n" +
                    "            HLSLPROGRAM\n" +
                    "            #pragma vertex vert\n" +
                    "            #pragma fragment frag\n" +
                    "            #include \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl\"\n\n" +
                    "            struct appdata\n" +
                    "            {\n" +
                    MakeStruct(data, 0) +
                    "            };" + '\n' +
                    "            struct v2f\n" +
                    "            {\n" +
                    MakeStruct(data, 1) + '\n' +
                    "            };" + '\n' +
                    "            struct gbuffer\n" +
                    "            {\n" +
                    MakeStruct(data, 2) + '\n' +
                    "            };" + '\n' +
                    MakePropertyDefine(data) + '\n' +
                    MakePropertyDefine(data, true) + '\n' +
                    MakeTexBuffer(data) + '\n' +
                    "            v2f vert(appdata v)\n" +
                    "            {\n" +
                    "                v2f o;\n" +
                    vertText.Replace("v2f_", "o.v2f_").Replace("attr_", "v.attr_").Replace("ret;", "") +
                    "                return o;\n"+
                    "            }\n\n" +
                    "            gbuffer frag(v2f i)\n" +
                    "            {\n" +
                    "                gbuffer o;\n" +
                    fragText.Replace("v2f_", "i.v2f_").Replace("gbuffer_", "o.gbuffer_").Replace("ret;", "") +
                    "                return o;\n"+
                    "            }\n" +
                    "            ENDHLSL\n" +
                    "        }\n" +
                    "    }\n"+
                    "}";
            return text;
        }
    }
}