using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEditor.Graphing;

namespace UnityEditor.ShaderGraph
{
    [Title("Math/Derivative/DDY")]
    public class DDYNode : CodeFunctionNode
    {
        public DDYNode()
        {
            name = "DDY";
        }

        [SerializeField]
        private PartialDerivativePrecision m_PartialDerivativePrecision = PartialDerivativePrecision.Default;

        [EnumControl("Precision")]
        public PartialDerivativePrecision partialDerivativePrecision
        {
            get { return m_PartialDerivativePrecision; }
            set
            {
                if (m_PartialDerivativePrecision == value)
                    return;

                m_PartialDerivativePrecision = value;
                if (onModified != null)
                {
                    onModified(this, ModificationScope.Graph);
                }
            }
        }

        string GetCurrentPrecision()
        {
            return System.Enum.GetName(typeof(PartialDerivativePrecision), m_PartialDerivativePrecision);
        }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod(string.Format("Unity_DDY_{0}", GetCurrentPrecision()),
                BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string Unity_DDY_Default(
            [Slot(0, Binding.None)] Vector1 In,
            [Slot(1, Binding.None)] out Vector1 Out)
        {
            return
                @"
{
    Out = ddy(In);
}
";
        }

        static string Unity_DDY_Coarse(
            [Slot(0, Binding.None)] Vector1 In,
            [Slot(1, Binding.None)] out Vector1 Out)
        {
            return
                @"
{
    Out = ddy_coarse(In);
}
";
        }

        static string Unity_DDY_Fine(
            [Slot(0, Binding.None)] Vector1 In,
            [Slot(1, Binding.None)] out Vector1 Out)
        {
            return
                @"
{
    Out = ddy_fine(In);
}
";
        }
    }
}
