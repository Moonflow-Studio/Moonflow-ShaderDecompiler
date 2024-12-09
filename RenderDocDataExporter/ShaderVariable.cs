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
        // public Vector4[] values;
        public List<ShaderVariable> sub = new List<ShaderVariable>();

        public VariableType GetType()
        {
            if (sub == null || sub.Count == 0)
            {
                return VariableType.Float;
            }
            if (sub.Count == 1 && sub[0].sub.Count == 4)
            {
                return VariableType.Vector;
            }
            if (sub.Count == 4)
            {
                for (int i = 0; i < sub.Count; i++)
                {
                    if(sub[i].sub.Count != 4)
                    {
                        return VariableType.ERROR;
                    }
                }
                return VariableType.Matrix;
            }
            return VariableType.VectorList;
        }

        public object Clone()
        {
            ShaderVariable clone = new ShaderVariable();
            clone.name = name;
            clone.value = value;
            clone.sub = new List<ShaderVariable>();
            if (sub == null) sub = new List<ShaderVariable>();
            foreach (ShaderVariable v in sub)
            {
                clone.sub.Add((ShaderVariable)v.Clone());
            }
            
            return clone;
        }
    }

    public enum VariableType
    {
        Float,
        Vector,
        Matrix,
        VectorList,
        Sub,
        ERROR
    }
}