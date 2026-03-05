// =========================================
// AUTHOR: Juraj Joscak
// DATE:   20.11.2025
// =========================================

namespace ServerData.Logging
{
	public interface ICurrencyDataLogging
	{
		void LogCurrencySourcesAndSinks(EValuable currencyType);
	}
}