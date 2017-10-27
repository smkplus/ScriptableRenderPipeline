﻿using System;
using System.Text;
using UnityEngine;

namespace UnityEditor.ShaderGraph
{
    [Serializable]
    public class TextureShaderProperty : AbstractShaderProperty<SerializableTexture>
    {
        [SerializeField]
        private bool m_Modifiable;

        public TextureShaderProperty()
        {
            value = new SerializableTexture();
        }

        public override PropertyType propertyType
        {
            get { return PropertyType.Texture; }
        }

        public bool modifiable
        {
            get { return m_Modifiable; }
            set { m_Modifiable = value; }
        }

        public override Vector4 defaultValue
        {
            get { return new Vector4(); }
        }

        public override string GetPropertyBlockString()
        {
            var result = new StringBuilder();
            if (!m_Modifiable)
            {
                result.Append("[HideInInspector] ");
                result.Append("[NonModifiableTextureData] ");
            }
            result.Append("[NoScaleOffset] ");

            result.Append(referenceName);
            result.Append("(\"");
            result.Append(displayName);
            result.Append("\", 2D) = \"white\" {}");
            return result.ToString();
        }

        public override string GetPropertyDeclarationString()
        {
            return "UNITY_DECLARE_TEX2D(" + referenceName + ");";
        }

        public override string GetInlinePropertyDeclarationString()
        {
            return "UNITY_DECLARE_TEX2D_NOSAMPLER(" + referenceName + ");";
        }

        public override PreviewProperty GetPreviewMaterialProperty()
        {
            return new PreviewProperty()
            {
                m_Name = referenceName,
                m_PropType = PropertyType.Texture,
                m_Texture = value.texture
            };
        }
    }
}
