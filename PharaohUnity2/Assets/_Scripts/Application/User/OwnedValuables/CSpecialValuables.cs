// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using AldaEngine;
using ServerData;
using UnityEngine;

namespace Pharaoh
{
	public class CSpecialValuables
	{

		public bool TryHandleSpecialValuable(IValuable valuable, CUser user, long timestampInMs, CValueModifyParams modifyParams)
		{
			return false;
		}

		public bool? HaveValuable(IValuable valuable, CUser user, long timestamp)
		{
			return null;
		}
	}
}