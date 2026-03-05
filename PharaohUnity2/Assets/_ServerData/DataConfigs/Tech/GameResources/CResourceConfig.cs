// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.07.2025
// =========================================

namespace ServerData
{
	public class CResourceConfig
	{
		private readonly IResourceConfigComponent[] _components;
		public readonly IUnlockRequirement UnlockRequirement;
		public readonly ETransportType TransportType;
		public readonly EWarehouseCategory Type;
		public readonly EResource Id;
		public readonly ELiveEvent LiveEvent;
		
		public bool CountToWarehouseCapacity => Type != EWarehouseCategory.Consumable && Id != EResource.Passenger;

		public CResourceConfig(
			EResource id, 
			EWarehouseCategory type, 
			ETransportType transportType, 
			IUnlockRequirement unlockRequirement,
			IResourceConfigComponent[] components,
			ELiveEvent liveEvent
			)
		{
			Id = id;
			Type = type;
			_components = components;
			TransportType = transportType;
			UnlockRequirement = unlockRequirement;
			LiveEvent = liveEvent;
		}
		
		public T GetComponentOrDefault<T>() where T : class, IResourceConfigComponent
		{
			if (_components == null)
				return null;
			
			foreach (IResourceConfigComponent component in _components)
			{
				if (component is T matchedComponent)
					return matchedComponent;
			}

			return null;
		}
	}
}