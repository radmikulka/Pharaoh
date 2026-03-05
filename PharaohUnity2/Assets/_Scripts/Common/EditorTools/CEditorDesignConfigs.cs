// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.08.2025
// =========================================

using ServerData;
using ServerData.Design;
using UnityEngine;

namespace TycoonBuilder
{
	#if UNITY_EDITOR
	public static class CStaticConfigs
	{
		public static CDesignSpecialBuildingConfigs Buildings;
		public static CDesignStoryContractConfigs Contracts;
		public static CLiveEventBundles LiveEventBundles;
		public static CDesignIndustryConfigs Industry;
		public static CDesignFactoryConfigs Factories;
		public static CDesignVehicleConfigs Vehicle;
		public static CDesignRegionConfigs Regions;

		static CStaticConfigs()
		{
			LiveEventBundles = new();
			Buildings = new();
			Contracts = new();
			Industry = new();
			Regions = new();
			Factories = new();
			
			CEditorVehicleBuilderFactory editorVehicleBuilderFactory = new(Regions);
			Vehicle = new(Regions, editorVehicleBuilderFactory);
		}
		
		private class CEditorVehicleBuilderFactory : IVehicleBuilderFactory
		{
			private readonly CDesignRegionConfigs _regionConfigs;

			public CEditorVehicleBuilderFactory(CDesignRegionConfigs regionConfigs)
			{
				_regionConfigs = regionConfigs;
			}

			public CVehicleBuilder GetNewVehicleBuilder()
			{
				return new CVehicleBuilder(_regionConfigs);
			}
		}
	}
	#endif
}