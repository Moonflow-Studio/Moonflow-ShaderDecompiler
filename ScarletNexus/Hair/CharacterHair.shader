Shader"Invert/CharacterHair"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalTex("normal tex", 2D) = "bump"{}
        _LightenColorTex("lighten color tex", 2D) = "white"{}
        _DarkColorTex("dark color tex", 2D) = "black"{}
        _AnistophTex("Anistoph Tex", 2D) = "grey"{}
        _GradientTex("Gradient Tex", 2D) = "white"{}
        _HighlightMask("Highlight Mask", 2D) = "black"{}
        _RampTex("Ramp Tex", 2D) = "white"{}
        
        _AnistophStr("Anistoph Str", Float) = 15
        
        _NormalOffsetStr("normalOffsetStr", Range(0,1)) = 0
        _UnknownVector0("Unknown Vector 0", Vector) = (300, -0.5, 0, 100)
        _UnknownVector019("Unknown Vector 0 19", Vector) = (-0.97132, -0.22261, 0.0835, 0)
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
                float4 normal : NORMAL;
                float4 tangent : TANGENT;
                float3 color : COLOR;
               float2 uv : TEXCOORD0;
            };

            struct v2f
            {
               float4 vertex : SV_POSITION;
               float2 uv : TEXCOORD0;
                float3 t : TEXCOORD1;
                float3 b : TEXCOORD2;
                float4 n : TEXCOORD3;
                float4 color : COLOR;
                
                float2 suspectNormal : TEXCOORD4;
                float2 suspectTangent : TEXCOORD5;
                float2 suspectPosition : TEXCOORD6;
                float2 anistophParams : TEXCOORD7;
                float3 posWorld : TEXCOORD9;
                float4 normalTEMP : TEXCOORD10;
                float3 normalOS : TEXCOORD11;
                // float4 isFrontFace : SV_POSITION;
            };

            struct gbuffer
            {
                float4 emissive;
                float4 worldNormal;
                float4 gbuffer_2;
                float4 diffuse;
                float4 charaLight;
                float4 gbuffer_5;
                float4 gbuffer_6;
            };

            Texture2D _MainTex;
            Texture2D _NormalTex;
            Texture2D _LightenColorTex;
            Texture2D _DarkColorTex;
            Texture2D _AnistophTex;
            Texture2D _GradientTex;
            Texture2D _HighlightMask;
            Texture2D _RampTex;
            SamplerState sampler_NormalTex;
            SamplerState single_linear_clamp_sampler;
            
            float4 _MainTex_ST;
            float _AnistophStr;
            float _NormalOffsetStr;
            float4 _UnknownVector0;
            float4 _UnknownVector019;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.normalOS = v.vertex;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                VertexNormalInputs vni = GetVertexNormalInputs(v.normal, v.tangent);
                o.t = vni.tangentWS;
                o.b = vni.bitangentWS;
                o.n.xyz = vni.normalWS;
                o.n.w = 1;
                VertexPositionInputs vpi = GetVertexPositionInputs(v.vertex);
                o.posWorld = vpi.positionWS;

                // v.normal = float3(-v.normal.x, v.normal.z, v.normal.y);
                o.normalTEMP.xyz = v.normal;

                float r0w = min(abs(v.normal.x), abs(v.normal.y));
                float r1x = (1 / (max(abs(v.normal.x), abs(v.normal.y))));
                r0w = r0w * r1x;
                o.normalTEMP.w = r0w;
                r1x = (r0w * r0w) * ((r0w * r0w) * ((r0w * r0w) * ((r0w * r0w) * 0.0208 + -0.0851) + 0.1801) + -0.3303) + 0.9999;
                float r1y = (abs(v.normal.x) < abs(v.normal.y)) ? 0 : ((r0w * r1x) * -2 + 1.5708);
                r0w = r0w * r1x + r1y;
                r1x = (v.normal.x < 0) ?  -3.1416 : 0;
                r0w = r0w + r1x;
                r1x = ((max(v.normal.x, v.normal.y)) >= -(max(v.normal.x, v.normal.y))) & ((min(v.normal.x, v.normal.y)) < -(min(v.normal.x, v.normal.y)));
                o.anistophParams.x = frac(((r1x ? -r0w : r0w) * 0.1592));
                o.anistophParams.y = -abs(v.normal.z) + 1;

                o.suspectNormal = v.normal.xy;
                o.suspectTangent.x = v.normal.z;
                o.suspectTangent.y = v.vertex.x;
                o.suspectPosition.xy = v.vertex.yz;
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float3 unknown = (i.n.yzx * i.t.zxy + -(i.t.yzx * i.n.zxy)) * i.n.w;
                half3 normal = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, i.uv);
                float3 unpackedNormal;
                unpackedNormal = normal * 2 - 1;
                // unpackedNormal.z = 1;
                unpackedNormal = normalize(unpackedNormal);
                float3 normalOffsetValue = normalize(float3(i.suspectNormal.xy, i.suspectTangent.x));
                float3 offsetedNormal;
                offsetedNormal.xz = unpackedNormal.xz + normalOffsetValue.xz;
                offsetedNormal.y = unpackedNormal.y * normalOffsetValue.y;
                
                float normalOffsetStr = _NormalOffsetStr;
                float3 mixedNormal = lerp(-unpackedNormal.xyz, normalize(offsetedNormal), normalOffsetStr);
                float3 finalNormalTex = normalize(mixedNormal);

                float2 anistophUV = i.anistophParams.xx * float2(0.4,0);
                float anistoph = ((SAMPLE_TEXTURE2D(_AnistophTex, single_linear_clamp_sampler, anistophUV)) * i.anistophParams.y) * _AnistophStr;
                
                float3 temp = finalNormalTex.x * i.t.xyz + (unknown.xyz * finalNormalTex.y);
                finalNormalTex.xyz = finalNormalTex.z * i.n.xyz + temp.xyz;
                float3 finalNormal = normalize(finalNormalTex);
                float normalOffsetDot = dot(finalNormal, normalize(i.normalOS));
                normalOffsetDot = 1 - max(normalOffsetDot, 0);
                
                float nnNormalOffsetDot = max(abs(normalOffsetDot), 0);
                nnNormalOffsetDot = nnNormalOffsetDot * nnNormalOffsetDot * .96 + 0.04;
                nnNormalOffsetDot = nnNormalOffsetDot * abs(i.n) * _UnknownVector019.z + _UnknownVector019.z * 0.5 + i.suspectPosition.y * 0.001 + finalNormal.z * 0.6;
                float3 dir = /*-(*/finalNormal * anistoph/* + i.normalOS.xyz) + props_f_0_68.xyz*/;
                float gradientUVXOffset = nnNormalOffsetDot - (dir.x / length(dir));
                float3 hairGradient = SAMPLE_TEXTURE2D(_GradientTex, single_linear_clamp_sampler, float2(gradientUVXOffset, 0));
                
                half3 worldNormal = mul(finalNormalTex, half3x3(i.t, i.b, i.n.xyz));


                
                return half4(hairGradient, 1);
            }
            ENDHLSL
        }
    }
}
