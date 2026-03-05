// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.10.2025
// =========================================

namespace TycoonBuilder
{
	public class CIsTutorialArrowActiveRequest
	{
		
	}

	public class CIsTutorialArrowActiveResponse
	{
		public readonly bool IsActive;
		
		public CIsTutorialArrowActiveResponse(bool isActive)
		{
			IsActive = isActive;
		}
	}
}