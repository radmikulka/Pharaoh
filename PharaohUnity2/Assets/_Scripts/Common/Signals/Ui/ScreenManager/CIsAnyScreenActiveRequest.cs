// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

namespace TycoonBuilder
{
	public class CIsAnyScreenActiveRequest
	{
		
	}
	
	public class CIsAnyScreenActiveResponse
	{
		public readonly bool IsActive;
		
		public CIsAnyScreenActiveResponse(bool isActive)
		{
			IsActive = isActive;
		}
	}
}