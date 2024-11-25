using System;

namespace Moonflow.Tools.MFUtilityTools.GLSLCC
{
    public class SAILToken : Object
    {
        public string tokenString;
        public virtual string ShowString()
        {
            return tokenString;
        }
        public virtual float GetDisplaySize()
        {
            return 20 + tokenString.Length * 8;
        }

        public virtual float GetUIIntensity()
        {
            return 1;
        }
    }
}