// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Numerics;

namespace ServerData
{
    public static class CValuableFactory
    {
        public static CEventCoinValuable EventCoin(ELiveEvent liveEvent, int count) => new(liveEvent, count);
        public static CEventPointValuable EventPoint(ELiveEvent liveEvent, int count) => new(liveEvent, count);
        public static CConsumableValuable SoftCurrency(int count) => new(EValuable.SoftCurrency, count);
        public static CConsumableValuable HardCurrency(int count) => new(EValuable.HardCurrency, count);
        public static CConsumableValuable Fuel(int count) => new(EValuable.Fuel, count);
        public static CResourceValuable Passenger(int count) => new(EResource.Passenger, count);
        public static CConsumableValuable Consumable(EValuable id, int count) => new(id, count);
        public static CConsumableValuable CityBlueprint(int count) => new(EValuable.CityBlueprint, count);
        public static CFrameValuable Frame(EProfileFrame frame) => new(frame);
        public static CConsumableValuable FuelPart(int count) => new(EValuable.FuelPart, count);
        public static CConsumableValuable DurabilityPart(int count) => new(EValuable.DurabilityPart, count);
        public static CBuildingValuable Building(ESpecialBuilding id) => new(id);
        public static CConsumableValuable CapacityPart(int count) => new(EValuable.CapacityPart, count);
        public static CConsumableValuable AdvancedCapacityPart(int count) => new(EValuable.AdvancedCapacityPart, count);
        public static CConsumableValuable MachineOil(int count) => new(EValuable.MachineOil, count);
        public static CConsumableValuable Wrenche(int count) => new(EValuable.Wrenche, count);
        public static CRealMoneyValuable RealMoney(EInAppPrice price) { return new(price); }
        public static CDispatcherValuable Dispatcher(EDispatcher id, long? expirationDurationIsSecs = null) { return new(id, expirationDurationIsSecs); }
        public static CXpValuable Xp(int amount) { return new(amount); }
        public static CVehicleValuable Vehicle(EVehicle vehicle) { return new(vehicle); }
        public static CResourceValuable Resource(EResource id, int amount = 1) { return new(id, amount); }
        public static CAdvertisementValuable Advertisement(EAdPlacement placement) { return new(placement); }
        public static CEventRewardValuable EventReward(EEventRewardPlacement placement) { return new(placement); }

        public static CFreeNoHitValuable FreeNoHit => CFreeNoHitValuable.Instance;
        public static CFreeValuable Free => CFreeValuable.Instance;
        public static CNullValuable Null => CNullValuable.Instance;
    }
}