// FBXEditor.TextureLinker.cs
// This partial class contains all logic for the "Texture Linker" tab.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace FBXEditor.Editor
{
    public partial class FBXEditor : EditorWindow
{
    // --- Texture Linker Variables ---
    private Vector2 materialScrollPos;
    private Object searchFolder;
    private Dictionary<string, List<string>> textureSuffixMap;
    private List<string> texturePropertyDrawOrder;
    private Dictionary<string, string> propertyDisplayNames;
    private Dictionary<Material, Dictionary<string, Texture>> originalTextureCache;

    /// <summary>
    /// Initializes dictionaries used for texture searching and UI drawing order.
    /// </summary>
    private void InitializeTextureLinker()
    {
        // Maps shader property names to common keywords found in texture filenames.
        textureSuffixMap = new Dictionary<string, List<string>>()
        {
            { "_BaseMap", new List<string> { "albedo", "diffuse", "color", "basecolor", "basemap", "base" } },
            { "_MainTex", new List<string> { "albedo", "diffuse", "color", "basecolor", "basemap", "base" } },
            { "_MetallicGlossMap", new List<string> { "metallic", "metal", "m" } },
            { "_SpecGlossMap", new List<string> { "specular", "spec", "s" } },
            { "_BumpMap", new List<string> { "normal", "norm", "n", "bump" } },
            { "_ParallaxMap", new List<string> { "height", "h", "displacement", "disp" } },
            { "_HeightMap", new List<string> { "height", "h", "displacement", "disp" } },
            { "_OcclusionMap", new List<string> { "occlusion", "ao", "ambientocclusion" } },
            { "_EmissionMap", new List<string> { "emission", "emissive", "e", "glow" } }
        };
        
        // Defines the order in which texture slots are drawn in the UI.
        texturePropertyDrawOrder = new List<string>
        {
            "_BaseMap", "_MainTex", "_MetallicGlossMap", "_SpecGlossMap", "_BumpMap", "_ParallaxMap", "_HeightMap", "_OcclusionMap", "_EmissionMap"
        };
        
        // Defines user-friendly names for shader properties.
        propertyDisplayNames = new Dictionary<string, string>()
        {
            {"_BaseMap", "Base Map (Albedo)"},
            {"_MainTex", "Base Map (Albedo)"},
            {"_MetallicGlossMap", "Metallic"},
            {"_SpecGlossMap", "Specular"},
            {"_BumpMap", "Normal Map"},
            {"_ParallaxMap", "Height Map"},
            {"_HeightMap", "Height Map"},
            {"_OcclusionMap", "Occlusion (AO)"},
            {"_EmissionMap", "Emission"}
        };
    }

    /// <summary>
    /// Draws the UI for the Texture Linker tab.
    /// </summary>
    private void DrawTextureLinkerTab()
    {
        EditorGUILayout.HelpBox("Automatically find and link textures for materials on the current model.", MessageType.Info);
        EditorGUILayout.HelpBox("Warning: Features in this tab directly modify your project's Material assets. It is highly recommended to use version control or create backups.", MessageType.Warning);
        
        if (activeEditingCopy == null)
        {
            EditorGUILayout.HelpBox("Load a model in the 'Hierarchy Editor' tab first.", MessageType.Warning);
            return;
        }

        // --- Search Folder Field ---
        EditorGUILayout.BeginHorizontal();
        searchFolder = EditorGUILayout.ObjectField("Search Folder (Optional)", searchFolder, typeof(DefaultAsset), false);
        if (GUILayout.Button("X", GUILayout.Width(22))) { searchFolder = null; }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
        
        // --- Button Toolbar ---
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto-link Textures")) { AutoLinkTextures(); }
        GUI.backgroundColor = new Color(0.7f, 1f, 0.8f);
        if (GUILayout.Button("Extract Materials")) { ExtractAllMaterials(); }
        GUI.backgroundColor = new Color(1f, 0.9f, 0.6f);
        if (GUILayout.Button("Restore Original Textures")) { RestoreOriginalTextures(); }
        GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
        if (GUILayout.Button("Clear All Textures")) { ClearAllTextures(); }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        // --- Material Dashboard ---
        EditorGUILayout.Space();
        GUILayout.Label("Material & Texture Dashboard", EditorStyles.boldLabel);
        
        materialScrollPos = EditorGUILayout.BeginScrollView(materialScrollPos, GUI.skin.box);
        var allRenderers = activeEditingCopy.GetComponentsInChildren<Renderer>(true);
        var uniqueMaterials = allRenderers.SelectMany(r => r.sharedMaterials).Distinct().Where(m => m != null).ToList();

        if (uniqueMaterials.Count == 0) { GUILayout.Label("No materials found on model."); }
        
        foreach (var mat in uniqueMaterials)
        {
            DrawMaterialEditor(mat);
        }
        
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Draws the editor for a single material in the dashboard.
    /// </summary>
    private void DrawMaterialEditor(Material mat)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        bool isEmbedded = AssetDatabase.IsSubAsset(mat);

        EditorGUILayout.BeginHorizontal();

        if (isEmbedded)
        {
            EditorGUILayout.LabelField("Name", mat.name);
        }
        else
        {
            // Allow renaming of external materials
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.TextField("Name", mat.name);
            if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newName) && newName != mat.name)
            {
                string assetPath = AssetDatabase.GetAssetPath(mat);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    string error = AssetDatabase.RenameAsset(assetPath, newName);
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError($"Failed to rename material '{mat.name}': {error}");
                    }
                }
            }
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                EditorGUIUtility.PingObject(mat);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        if (isEmbedded)
        {
            EditorGUILayout.HelpBox("This is an embedded material. To edit its name, first extract it using the 'Extract Materials' button.", MessageType.Info);
        }
        
        EditorGUILayout.Space(2);
        
        // Draw texture slots
        Undo.RecordObject(mat, "Edit Material Textures");
        bool albedoDrawn = false; // Flag to prevent drawing duplicate albedo slots
        foreach (string propertyName in texturePropertyDrawOrder)
        {
            if (mat.HasProperty(propertyName))
            {
                bool isAlbedoProperty = (propertyName == "_BaseMap" || propertyName == "_MainTex");

                // If this is an albedo property and we've already drawn one, skip it.
                if (isAlbedoProperty && albedoDrawn)
                {
                    continue;
                }

                string propertyDesc = propertyDisplayNames.ContainsKey(propertyName) ? propertyDisplayNames[propertyName] : propertyName;
                Texture currentTexture = mat.GetTexture(propertyName);
                
                EditorGUI.BeginChangeCheck();
                Texture newTexture = (Texture)EditorGUILayout.ObjectField(propertyDesc, currentTexture, typeof(Texture), false);
                if (EditorGUI.EndChangeCheck())
                {
                    mat.SetTexture(propertyName, newTexture);
                }

                // If we just drew an albedo map, set the flag so we don't draw another.
                if (isAlbedoProperty)
                {
                    albedoDrawn = true;
                }
            }
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    // --- Core Texture Logic ---

    private void AutoLinkTextures()
    {
        if (activeEditingCopy == null) return;
        Undo.SetCurrentGroupName("Auto-link Textures");

        var allRenderers = activeEditingCopy.GetComponentsInChildren<Renderer>(true);
        var uniqueMaterials = allRenderers.SelectMany(r => r.sharedMaterials).Distinct().Where(m => m != null).ToList();
        
        int linkCount = 0;
        int missingCount = 0;
        
        string[] searchPaths = null;
        if (searchFolder != null)
        {
            searchPaths = new string[] { AssetDatabase.GetAssetPath(searchFolder) };
        }
        
        string[] allTextureGuids = AssetDatabase.FindAssets("t:Texture", searchPaths);
        var allTextures = allTextureGuids
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .ToDictionary(p => p, p => Tokenize(Path.GetFileNameWithoutExtension(p)));

        foreach (var mat in uniqueMaterials)
        {
            var materialTokens = Tokenize(mat.name);
            var singularMaterialTokens = materialTokens.Select(t => t.EndsWith("s") ? t.Substring(0, t.Length - 1) : t).ToList();
            
            Shader shader = mat.shader;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv) continue;
                
                string propertyName = ShaderUtil.GetPropertyName(shader, i);
                if (mat.GetTexture(propertyName) != null) continue;
                
                if (!textureSuffixMap.ContainsKey(propertyName)) continue;
                
                missingCount++;

                string bestMatchPath = FindBestTextureMatch(materialTokens, singularMaterialTokens, textureSuffixMap[propertyName], allTextures);

                if (!string.IsNullOrEmpty(bestMatchPath))
                {
                    Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(bestMatchPath);
                    Undo.RecordObject(mat, "Assign Texture");
                    mat.SetTexture(propertyName, texture);
                    linkCount++;
                }
            }
        }
        
        EditorUtility.DisplayDialog("Auto-link Report", $"Process complete.\n\nSuccessfully linked {linkCount} of {missingCount} missing textures.", "OK");
    }

    private void ExtractAllMaterials()
    {
        if (activeEditingCopy == null || sourceObject == null) return;

        string sourcePath = AssetDatabase.GetAssetPath(sourceObject);
        if (string.IsNullOrEmpty(sourcePath)) return;

        var allRenderers = activeEditingCopy.GetComponentsInChildren<Renderer>(true);
        var uniqueMaterials = allRenderers.SelectMany(r => r.sharedMaterials).Distinct().Where(m => m != null).ToList();
        var embeddedMaterials = uniqueMaterials.Where(m => AssetDatabase.IsSubAsset(m)).ToList();

        if (embeddedMaterials.Count == 0)
        {
            EditorUtility.DisplayDialog("No Embedded Materials", "No embedded materials were found on this model to extract.", "OK");
            return;
        }

        string sourceDirectory = Path.GetDirectoryName(sourcePath);
        string materialsFolder = Path.Combine(sourceDirectory, "Materials");
        if (!AssetDatabase.IsValidFolder(materialsFolder))
        {
            AssetDatabase.CreateFolder(sourceDirectory, "Materials");
        }

        Undo.SetCurrentGroupName("Extract All Materials");
        
        var extractionMap = new Dictionary<Material, Material>();

        foreach (var embeddedMat in embeddedMaterials)
        {
            if (extractionMap.ContainsKey(embeddedMat)) continue;

            Material newMat = new Material(embeddedMat);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(materialsFolder, embeddedMat.name + ".mat"));
            
            AssetDatabase.CreateAsset(newMat, newPath);
            extractionMap[embeddedMat] = newMat;
        }

        // After extraction, we need to update the original texture cache to point to the new material instances
        var newOriginalTextureCache = new Dictionary<Material, Dictionary<string, Texture>>();
        foreach(var pair in originalTextureCache)
        {
            if (extractionMap.TryGetValue(pair.Key, out Material newMat))
            {
                newOriginalTextureCache[newMat] = pair.Value;
            } else {
                newOriginalTextureCache[pair.Key] = pair.Value;
            }
        }
        originalTextureCache = newOriginalTextureCache;


        foreach (var renderer in allRenderers)
        {
            var currentSharedMaterials = renderer.sharedMaterials;
            var newSharedMaterials = new Material[currentSharedMaterials.Length];
            bool needsUpdate = false;
            
            for (int i = 0; i < currentSharedMaterials.Length; i++)
            {
                if (currentSharedMaterials[i] != null && extractionMap.TryGetValue(currentSharedMaterials[i], out Material newMat))
                {
                    newSharedMaterials[i] = newMat;
                    needsUpdate = true;
                }
                else
                {
                    newSharedMaterials[i] = currentSharedMaterials[i];
                }
            }

            if (needsUpdate)
            {
                Undo.RecordObject(renderer, "Assign Extracted Material");
                renderer.sharedMaterials = newSharedMaterials;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Successfully extracted {extractionMap.Count} material(s) to '{materialsFolder}'.");
    }

    private void RestoreOriginalTextures()
    {
        if (activeEditingCopy == null || originalTextureCache == null) return;
        if (!EditorUtility.DisplayDialog("Restore Original Textures?",
            "Are you sure you want to revert all texture assignments on this model to their original state (from when the model was first loaded)? This can be undone.",
            "Yes, Restore", "Cancel"))
        {
            return;
        }

        Undo.SetCurrentGroupName("Restore Original Textures");

        foreach (var materialCachePair in originalTextureCache)
        {
            Material mat = materialCachePair.Key;
            if (mat == null) continue;

            Undo.RecordObject(mat, "Restore Textures on " + mat.name);
            Dictionary<string, Texture> textureProperties = materialCachePair.Value;

            foreach (var texturePropertyPair in textureProperties)
            {
                if (mat.HasProperty(texturePropertyPair.Key))
                {
                    mat.SetTexture(texturePropertyPair.Key, texturePropertyPair.Value);
                }
            }
        }
        Debug.Log("Restored original texture assignments.");
    }

    private void ClearAllTextures()
    {
        if (activeEditingCopy == null) return;
        if (!EditorUtility.DisplayDialog("Clear All Textures?",
            "Are you sure you want to remove all texture assignments from all materials on this model? This action can be undone.",
            "Yes, Clear All", "Cancel"))
        {
            return;
        }

        var allRenderers = activeEditingCopy.GetComponentsInChildren<Renderer>(true);
        var uniqueMaterials = allRenderers.SelectMany(r => r.sharedMaterials).Distinct().Where(m => m != null).ToList();

        Undo.SetCurrentGroupName("Clear All Textures");
        foreach (var mat in uniqueMaterials)
        {
            Undo.RecordObject(mat, "Clear Textures");
            Shader shader = mat.shader;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    mat.SetTexture(propertyName, null);
                }
            }
        }
        Debug.Log("Cleared all texture assignments.");
    }

    private void CacheOriginalTextures()
    {
        originalTextureCache = new Dictionary<Material, Dictionary<string, Texture>>();
        if (activeEditingCopy == null) return;

        var allRenderers = activeEditingCopy.GetComponentsInChildren<Renderer>(true);
        var uniqueMaterials = allRenderers.SelectMany(r => r.sharedMaterials).Distinct().Where(m => m != null).ToList();

        foreach (var mat in uniqueMaterials)
        {
            var textureProperties = new Dictionary<string, Texture>();
            Shader shader = mat.shader;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    textureProperties[propertyName] = mat.GetTexture(propertyName);
                }
            }
            originalTextureCache[mat] = textureProperties;
        }
    }

    private string FindBestTextureMatch(List<string> pluralTokens, List<string> singularTokens, List<string> slotSuffixes, Dictionary<string, List<string>> allTextures)
    {
        var scores = new Dictionary<string, int>();

        foreach (var textureInfo in allTextures)
        {
            string texPath = textureInfo.Key;
            List<string> texTokens = textureInfo.Value;
            int currentScore = 0;

            if (pluralTokens.All(texTokens.Contains)) currentScore += 10;
            else if (singularTokens.All(texTokens.Contains)) currentScore += 9;
            
            if (slotSuffixes.Any(texTokens.Contains)) currentScore += 10;
            
            if (currentScore > 10) // Must match both material name AND suffix type
            {
                scores[texPath] = currentScore;
            }
        }
        
        if (scores.Count > 0)
        {
            int maxScore = scores.Values.Max();
            var topMatches = scores.Where(p => p.Value == maxScore).ToList();

            if (topMatches.Count > 0)
            {
                // If there's a tie, prefer the match with the shortest filename.
                return topMatches.OrderBy(p => p.Key.Length).First().Key;
            }
        }
        
        return null;
    }

    private List<string> Tokenize(string input)
    {
        string processed = Regex.Replace(input, "([a-z])([A-Z])", "$1 $2").ToLower();
        processed = Regex.Replace(processed, @"[\s_-]", " ");
        processed = Regex.Replace(processed, @"(^t_)|(_mat|mat_|_inst$)", "", RegexOptions.IgnoreCase);
        return processed.Split(new[] {' '}, System.StringSplitOptions.RemoveEmptyEntries).ToList();
    }
    }
}
