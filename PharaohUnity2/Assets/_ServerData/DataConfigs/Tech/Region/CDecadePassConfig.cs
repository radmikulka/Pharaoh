// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.07.2025
// =========================================

using System;

namespace ServerData
{
	public class CDecadePassConfig : CLazyConfig<CDecadePassConfigData>
	{
		public CDecadePassConfig(Func<CDecadePassConfigData> getter) : base(getter)
		{
		}
	}
}