Shader"Invert/CharacterHair"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tex0("Texture 0", 2D) = "black"{}
        _Tex1("Texture 1", 2D) = "black"{}
        _Tex2("Texture 2", 2D) = "black"{}
        _Tex3("Texture 3", 2D) = "black"{}
        _Tex4("Texture 4", 2D) = "black"{}
        _Tex5("Texture 5", 2D) = "black"{}
        _Tex6("Texture 6", 2D) = "black"{}
        _Tex7("Texture 7", 2D) = "black"{}
        _Tex8("Texture 8", 2D) = "black"{}
        _Tex9("Texture 9", 2D) = "black"{}
        _Tex10("Texture 10", 2D) = "black"{}
        _Tex11("Texture 11", 2D) = "black"{}
        _Tex12("Texture 12", 2D) = "black"{}
        _Tex13("Texture 13", 2D) = "black"{}
        _Tex14("Texture 14", 2D) = "bump"{}
        _Tex15("Texture 15", 2D) = "black"{}
        _Tex16("Texture 16", 2D) = "black"{}
        _Tex17("Texture 17", 2D) = "black"{}
        _Tex18("Texture 18", 2D) = "black"{}
        _Tex19("Texture 19", 2D) = "black"{}
        _Tex20("Texture 20", 2D) = "black"{}
        _Tex21("Texture 21", 2D) = "black"{}
        _Tex22("Texture 22", 2D) = "black"{}
        _Tex23("Texture 23", 2D) = "black"{}
        _Tex24("Texture 24", 2D) = "black"{}
    }
    SubShader
    {
        Cull Front
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

            struct appdata
            {
               float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
               float2 uv : TEXCOORD0;
            };

            struct v2f
            {
               float4 vertex : SV_POSITION;
               float2 uv : TEXCOORD0;
                float3 t : TEXCOORD1;
                float3 b : TEXCOORD2;
                float3 n : TEXCOORD3;
            };

            Texture2D _MainTex;
            Texture2D _Tex0;
            Texture2D _Tex1;
            Texture2D _Tex2;
            Texture2D _Tex3;
            Texture2D _Tex4;
            Texture2D _Tex5;
            Texture2D _Tex6;
            Texture2D _Tex7;
            Texture2D _Tex8;
            Texture2D _Tex9;
            Texture2D _Tex10;
            Texture2D _Tex11;
            Texture2D _Tex12;
            Texture2D _Tex13;
            Texture2D _Tex14;
            SamplerState sampler_Tex14;
            Texture2D _Tex15;
            Texture2D _Tex16;
            Texture2D _Tex17;
            Texture2D _Tex18;
            Texture2D _Tex19;
            Texture2D _Tex20;
            Texture2D _Tex21;
            Texture2D _Tex22;
            Texture2D _Tex23;
            Texture2D _Tex24;
            
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                VertexNormalInputs vni = GetVertexNormalInputs(v.normal, v.tangent);
                o.t = vni.tangentWS;
                o.b = vni.bitangentWS;
                o.n = vni.normalWS;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half3 normalTex = SAMPLE_TEXTURE2D(_Tex14, sampler_Tex14, i.uv);
                half3 worldNormal = mul(normalTex, half3x3(i.t, i.b, i.n));
                
                return half4(worldNormal, 1);
            }
            ENDHLSL
        }
    }
}
