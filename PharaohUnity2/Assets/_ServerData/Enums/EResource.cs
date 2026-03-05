// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.06.2025
// =========================================

namespace ServerData
{
	public enum EResource
	{
		None = 0,

		// 1 - 4999 - Raw Materials
		Coal = 1,
		IronOre = 2,
		Wood = 3,
		CrudeOil = 4,
		LeadOre = 5,
		Livestock = 6,
		Water = 7,
		


		// 5000-5999 - Products
		Steel = 5000,
		Lumber = 5001,
		Tools = 5002,
		SteelRope = 5003,
		Crate = 5004,
		Gasoline = 5005,
		LeadIngots = 5006,
        Batteries = 5007,
        Meat = 5008,
        CannedFood = 5009,
        Engines = 5010,
        //Furniture = 5011,
        //Clothes = 5012,
		Nails = 5013,
		Paper = 5019,
		
        // Earth & Fire event 6000-6009
		Neoprene = 6000,
        FireClothes = 6001,
        FireHoses = 6002,
        WaterBarrels = 6003,
        ReinforcedBeams = 6004,
        Scaffold = 6005, // nepouzivam

        // Banking Tycoon event 6010-6019
        MolybdenumOre = 6010,
        BallisticSteel = 6011,
        BulletproofPlates = 6012,
        BankFurniture = 6013,
        Vaults = 6014,
        Locks = 6015,

        // 10000 - Others
        Passenger = 10_000,
		
		SmallFuelBarrel = 20_000,
		MediumFuelBarrel = 20_001,
		BigFuelBarrel = 20_002,
		
		SmallPassengerPack = 20_100,
		MediumPassengerPack = 20_101,
		LargePassengerPack = 20_102,
	}
}