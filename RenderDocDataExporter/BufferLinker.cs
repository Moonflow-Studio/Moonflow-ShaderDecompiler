using System;

namespace Moonflow
{
    [Serializable]
    public struct BufferLinker
    {
        public int setIndex;
        public int bindingIndex;
        public int uniformIndex;
        public BufferDeclaration bufferDec;
    }
}