// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.12.2025
// =========================================

using System.IO;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/Region")]
	public class CRegionResourceConfig : ScriptableObject, IResourceConfigBase<ERegion>
	{
		[SerializeField] [SearchableEnum] private ERegion _id;
		[SerializeField] [SearchableEnum] private ESceneId _mainScene;

		public ESceneId MainScene => _mainScene;
		public ERegion Id => _id;
	}
}