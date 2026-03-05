// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.10.2025
// =========================================

namespace TycoonBuilder
{
	public class CGetSmartArrowRequest
	{
		
	}
	
	public class CGetSmartArrowResponse
	{
		public readonly ISmartArrow SmartArrow;
		
		public CGetSmartArrowResponse(ISmartArrow smartArrow)
		{
			SmartArrow = smartArrow;
		}
	}
}