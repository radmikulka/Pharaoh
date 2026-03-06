// =========================================
// AUTHOR: Marek Karaba
// DATE:   21.08.2025
// =========================================

using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class COwnedValuableChangedSignal : IEventBusSignal
	{
		public readonly IValuable Valuable;

		public COwnedValuableChangedSignal(IValuable valuable)
		{
			Valuable = valuable;
		}
	}
}