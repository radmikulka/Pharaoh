// =========================================
// AUTHOR: Juraj Joscak
// DATE:   16.02.2026
// =========================================

namespace TycoonBuilder
{
	public enum ECityMenuTab
	{
		None = 0,
		Passengers = 1,
		TransportContracts = 2,
	}
	
	public class COpenCityMenuTask
	{
		public readonly ECityMenuTab Tab;
		
		public COpenCityMenuTask(ECityMenuTab tab)
		{
			Tab = tab;
		}
	}
}