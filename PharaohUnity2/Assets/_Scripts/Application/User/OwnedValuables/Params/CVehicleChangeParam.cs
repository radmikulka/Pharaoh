// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.11.2025
// =========================================

namespace TycoonBuilder
{
	public class CVehicleChangeParam : IValueModifyParam
	{
        public readonly EVehicleObtainSource Source;

        public CVehicleChangeParam(EVehicleObtainSource source)
        {
	        Source = source;
        }
	}
}