// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.09.2025
// =========================================

namespace ServerData
{
	public class CPassengerContractConfig : CContractConfig
	{
		public readonly ECity City;

		public CPassengerContractConfig(ECity city, CContractTask task, CTripPrice tripPrice) : base(new []{task}, tripPrice)
		{
			City = city;
		}
	}
}