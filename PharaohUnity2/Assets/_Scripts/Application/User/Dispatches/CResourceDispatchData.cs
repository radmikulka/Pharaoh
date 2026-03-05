// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.02.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CResourceDispatchData
	{
		public readonly SResource ResourceToCollect;

		public CResourceDispatchData(SResource resourceToCollect)
		{
			ResourceToCollect = resourceToCollect;
		}
	}
}
