// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.08.2025
// =========================================

namespace TycoonBuilder
{
	public class CDestroyFloatingWindowRequest
	{
		public IFloatingWindowData FloatingWindowData { get; private set; }

		public CDestroyFloatingWindowRequest(IFloatingWindowData floatingWindowData)
		{
			FloatingWindowData = floatingWindowData;
		}
	}
}