﻿using Unity.Mathematics;
using UnityEngine;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public static class SAILTokenFactory
    {
        public static SAILTextureToken CreateTexture(string name, SAILDataTokenType tokenType)
        {
            switch (tokenType)
            {
                case SAILDataTokenType.SAMPLER2D:
                    return new SAILTextureToken() { tokenString = name, type = SAILTextureTokenType.D2 , tokenTypeName = "Texture2D"};
                case SAILDataTokenType.SAMPLER3D:
                    return new SAILTextureToken() { tokenString = name, type = SAILTextureTokenType.D3 , tokenTypeName = "Texture3D"};
                case SAILDataTokenType.SAMPLERCUBE:
                    return new SAILTextureToken() { tokenString = name, type = SAILTextureTokenType.CUBE , tokenTypeName = "TextureCube"};
                case SAILDataTokenType.SAMPLER2DARRAY:
                    return new SAILTextureToken() { tokenString = name, type = SAILTextureTokenType.D2ARRAY , tokenTypeName = "Texture2D<>"};
                case SAILDataTokenType.SAMPLER3DARRAY:
                    return new SAILTextureToken() { tokenString = name, type = SAILTextureTokenType.D3ARRAY , tokenTypeName = "Texture3D<>"};
                case SAILDataTokenType.SAMPLERCUBEARRAY:
                    return new SAILTextureToken() { tokenString = name, type = SAILTextureTokenType.CUBEARRAY , tokenTypeName = "TextureCube<>"};
            }
            Debug.Assert(false, "未知的纹理类型");
            return null;
        }
        public static SAILVariableToken CreateVariable(string name, SAILDataTokenType tokenType)
        {
            switch (tokenType)
            {
                case SAILDataTokenType.INT:
                    return new SailVariableToken<int>() { tokenString = name , tokenTypeName = "int"};
                case SAILDataTokenType.INT2:
                    return new SailVariableToken<int2>() { tokenString = name , tokenTypeName = "int2"};
                case SAILDataTokenType.INT3:
                    return new SailVariableToken<int3>() { tokenString = name , tokenTypeName = "int3"};
                case SAILDataTokenType.INT4:
                    return new SailVariableToken<int4>() { tokenString = name , tokenTypeName = "int4"};
                case SAILDataTokenType.UINT:
                    return new SailVariableToken<uint>() { tokenString = name , tokenTypeName = "uint"};
                case SAILDataTokenType.UINT2:
                    return new SailVariableToken<uint2>() { tokenString = name , tokenTypeName = "uint2"};
                case SAILDataTokenType.UINT3:
                    return new SailVariableToken<uint3>() { tokenString = name , tokenTypeName = "uint3"};
                case SAILDataTokenType.UINT4:
                    return new SailVariableToken<uint4>() { tokenString = name , tokenTypeName = "uint4"};
                case SAILDataTokenType.FLOAT:
                    return new SailVariableToken<float>() { tokenString = name , tokenTypeName = "float"};
                case SAILDataTokenType.FLOAT2:
                    return new SailVariableToken<float2>() { tokenString = name , tokenTypeName = "float2"};
                case SAILDataTokenType.FLOAT3:
                    return new SailVariableToken<float3>() { tokenString = name , tokenTypeName = "float3"};
                case SAILDataTokenType.FLOAT4:
                    return new SailVariableToken<float4>() { tokenString = name , tokenTypeName = "float4"};
                case SAILDataTokenType.MATRIX2X2:
                    return new SailVariableToken<float2x2>() { tokenString = name , tokenTypeName = "float2x2"};
                case SAILDataTokenType.MATRIX2X3:
                    return new SailVariableToken<float2x3>() { tokenString = name , tokenTypeName = "float2x3"};
                case SAILDataTokenType.MATRIX2X4:
                    return new SailVariableToken<float2x4>() { tokenString = name , tokenTypeName = "float2x4"};
                case SAILDataTokenType.MATRIX3X2:
                    return new SailVariableToken<float3x2>() { tokenString = name , tokenTypeName = "float3x2"};
                case SAILDataTokenType.MATRIX3X3:
                    return new SailVariableToken<float3x3>() { tokenString = name , tokenTypeName = "float3x3"};
                case SAILDataTokenType.MATRIX3X4:
                    return new SailVariableToken<float3x4>() { tokenString = name , tokenTypeName = "float3x4"};
                case SAILDataTokenType.MATRIX4X2:
                    return new SailVariableToken<float4x2>() { tokenString = name , tokenTypeName = "float4x2"};
                case SAILDataTokenType.MATRIX4X3:
                    return new SailVariableToken<float4x3>() { tokenString = name , tokenTypeName = "float4x3"};
                case SAILDataTokenType.MATRIX4X4:
                    return new SailVariableToken<float4x4>() { tokenString = name , tokenTypeName = "float4x4"};
                case SAILDataTokenType.BOOL:
                    return new SailVariableToken<bool>() { tokenString = name , tokenTypeName = "bool"};
                case SAILDataTokenType.BOOL2:
                    return new SailVariableToken<bool2>() { tokenString = name , tokenTypeName = "bool2"};
                case SAILDataTokenType.BOOL3:
                    return new SailVariableToken<bool3>() { tokenString = name , tokenTypeName = "bool3"};
                case SAILDataTokenType.BOOL4:
                    return new SailVariableToken<bool4>() { tokenString = name , tokenTypeName = "bool4"};
            }
            Debug.LogError($"未知的数据类型 {name}");
            return null;
        }
    }
}