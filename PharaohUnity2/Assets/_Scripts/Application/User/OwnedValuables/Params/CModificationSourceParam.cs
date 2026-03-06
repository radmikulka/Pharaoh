// =========================================
// AUTHOR: Radek Mikulka
// DATE:   17.09.2025
// =========================================

using ServerData;

namespace Pharaoh
{
	public class CModificationSourceParam : IValueModifyParam
	{
		public readonly EModificationSource Source;

		public CModificationSourceParam(EModificationSource source)
		{
			Source = source;
		}
	}
}