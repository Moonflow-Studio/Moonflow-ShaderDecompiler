using System;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
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
    }
}