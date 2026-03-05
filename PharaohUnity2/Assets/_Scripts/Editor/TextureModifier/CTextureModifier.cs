using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CTextureModifier : UnityEditor.Editor
{
	private const string AndroidPlatform = "Android";
	private const int MaxTextureSize = 16384;

	[MenuItem("Assets/AldaGames/UpdateTexturesForScreenshot")]
	[MenuItem("GameObject/AldaGames/UpdateTexturesForScreenshot", false)]
	private static void ModifyPrefabTextures()
	{
		GameObject selected = Selection.activeGameObject;
		if (!selected)
			return;

		HashSet<string> processedPaths = CollectAndProcessUniqueTextures(selected);
		Debug.Log($"[CPrefabTextureModifier] Modified {processedPaths.Count} textures in {selected.name}.");
	}

	private static HashSet<string> CollectAndProcessUniqueTextures(GameObject prefab)
	{
		HashSet<string> processedPaths = new HashSet<string>();
		Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);

		foreach (Renderer renderer in renderers)
		{
			foreach (Material material in renderer.sharedMaterials)
			{
				if (!material)
					continue;

				CollectTexturesFromMaterial(material, processedPaths);
			}
		}
		return processedPaths;
	}

	private static void CollectTexturesFromMaterial(Material material, HashSet<string> processedPaths)
	{
		Shader shader = material.shader;
		int propertyCount = shader.GetPropertyCount();

		for (int i = 0; i < propertyCount; i++)
		{
			bool isTextureProperty = IsTextureProperty(shader, i);
			if (!isTextureProperty)
				continue;

			Texture texture = material.GetTexture(shader.GetPropertyName(i));
			TryProcessTexture(texture, processedPaths);
		}
	}

	private static bool IsTextureProperty(Shader shader, int propertyIndex)
	{
		bool isTextureProperty = shader.GetPropertyType(propertyIndex) == ShaderPropertyType.Texture;
		return isTextureProperty;
	}

	private static void TryProcessTexture(Texture texture, HashSet<string> processedPaths)
	{
		if (!texture)
			return;

		string assetPath = AssetDatabase.GetAssetPath(texture);
		if (string.IsNullOrEmpty(assetPath))
			return;

		if (!processedPaths.Add(assetPath))
			return;

		if (assetPath.Contains("/Skybox/"))
			return;

		ApplyTextureImportSettings(assetPath);
	}

	private static void ApplyTextureImportSettings(string path)
	{
		TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
		if (!importer)
			return;

		bool changed = false;
		changed |= TrySetAndroidMaxSize(importer);
		changed |= TryDisableMipmaps(importer);

		if (!changed)
			return;

		importer.SaveAndReimport();
	}

	private static bool TrySetAndroidMaxSize(TextureImporter importer)
	{
		TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(AndroidPlatform);

		bool needsOverride = !settings.overridden;
		bool needsSizeChange = settings.maxTextureSize != MaxTextureSize;

		if (!needsOverride && !needsSizeChange)
			return false;

		settings.overridden = true;
		settings.maxTextureSize = MaxTextureSize;
		importer.SetPlatformTextureSettings(settings);
		return true;
	}

	private static bool TryDisableMipmaps(TextureImporter importer)
	{
		if (!importer.mipmapEnabled)
			return false;

		importer.mipmapEnabled = false;
		return true;
	}
}