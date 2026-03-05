// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System;
using System.IO;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;
using UnityEngine.Serialization;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/Scene")]
	public class CSceneResourceConfig : ScriptableObject, IResourceConfigBase<ESceneId>
	{
		[SerializeField] [SearchableEnum] private ESceneId _id;
		[SerializeField] [SearchableEnum] private EBundleId[] _bundleIds;
		[SerializeField] private string _scenePath;

		public EBundleId[] BundleIds => _bundleIds;
		public string SceneName => Path.GetFileName(_scenePath);
		public string ScenePath => _scenePath;
		public ESceneId Id => _id;
	}
}

