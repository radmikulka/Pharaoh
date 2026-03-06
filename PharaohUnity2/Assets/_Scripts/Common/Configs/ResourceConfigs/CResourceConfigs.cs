// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/ResourceConfigs")]
	public class CResourceConfigs : CResourceConfigsDb, IConstructable
	{
		public CResourceConfigsSet<CValuableResourceConfig, EValuable> Valuables { get; private set; }
		public CResourceConfigsSet<CSceneResourceConfig, ESceneId> Scenes { get; private set; }
		
		public void Construct()
		{
			Scenes = GetConfigs<CResourceConfigsSet<CSceneResourceConfig, ESceneId>, CSceneResourceConfig>();
			Valuables = GetConfigs<CResourceConfigsSet<CValuableResourceConfig, EValuable>, CValuableResourceConfig>();
		}
	}
}