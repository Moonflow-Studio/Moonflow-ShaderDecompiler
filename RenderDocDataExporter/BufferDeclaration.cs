using System;
using System.Collections.Generic;

namespace Moonflow
{
    [Serializable]
    public class BufferDeclaration
    {
        public int setIndex;
        public int bindingIndex;
        public int uniformIndex;
        public ShaderPassDef passDef;
        public string linkedFile;
        public string bufferName;
        public List<ShaderVariable> variables;
    }
}