// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

#if UNITY_EDITOR

using UnityEngine;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    internal class CBaseShaderGUI : BaseShaderGUI
    {
        protected Material Material => (Material) materialEditor.target;
        private MaterialProperty[] materialProperties;
        
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            materialProperties = properties;
        }
        
        protected void DrawShaderKeywordToggle(string label, string keyword)
        {
            bool isEnabled = IsKeywordEnabled(keyword);
            bool newValue = EditorGUILayout.Toggle(label, isEnabled);
            SetActiveKeyword(keyword, newValue);
        }
        
        protected void DrawTextureSingleLine(string propertyName, string extraProperty = null)
        {
            MaterialProperty prop = FindProperty(propertyName, materialProperties);
            MaterialProperty extraProp = string.IsNullOrEmpty(extraProperty) ? null : FindProperty(extraProperty, materialProperties);
            materialEditor.TexturePropertySingleLine(new GUIContent(prop.displayName), prop, extraProp);
        }

        protected void DrawUvDissolve()
        {
            DrawShaderKeywordToggle("UV Dissolve", "_UV_DISSOLVE");
            bool enabled = IsKeywordEnabled("_UV_DISSOLVE");
            EditorGUI.BeginDisabledGroup(!enabled);
            DrawNativeProperty("_UvDissolveTexture");
            DrawNativeProperty("_UvDissolveThreshold");
            EditorGUI.EndDisabledGroup();
        }
        
        protected void DrawRim()
        {
            EditorGUILayout.Space();
            DrawShaderKeywordToggle("Rim", "_RIM");
            bool enabled = IsKeywordEnabled("_RIM");
            EditorGUI.BeginDisabledGroup(!enabled);
            DrawShaderKeywordToggle("Light To Rim", "_RIM_TO_LIGHT");
            DrawNativeProperty("_RimColor");
            DrawNativeProperty("_RimContrast");
            DrawNativeProperty("_RimAmount");
            EditorGUI.EndDisabledGroup();
        }
        
        protected void DrawWind()
        {
            EditorGUILayout.Space();
            DrawShaderKeywordToggle("Wind", "_WIND");
            bool enabled = IsKeywordEnabled("_WIND");
            
            EditorGUI.BeginDisabledGroup(!enabled);
            DrawNativeProperty("_Wind");
            DrawNativeProperty("_WindEdgeFlutter");
            DrawNativeProperty("_WindEdgeFlutterFreqScale");
            EditorGUI.EndDisabledGroup();
        }
        
        protected void DrawAo()
        {
            DrawTextureSingleLine("_Ao");
            DrawNativeProperty("_AoMod", 2);
            MaterialProperty property = FindProperty("_Ao", materialProperties);
            SetActiveKeyword("_USE_AO", property.textureValue != null);
        }

        protected void DrawReflectivity()
        {
            EditorGUILayout.Space();
            DrawShaderKeywordToggle("Environment Lighting", "_ENVIRO_LIGHTING");
            bool enabled = IsKeywordEnabled("_ENVIRO_LIGHTING");
            EditorGUI.BeginDisabledGroup(!enabled);
            DrawTextureSingleLine("_EnvironmentMap", "_EnvironmentTint");
            DrawNativeProperty("_EnvironmentMapMip");
            EditorGUI.EndDisabledGroup();
        }

        protected void SetActiveKeyword(string keyword, bool newState)
        {
            bool state = Material.IsKeywordEnabled(keyword);
            if (state == newState)
                return;

            if (newState)
            {
                Material.EnableKeyword(keyword);
            }
            else
            {
                Material.DisableKeyword(keyword);
            }
        }
        
        protected bool IsKeywordEnabled(string keyword)
        {
            return Material.IsKeywordEnabled(keyword);
        }
        
        protected void DrawNativeProperty(string property, int indentLevel = 0)
        {
            MaterialProperty prop = FindProperty(property, materialProperties);
            materialEditor.ShaderProperty(prop, prop.displayName, indentLevel);
        }
        
        private void DrawNativeProperty(MaterialProperty property, int indentLevel = 0)
        {
            materialEditor.ShaderProperty(property, property.displayName, indentLevel);
        }
    }
}
#endif