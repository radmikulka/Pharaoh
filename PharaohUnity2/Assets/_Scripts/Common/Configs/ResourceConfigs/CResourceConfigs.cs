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
		public CResourceConfigsSet<CSpecialBuildingResourceConfig, ESpecialBuilding> SpecialBuildings { get; private set; }
		public CResourceConfigsSet<CVehicleResourceConfig, EVehicle> Vehicles { get; private set; }
		public CResourceConfigsSet<CValuableResourceConfig, EValuable> Valuables { get; private set; }
		public CResourceConfigsSet<CResourceResourceConfig, EResource> Resources { get; private set; }
		public CResourceConfigsSet<CSceneResourceConfig, ESceneId> Scenes { get; private set; }
		public CResourceConfigsSet<CRegionDialogueConfig, ERegion> RegionDialogueConfigs { get; private set; }
		public CResourceConfigsSet<CDialogueCharacterConfig, EDialogueCharacter> DialogueCharacters { get; private set; }
		public CResourceConfigsSet<CDialoguePictureConfig, EDialoguePictureId> DialoguePictureConfigs { get; private set; }
		public CResourceConfigsSet<CDecadeMilestoneResourceConfig, EDecadeMilestone> DecadeMilestoneConfigs { get; private set; }
		public CResourceConfigsSet<CProfileFrameConfig, EProfileFrame> ProfileFrames { get; private set; }
		public CResourceConfigsSet<CProfileAvatarConfig, EProfileAvatar> ProfileAvatarConfigs { get; private set; }
		public CResourceConfigsSet<CCountryFlagConfig, ECountryCode> CountryFlagConfigs { get; private set; }
		public CResourceConfigsSet<CTransportTypeConfig, ETransportType> TransportTypeConfigs { get; private set; }
		public CResourceConfigsSet<CCustomerConfig, ECustomer> CustomerConfigs { get; private set; }
		public CResourceConfigsSet<CMovementTypeConfig, EMovementType> MovementTypeConfigs { get; private set; }
		public CResourceConfigsSet<CFactoryResourceConfig, EFactory> Factories { get; private set; }
		public CResourceConfigsSet<CSideCityConfig, ECity> SideCities { get; private set; }
		public CResourceConfigsSet<CIndustryResourceConfig, EIndustry> Industries { get; private set; }
		public CResourceConfigsSet<CRegionResourceConfig, ERegion> Regions { get; private set; }
		public CResourceConfigsSet<CLiveEventResourceConfig, ELiveEvent> LiveEvents { get; private set; }
		public CResourceConfigsSet<CDispatcherConfig, EDispatcher> Dispatchers { get; private set; }
		public CResourceConfigsSet<CDecadePassResourceConfig, EDecadeMilestone> DecadePassConfigs { get; private set; }
		public CResourceConfigsSet<CTaskResourceConfig, ETaskId> Tasks { get; private set; }
		
		public void Construct()
		{
			Regions = GetConfigs<CResourceConfigsSet<CRegionResourceConfig, ERegion>, CRegionResourceConfig>();
			Scenes = GetConfigs<CResourceConfigsSet<CSceneResourceConfig, ESceneId>, CSceneResourceConfig>();
			Valuables = GetConfigs<CResourceConfigsSet<CValuableResourceConfig, EValuable>, CValuableResourceConfig>();
			Resources = GetConfigs<CResourceConfigsSet<CResourceResourceConfig, EResource>, CResourceResourceConfig>();
			Vehicles = GetConfigs<CResourceConfigsSet<CVehicleResourceConfig, EVehicle>, CVehicleResourceConfig>();
			RegionDialogueConfigs = GetConfigs<CResourceConfigsSet<CRegionDialogueConfig, ERegion>, CRegionDialogueConfig>();
			DialogueCharacters = GetConfigs<CResourceConfigsSet<CDialogueCharacterConfig, EDialogueCharacter>, CDialogueCharacterConfig>();
			DialoguePictureConfigs = GetConfigs<CResourceConfigsSet<CDialoguePictureConfig, EDialoguePictureId>, CDialoguePictureConfig>();
			DecadeMilestoneConfigs = GetConfigs<CResourceConfigsSet<CDecadeMilestoneResourceConfig, EDecadeMilestone>, CDecadeMilestoneResourceConfig>();
			ProfileFrames = GetConfigs<CResourceConfigsSet<CProfileFrameConfig, EProfileFrame>, CProfileFrameConfig>();
			ProfileAvatarConfigs = GetConfigs<CResourceConfigsSet<CProfileAvatarConfig, EProfileAvatar>, CProfileAvatarConfig>();
			CountryFlagConfigs = GetConfigs<CResourceConfigsSet<CCountryFlagConfig, ECountryCode>, CCountryFlagConfig>();
			TransportTypeConfigs = GetConfigs<CResourceConfigsSet<CTransportTypeConfig, ETransportType>, CTransportTypeConfig>();
			CustomerConfigs = GetConfigs<CResourceConfigsSet<CCustomerConfig, ECustomer>, CCustomerConfig>();
			MovementTypeConfigs = GetConfigs<CResourceConfigsSet<CMovementTypeConfig, EMovementType>, CMovementTypeConfig>();
			Factories = GetConfigs<CResourceConfigsSet<CFactoryResourceConfig, EFactory>, CFactoryResourceConfig>();
			SpecialBuildings = GetConfigs<CResourceConfigsSet<CSpecialBuildingResourceConfig, ESpecialBuilding>, CSpecialBuildingResourceConfig>();
			SideCities = GetConfigs<CResourceConfigsSet<CSideCityConfig, ECity>, CSideCityConfig>();
			Industries = GetConfigs<CResourceConfigsSet<CIndustryResourceConfig, EIndustry>, CIndustryResourceConfig>();
			LiveEvents = GetConfigs<CResourceConfigsSet<CLiveEventResourceConfig, ELiveEvent>, GameObject>();
			Dispatchers = GetConfigs<CResourceConfigsSet<CDispatcherConfig, EDispatcher>, CDispatcherConfig>();
			DecadePassConfigs = GetConfigs<CResourceConfigsSet<CDecadePassResourceConfig, EDecadeMilestone>, CDecadePassResourceConfig>();
			Tasks = GetConfigs<CResourceConfigsSet<CTaskResourceConfig, ETaskId>, CTaskResourceConfig>();
		}
	}
}