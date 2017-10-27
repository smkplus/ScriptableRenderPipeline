using System.Reflection;
using UnityEngine;

namespace UnityEditor.ShaderGraph
{
    [Title("Math/Vector/Dot Product")]
    public class DotProductNode : CodeFunctionNode
    {
        public DotProductNode()
        {
            name = "Dot Product";
        }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("Unity_DotProduct", BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string Unity_DotProduct(
            [Slot(0, Binding.None)] Vector3 A,
            [Slot(1, Binding.None)] Vector3 B,
            [Slot(2, Binding.None)] out Vector1 Out)
        {
            return
                @"
{
    Out = dot(A, B);
}
";
        }
    }
}
