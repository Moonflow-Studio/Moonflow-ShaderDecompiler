using System;

namespace Moonflow
{
    [Serializable]
    public struct BufferLinker : ICloneable
    {
        public int setIndex;
        public int bindingIndex;
        public int uniformIndex;
        public ShaderPassDef passDef;
        public BufferDeclaration bufferDec;
        public object Clone()
        {
            BufferLinker linker = new BufferLinker();
            linker.setIndex = setIndex;
            linker.bindingIndex = bindingIndex;
            linker.uniformIndex = uniformIndex;
            linker.passDef = passDef;
            linker.bufferDec = bufferDec;
            return linker;
        }
    }
}