using System.Collections.Generic;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class SAILData
    {
        public List<SAILVariableToken> inVar;
        public List<SAILVariableToken> outVar;
        public List<SAILVariableToken> glbVar;
        public List<SAILVariableToken> tempVar;
        public List<SAILSingleline> calculationLines;
        
        public SAILData()
        {
            inVar = new List<SAILVariableToken>();
            outVar = new List<SAILVariableToken>();
            glbVar = new List<SAILVariableToken>();
            tempVar = new List<SAILVariableToken>();
            calculationLines = new List<SAILSingleline>();
        }

        public SAILVariableToken FindVariable(string token)
        {
            foreach (var variable in inVar)
            {
                if (variable.tokenString == token)
                {
                    return variable;
                }
            }
            foreach (var variable in outVar)
            {
                if (variable.tokenString == token)
                {
                    return variable;
                }
            }
            foreach (var variable in glbVar)
            {
                if (variable.tokenString == token)
                {
                    return variable;
                }
            }
            foreach (var variable in tempVar)
            {
                if (variable.tokenString == token)
                {
                    return variable;
                }
            }
            return null;
        }
    }
}