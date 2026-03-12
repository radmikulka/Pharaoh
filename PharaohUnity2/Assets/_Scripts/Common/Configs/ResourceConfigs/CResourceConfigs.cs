// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;

namespace Pharaoh
{
	[CreateAssetMenu(menuName = "____Pharaoh/Configs/ResourceConfigs")]
	public class CResourceConfigs : CResourceConfigsDb, IConstructable
	{
		public CResourceConfigsSet<CValuableResourceConfig, EValuable> Valuables { get; private set; }
		public CResourceConfigsSet<CSceneResourceConfig, ESceneId> Scenes { get; private set; }
		public CResourceConfigsSet<CMissionResourceConfig, EMissionId> Missions { get; private set; }
		public CResourceConfigsSet<CMonumentResourceConfig, EMonumentId> Monuments { get; private set; }

		public void Construct()
		{
			Scenes = GetConfigs<CResourceConfigsSet<CSceneResourceConfig, ESceneId>, CSceneResourceConfig>();
			Valuables = GetConfigs<CResourceConfigsSet<CValuableResourceConfig, EValuable>, CValuableResourceConfig>();
			Missions = GetConfigs<CResourceConfigsSet<CMissionResourceConfig, EMissionId>, CMissionResourceConfig>();
			Monuments = GetConfigs<CResourceConfigsSet<CMonumentResourceConfig, EMonumentId>, CMonumentResourceConfig>();
		}
	}
}