using System;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class SAILVariableToken : SAILToken
    {
        public string tokenTypeName;
        private float _intensity = 1f;

        public virtual SAILVariableToken Copy()
        {
            var copy = new SAILVariableToken();
            copy.tokenString = tokenString;
            copy.tokenTypeName = tokenTypeName;
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
    }
    public class SAILVariableToken<T> : SAILVariableToken
    {
        public T data;
        public override SAILVariableToken Copy()
        {
            var copy = new SAILVariableToken<T>();
            copy.tokenString = tokenString;
            copy.tokenTypeName = tokenTypeName;
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
    }
}