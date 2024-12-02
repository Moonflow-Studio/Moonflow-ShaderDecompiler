using System.Collections.Generic;
using UnityEngine;

namespace Moonflow
{
    public class HLSLAnalyzer
    {
        public ShaderCodePair shaderCodePair;
        public Dictionary<int, BufferDeclaration> cbuffers;
        
        public void Setup(ShaderCodePair codepairValue)
        {
            shaderCodePair = codepairValue;
        }

        public void Analyze()
        {
            
        }
    }
}