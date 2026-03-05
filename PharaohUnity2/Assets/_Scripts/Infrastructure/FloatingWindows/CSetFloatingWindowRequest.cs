// =========================================
// AUTHOR: Marek Karaba
// DATE:   03.07.2025
// =========================================

using AldaEngine;
using UnityEngine;

namespace TycoonBuilder
{
	public class CSetFloatingWindowRequest
	{
		public IFloatingWindowData FloatingWindowData { get; private set; }

		public CSetFloatingWindowRequest(IFloatingWindowData floatingWindowData)
		{
			FloatingWindowData = floatingWindowData;
		}
	}
}