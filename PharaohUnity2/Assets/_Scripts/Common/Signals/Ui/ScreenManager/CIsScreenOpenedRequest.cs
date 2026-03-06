// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.10.2025
// =========================================

using AldaEngine;

namespace Pharaoh
{
	public class CIsScreenOpenedRequest
	{
		public readonly EScreenId ScreenId;

		public CIsScreenOpenedRequest(EScreenId screenId)
		{
			ScreenId = screenId;
		}
	}

	public class CIsScreenOpenedResponse
	{
		public readonly bool IsActive;
		
		public CIsScreenOpenedResponse(bool isActive)
		{
			IsActive = isActive;
		}
	}
}