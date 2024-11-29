using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonflow
{
    [Serializable]
    public class ShaderVariable : ICloneable
    {
        public string name;
        public float value;
        public Vector4[] values;

        public VariableType GetType()
        {
            if (values == null || values.Length == 0)
            {
                return VariableType.Float;
            }

            if (values.Length == 1)
            {
                return VariableType.Vector;
            }

            if (values.Length == 4)
            {
                return VariableType.Matrix;
            }
            
            return VariableType.VectorList;
        }

        public object Clone()
        {
            ShaderVariable clone = new ShaderVariable();
            clone.name = name;
            clone.value = value;
            clone.values = values;
            return clone;
        }
    }

    public enum VariableType
    {
        Float,
        Vector,
        Matrix,
        VectorList
    }
}