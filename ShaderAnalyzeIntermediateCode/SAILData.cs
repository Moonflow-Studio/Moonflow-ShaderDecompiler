using System.Collections.Generic;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class SAILData
    {
        public List<SAILVariable> inVar;
        public List<SAILVariable> outVar;
        public List<SAILTexture> tex;
        public List<SAILVariable> tempVar;
        public List<SASingleLine> calculationLines;
        
        public SAILData()
        {
            inVar = new List<SAILVariable>();
            outVar = new List<SAILVariable>();
            tex = new List<SAILTexture>();
            tempVar = new List<SAILVariable>();
            calculationLines = new List<SASingleLine>();
        }
    }
}