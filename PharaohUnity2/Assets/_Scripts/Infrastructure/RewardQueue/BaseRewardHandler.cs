// =========================================
// AUTHOR: Marek Karaba
// DATE:   18.07.2025
// =========================================

using ServerData;

namespace TycoonBuilder.Infrastructure
{
	public class BaseRewardHandler
	{
		protected bool AreSameType(IValuable a, IValuable b)
		{
			return a.GetType() == b.GetType();
		}
	}
}