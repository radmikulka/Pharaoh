// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

namespace ServerData
{
    public enum EOfferTag
    {
        None = 0,
        // 1-1000 Vyhrazeno pro system tagy
        // 1 - free
        WeekFlashSale = 2,
        DecadePassPremium = 3,
        DecadePassExtraPremium = 4,
        EventPassPremium = 5,
        EventPassExtraPremium = 6,
        DecadePassExtraPremiumUpgrade = 7,
        EventPassExtraPremiumUpgrade = 8,

		// 1001-2000 - Vyhrazeno pro one time offery
		FirstSoftCurrencySmallPack = 1001,
	}
}