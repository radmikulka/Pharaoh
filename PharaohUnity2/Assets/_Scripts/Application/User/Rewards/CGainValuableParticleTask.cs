// =========================================
// AUTHOR: Marek Karaba
// DATE:   21.07.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CGainValuableParticleTask
	{
		public readonly IValuable Valuable;
		public readonly CValueModifyParams ModifyParams;

		public CGainValuableParticleTask(IValuable valuable, CValueModifyParams modifyParams)
		{
			Valuable = valuable;
			ModifyParams = modifyParams;
		}
	}
}