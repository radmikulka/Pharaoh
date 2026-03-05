// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.09.2025
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public class CGetUiRectRequest
	{
		public readonly EUiRect RectId;

		public CGetUiRectRequest(EUiRect rectId)
		{
			RectId = rectId;
		}
	}
	
	public class CGetUiRectResponse
	{
		public readonly RectTransform RectTransform;

		public CGetUiRectResponse(RectTransform rectTransform)
		{
			RectTransform = rectTransform;
		}
	}
}