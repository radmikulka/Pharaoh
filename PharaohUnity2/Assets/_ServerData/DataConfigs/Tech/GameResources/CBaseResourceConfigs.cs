// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.07.2025
// =========================================

using System.Collections.Generic;
using System.Linq;

namespace ServerData
{
	public class CBaseResourceConfigs
	{
		private readonly Dictionary<EResource, CResourceConfig> _configs = new();
		
		protected void AddResource(EResource id, EWarehouseCategory type, ETransportType transportType, EYearMilestone unlockYear, ELiveEvent liveEvent = ELiveEvent.None)
		{
			_configs.Add(id, new CResourceConfig(id, type, transportType, IUnlockRequirement.Year(unlockYear), null, liveEvent));
		}
		
		protected void AddResource(EResource id, EWarehouseCategory type, ETransportType transportType, ELiveEvent liveEvent = ELiveEvent.None)
		{
			_configs.Add(id, new CResourceConfig(id, type, transportType, IUnlockRequirement.Null(), null, liveEvent));
		}
		
		protected void AddResource(EResource id, EWarehouseCategory type, ETransportType transportType, EStaticContractId unlockContract, ELiveEvent liveEvent = ELiveEvent.None)
		{
			_configs.Add(id, new CResourceConfig(id, type, transportType, IUnlockRequirement.Contract(unlockContract), null, liveEvent));
		}
		
		protected void AddConsumable(EResource id, params IValuable[] rewards)
		{
			_configs.Add(id, new CResourceConfig(
				id, 
				EWarehouseCategory.Consumable, 
				ETransportType.None, 
				IUnlockRequirement.Null(),
				new IResourceConfigComponent[]{new CConsumableResourceData(rewards)},
				ELiveEvent.None)
			);
		}
		
		public CResourceConfig GetResourceConfig(EResource id)
		{
			return _configs[id];
		}
		
		public CResourceConfig[] GetAllResourceConfigs()
		{
			return _configs.Values.ToArray();
		}
	}
}