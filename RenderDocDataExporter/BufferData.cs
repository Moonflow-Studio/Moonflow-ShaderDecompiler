using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonflow
{
    [Serializable]
    public class BufferData
    {
        // public int setIndex;
        // public int bindingIndex;
        // public int uniformIndex;
        public BufferDeclaration dec;
        public ShaderPassDef passDef;
        public string linkedFile;
        public List<ShaderVariable> variables;

        // public void SetValueToMat(Material mat)
        // {
        //     if (mat == null) return;
        //     for (int i = 0; i < variables.Count; i++)
        //     {
        //         string targetName = $"_{uniformIndex}_m{variables[i].name.Replace("_child", "")}";
        //         if (variables[i].GetType() == VariableType.Float)
        //         {
        //             mat.SetFloat(targetName, variables[i].value);
        //         }else if (variables[i].GetType() == VariableType.Vector)
        //         {
        //             mat.SetVector(targetName, variables[i].values[0]);
        //         }else if (variables[i].GetType() == VariableType.Matrix)
        //         {
        //             Matrix4x4 matrix = Matrix4x4.zero;
        //             matrix.m00 = variables[i].values[0][0];
        //             matrix.m01 = variables[i].values[0][1];
        //             matrix.m02 = variables[i].values[0][2];
        //             matrix.m03 = variables[i].values[0][3];
        //             matrix.m10 = variables[i].values[1][0];
        //             matrix.m11 = variables[i].values[1][1];
        //             matrix.m12 = variables[i].values[1][2];
        //             matrix.m13 = variables[i].values[1][3];
        //             matrix.m20 = variables[i].values[2][0];
        //             matrix.m21 = variables[i].values[2][1];
        //             matrix.m22 = variables[i].values[2][2];
        //             matrix.m23 = variables[i].values[2][3];
        //             matrix.m30 = variables[i].values[3][0];
        //             matrix.m31 = variables[i].values[3][1];
        //             matrix.m32 = variables[i].values[3][2];
        //             matrix.m33 = variables[i].values[3][3];
        //             mat.SetMatrix(targetName, matrix);
        //         }else if (variables[i].GetType() == VariableType.VectorList)
        //         {
        //             mat.SetVectorArray(targetName, variables[i].values);
        //         }
        //     }
        // }
    }
}