using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonflow
{
    [Serializable]
    public struct BufferDeclaration
    {
        public string bufferName;
        public int bufferId;
        public int offset;
    }
}