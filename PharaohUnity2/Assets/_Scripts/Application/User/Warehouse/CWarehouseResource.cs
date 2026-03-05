// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.09.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	[ValidatableData]
	public class CWarehouseResource
	{
		public readonly CResourceConfig Config;
		[ValidatableData] public readonly CUserDataInt Amount;
		
		public EResource Id => Config.Id;
		public bool CountToWarehouseCapacity => Config.CountToWarehouseCapacity;
		public SResource Resource => new(Id, Amount);

		public CWarehouseResource(CResourceConfig config, int amount)
		{
			Config = config;
			Amount = new CUserDataInt(amount);
		}
		
		public static implicit operator int (CWarehouseResource resource) => resource.Amount;
	}
}