// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

namespace ServerData
{
    public class CFreeValuable : IValuable
    {
        public EValuable Id => EValuable.Free;
        
        public static readonly CFreeValuable Instance = new();

        private CFreeValuable()
        {
        }
    }
}