using System;
using UnityEngine.Serialization;

namespace Moonflow
{
    [Serializable]
    public struct ShaderCodePair
    {
        public ShaderCodeIdPair id;
        public string vsFilePath;
        public string psFilePath;
    }

    [Serializable]
    public struct ShaderCodeIdPair
    {
        public string vsid;
        public string psid;
    }
}