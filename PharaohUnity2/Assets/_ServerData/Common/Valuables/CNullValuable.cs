// =========================================
// AUTHOR: Radek Mikulka
// DATE:   28.07.2025
// =========================================

namespace ServerData
{
	public class CNullValuable : IValuable
	{
		public EValuable Id => EValuable.Null;
        
		public static readonly CNullValuable Instance = new();

		private CNullValuable()
		{
		}
	}
}