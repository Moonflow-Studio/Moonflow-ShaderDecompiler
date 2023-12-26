using Unity.Mathematics;
using UnityEngine;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public static class SAILTokenFactory
    {
        public static SAILTextureToken CreateTexture(string name, SAILDataTokenType tokenType)
        {
            switch (tokenType)
            {
                case SAILDataTokenType.TEXTURE2D:
                    return new SAILTextureToken() { tokenString = name, type = SAILTextureType.D2 , tokenType = SAILDataTokenType.TEXTURE2D};
                case SAILDataTokenType.TEXTURE3D:
                    return new SAILTextureToken() { tokenString = name, type = SAILTextureType.D3 , tokenType = SAILDataTokenType.TEXTURE3D};
                case SAILDataTokenType.TEXTURECUBE:
                    return new SAILTextureToken() { tokenString = name, type = SAILTextureType.CUBE , tokenType = SAILDataTokenType.TEXTURECUBE};
                case SAILDataTokenType.TEXTURE2DARRAY:
                    return new SAILTextureToken() { tokenString = name, type = SAILTextureType.D2ARRAY , tokenType = SAILDataTokenType.TEXTURE2DARRAY};
                case SAILDataTokenType.TEXTURE3DARRAY:
                    return new SAILTextureToken() { tokenString = name, type = SAILTextureType.D3ARRAY , tokenType = SAILDataTokenType.TEXTURE3DARRAY};
            }
            Debug.Assert(false, "未知的纹理类型");
            return null;
        }
        public static SAILVariableToken CreateVariable(string name, SAILDataTokenType tokenType)
        {
            switch (tokenType)
            {
                case SAILDataTokenType.INT:
                    return new SAILVariableToken<int>() { tokenString = name , tokenType = SAILDataTokenType.INT};
                case SAILDataTokenType.INT2:
                    return new SAILVariableToken<int2>() { tokenString = name , tokenType = SAILDataTokenType.INT2};
                case SAILDataTokenType.INT3:
                    return new SAILVariableToken<int3>() { tokenString = name , tokenType = SAILDataTokenType.INT3};
                case SAILDataTokenType.INT4:
                    return new SAILVariableToken<int4>() { tokenString = name , tokenType = SAILDataTokenType.INT4};
                case SAILDataTokenType.UINT:
                    return new SAILVariableToken<uint>() { tokenString = name , tokenType = SAILDataTokenType.UINT};
                case SAILDataTokenType.UINT2:
                    return new SAILVariableToken<uint2>() { tokenString = name , tokenType = SAILDataTokenType.UINT2};
                case SAILDataTokenType.UINT3:
                    return new SAILVariableToken<uint3>() { tokenString = name , tokenType = SAILDataTokenType.UINT3};
                case SAILDataTokenType.UINT4:
                    return new SAILVariableToken<uint4>() { tokenString = name , tokenType = SAILDataTokenType.UINT4};
                case SAILDataTokenType.FLOAT:
                    return new SAILVariableToken<float>() { tokenString = name , tokenType = SAILDataTokenType.FLOAT};
                case SAILDataTokenType.FLOAT2:
                    return new SAILVariableToken<float2>() { tokenString = name , tokenType = SAILDataTokenType.FLOAT2};
                case SAILDataTokenType.FLOAT3:
                    return new SAILVariableToken<float3>() { tokenString = name , tokenType = SAILDataTokenType.FLOAT3};
                case SAILDataTokenType.FLOAT4:
                    return new SAILVariableToken<float4>() { tokenString = name , tokenType = SAILDataTokenType.FLOAT4};
                // case SAILDataTokenType.MATRIX2X2:
                //     return new SAILVariableToken<float2x2>() { tokenString = name , tokenTypeName = "float2x2"};
                // case SAILDataTokenType.MATRIX2X3:
                //     return new SAILVariableToken<float2x3>() { tokenString = name , tokenTypeName = "float2x3"};
                // case SAILDataTokenType.MATRIX2X4:
                //     return new SAILVariableToken<float2x4>() { tokenString = name , tokenTypeName = "float2x4"};
                // case SAILDataTokenType.MATRIX3X2:
                //     return new SAILVariableToken<float3x2>() { tokenString = name , tokenTypeName = "float3x2"};
                // case SAILDataTokenType.MATRIX3X3:
                //     return new SAILVariableToken<float3x3>() { tokenString = name , tokenTypeName = "float3x3"};
                // case SAILDataTokenType.MATRIX3X4:
                //     return new SAILVariableToken<float3x4>() { tokenString = name , tokenTypeName = "float3x4"};
                // case SAILDataTokenType.MATRIX4X2:
                //     return new SAILVariableToken<float4x2>() { tokenString = name , tokenTypeName = "float4x2"};
                // case SAILDataTokenType.MATRIX4X3:
                //     return new SAILVariableToken<float4x3>() { tokenString = name , tokenTypeName = "float4x3"};
                // case SAILDataTokenType.MATRIX4X4:
                //     return new SAILVariableToken<float4x4>() { tokenString = name , tokenTypeName = "float4x4"};
                case SAILDataTokenType.BOOL:
                    return new SAILVariableToken<bool>() { tokenString = name , tokenType = SAILDataTokenType.BOOL};
                case SAILDataTokenType.BOOL2:
                    return new SAILVariableToken<bool2>() { tokenString = name , tokenType = SAILDataTokenType.BOOL2};
                case SAILDataTokenType.BOOL3:
                    return new SAILVariableToken<bool3>() { tokenString = name , tokenType = SAILDataTokenType.BOOL3};
                case SAILDataTokenType.BOOL4:
                    return new SAILVariableToken<bool4>() { tokenString = name , tokenType = SAILDataTokenType.BOOL4};
            }
            Debug.LogError($"未知的数据类型 {name}");
            return null;
        }
    }
}