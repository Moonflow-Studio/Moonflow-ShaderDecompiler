using System;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector4 = UnityEngine.Vector4;

namespace Moonflow
{
    public static class PRSReverseAux
    {
        public struct PRS
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        public static PRS GetPRS(Matrix4x4 matrix)
        {
            PRS prs = new PRS();
            prs.position = new Vector3(matrix.m03, matrix.m13, matrix.m23);
            Matrix4x4 rotationMatrix = new Matrix4x4(new Vector4(matrix.m00, matrix.m01, matrix.m02,0.0f),
                new Vector4(matrix.m10, matrix.m11, matrix.m12,0.0f),
                new Vector4(matrix.m20, matrix.m21, matrix.m22,0.0f),Vector4.zero);
            rotationMatrix = rotationMatrix.transpose;
            // prs.rotation = Quaternion.LookRotation(rotationMatrix.GetColumn(0), rotationMatrix.GetColumn(1));
            try
            {
                prs.rotation = rotationMatrix.rotation;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            prs.scale = new Vector3(
                matrix.GetColumn(0).magnitude,
                matrix.GetColumn(1).magnitude,
                matrix.GetColumn(2).magnitude);
            return prs;
        }
    }
}