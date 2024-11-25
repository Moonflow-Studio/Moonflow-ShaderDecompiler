using System.Collections.Generic;

namespace Moonflow.Tools.MFUtilityTools.GLSLCC
{
    public class SAILData
    {
        public List<SAILVariableToken> inVar;
        public List<SAILVariableToken> outVar;
        public List<SAILVariableToken> glbVar;
        public List<SAILVariableToken> tempVar;
        public List<SAILSingleline> calculationLines;

        public SAILVariableToken outputPosition;
        
        public SAILData()
        {
            inVar = new List<SAILVariableToken>();
            outVar = new List<SAILVariableToken>();
            glbVar = new List<SAILVariableToken>();
            tempVar = new List<SAILVariableToken>();
            calculationLines = new List<SAILSingleline>();
            outputPosition = new SAILVariableToken() { tokenString = "sv_position", tokenType = SAILDataTokenType.FLOAT4 };
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
        
        public bool MatchInputVariable(string token)
        {
            foreach (var variable in inVar)
            {
                if (variable.tokenString == token)
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool MatchOutputVariable(string token)
        {
            foreach (var variable in outVar)
            {
                if (variable.tokenString == token)
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool MatchGlobalVariable(string token)
        {
            foreach (var variable in glbVar)
            {
                if (variable.tokenString == token)
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool MatchTemporaryVariable(string token)
        {
            foreach (var variable in tempVar)
            {
                if (variable.tokenString == token)
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool MatchInputVariable(SAILVariableToken token)
        {
            foreach (var variable in inVar)
            {
                if (variable.tokenString == token.tokenString)
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool MatchOutputVariable(SAILVariableToken token)
        {
            foreach (var variable in outVar)
            {
                if (variable.tokenString == token.tokenString)
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool MatchGlobalVariable(SAILVariableToken token)
        {
            foreach (var variable in glbVar)
            {
                if (variable.tokenString == token.tokenString)
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool MatchTemporaryVariable(SAILVariableToken token)
        {
            foreach (var variable in tempVar)
            {
                if (variable.tokenString == token.tokenString)
                {
                    return true;
                }
            }
            return false;
        }

        public void UpdateIntensity(float decrease)
        {
            foreach (var invar in inVar)
            {
                invar.DecreaseIntensity(decrease);
            }

            foreach (var outvar in outVar)
            {
                outvar.DecreaseIntensity(decrease);
            }

            foreach (var glbvar in glbVar)
            {
                glbvar.DecreaseIntensity(decrease);
            }

            foreach (var tempvar in tempVar)
            {
                tempvar.DecreaseIntensity(decrease);
            }
        }
    }
}