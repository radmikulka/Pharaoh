// =========================================
// AUTHOR: Juraj Joscak
// DATE:   10.09.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class COpenFactoryTask
	{
		public readonly EFactory Factory;
		public readonly EResource Product;
		
		public COpenFactoryTask(EFactory factory, EResource product)
		{
			Factory = factory;
			Product = product;
		}
	}
}