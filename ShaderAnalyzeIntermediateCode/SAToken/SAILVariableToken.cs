using System;
using System.Collections.Generic;

namespace Moonflow.Tools.MFUtilityTools.GLSLCC
{
    public class SAILVariableToken : SAILToken
    {
        public SAILDataTokenType tokenType;
        private float _intensity = 1f;

        public virtual SAILVariableToken Copy()
        {
            var copy = new SAILVariableToken();
            copy.tokenString = tokenString;
            copy.tokenType = tokenType;
            return copy;
        }

        public override float GetUIIntensity()
        {
            return _intensity;
        }

        public void Chosen()
        {
            _intensity = 10;
        }

        public void DecreaseIntensity(float decrease)
        {
            _intensity -= decrease;
            if (_intensity < 1) _intensity = 1;
        }
        
        public string GetDefaultChannel()
        {
            switch (tokenType)
            {
                case SAILDataTokenType.INT:
                case SAILDataTokenType.UINT:
                case SAILDataTokenType.FLOAT:
                case SAILDataTokenType.BOOL: return "x";
                case SAILDataTokenType.INT2:
                case SAILDataTokenType.UINT2:
                case SAILDataTokenType.FLOAT2:
                case SAILDataTokenType.BOOL2: return "xy";
                case SAILDataTokenType.INT3:
                case SAILDataTokenType.UINT3:
                case SAILDataTokenType.FLOAT3:
                case SAILDataTokenType.BOOL3: return "xyz";
                case SAILDataTokenType.INT4:
                case SAILDataTokenType.UINT4:
                case SAILDataTokenType.FLOAT4:
                case SAILDataTokenType.BOOL4:
                case SAILDataTokenType.TEXTURE2D:
                case SAILDataTokenType.TEXTURE3D:
                case SAILDataTokenType.TEXTURECUBE:
                case SAILDataTokenType.TEXTURE2DARRAY:
                case SAILDataTokenType.TEXTURE3DARRAY: return "xyzw";
            }

            return "";
        }
    }
    public class SAILVariableToken<T> : SAILVariableToken
    {
        public T data;
        public override SAILVariableToken Copy()
        {
            var copy = new SAILVariableToken<T>();
            copy.tokenString = tokenString;
            copy.tokenType = tokenType;
            copy.data = data;
            return copy;
        }
    }
    public class SAILTextureToken : SAILVariableToken
    {
        public SAILTextureType type;
        public SAILSamplerType samplerType;
    }
    public class SAILPieceVariableToken : SAILToken
    {
        public string channel;
        public SAILVariableToken link;
        public override string ShowString()
        {
            try
            {
                return (link?.ShowString()) + "." + channel;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public override float GetDisplaySize()
        {
            return 10 + (tokenString.Length + channel.Length + 1) * 8;
        }

        public bool MatchChannel(string otherChannel, out bool totalMatch)
        {
            if (otherChannel == channel)
            {
                totalMatch = true;
                return true;
            }
            
            HashSet<char> channels = new HashSet<char>();
            for (int i = 0; i < channel.Length; i++)
            {
                channels.Add(channel[i]);
            }

            totalMatch = false;
            foreach (var channelChar in otherChannel)
            {
                if (channels.Contains(channelChar)) return true;
            }
            return false;
        }
        
    }
}