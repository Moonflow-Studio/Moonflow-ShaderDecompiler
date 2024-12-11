using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Moonflow
{
    [Serializable]
    public struct ShaderCodePair
    {
        public ShaderCodeIdPair id;
        public string vsFilePath;
        public string psFilePath;
        public string vsHLSLPath;
        public string psHLSLPath;
        public List<BufferLinker> bufferLinkersExample;
    }

    [Serializable]
    public struct ShaderCodeIdPair
    {
        public string vsid;
        public string psid;
    }
}