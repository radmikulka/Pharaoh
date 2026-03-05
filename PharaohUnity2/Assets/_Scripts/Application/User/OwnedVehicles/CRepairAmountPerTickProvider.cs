// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using ServerData.Design;

namespace TycoonBuilder
{
	public class CRepairAmountPerTickProvider
	{
		private readonly CUser _user;

		public CRepairAmountPerTickProvider(CUser user)
		{
			_user = user;
		}

		public int GetValue()
		{
			int result = _user.VehicleDepo.GetVehicleRepairAmount();
			return result;
		}
	}
}