// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.11.2025
// =========================================

using UnityEngine.UI;

namespace TycoonBuilder
{
	public class CGetUiScrollRectRequest
	{
		public readonly EUiScrollRect ScrollRectId;

		public CGetUiScrollRectRequest(EUiScrollRect scrollRectId)
		{
			ScrollRectId = scrollRectId;
		}
	}
	
	public class CGetUiScrollRectResponse
	{
		public readonly ScrollRect ScrollRect;

		public CGetUiScrollRectResponse(ScrollRect scrollRect)
		{
			ScrollRect = scrollRect;
		}
	}
}