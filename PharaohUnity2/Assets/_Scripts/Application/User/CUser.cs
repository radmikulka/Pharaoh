// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;

namespace TycoonBuilder
{
    [ValidatableData]
    public class CUser : ITickable, IDestroyable, IInitializable
    {
        [ValidatableData] public readonly CRechargerValidator RechargerValidator;
        [ValidatableData] public readonly COwnedValuables OwnedValuables;
        [ValidatableData] public readonly CWarehouse Warehouse;
        public readonly CAnimatedCurrencies AnimatedCurrencies;
        public readonly CGlobalVariables GlobalVariables;
        public readonly CVehicleDepo VehicleDepo;
        public readonly CUserTutorials Tutorials;
        public readonly CFuelStation FuelStation;
        public readonly CDispatchers Dispatchers;
        public readonly COwnedFrames OwnedFrames;
        public readonly COwnedVehicles Vehicles;
        public readonly CLiveEvents LiveEvents;
        public readonly CDispatches Dispatches;
        public readonly CUserProgress Progress;
        public readonly CDecadePass DecadePass;
        public readonly CSideCities SideCities;
        public readonly CContracts Contracts;
        public readonly CFactories Factories;
        public readonly CDebugInfo DebugInfo;
        public readonly CAccount Account;
        public readonly COffers Offers;
        public readonly CTasks Tasks;
        public readonly CProjects Projects;
        public readonly CCity City;

        public bool IsValid { get; private set; }

        private readonly IEventBus _eventBus;
        private readonly CServerResponseDataSync _dataSync = new();
        private readonly List<CBaseUserComponent> _allComponents = new();

        public CUser(
            CAnimatedCurrencies animatedCurrencies,
            CRechargerValidator rechargerValidator,
            CInitialUserDtoProvider dtoProvider,
            CGlobalVariables globalVariables,
            COwnedValuables ownedValuables,
            CDispatchers dispatchers,
            CFuelStation fuelStation,
            CVehicleDepo vehicleDepo,
            CUserTutorials tutorials,
            COwnedVehicles vehicles,
            COwnedFrames ownedFrames,
            CUserProgress progress,
            CDecadePass decadePass,
            CDispatches dispatches,
            CLiveEvents liveEvents,
            CSideCities sideCities,
            CDebugInfo debugInfo,
            CContracts contracts,
            CWarehouse warehouse,
            CFactories factories,
            IEventBus eventBus,
            CAccount account,
            COffers offers,
            CTasks tasks,
            CProjects projects,
            CCity city
            )
        {
            AnimatedCurrencies = animatedCurrencies;
            RechargerValidator = rechargerValidator;
            GlobalVariables = globalVariables;
            OwnedValuables = ownedValuables;
            FuelStation = fuelStation;
            VehicleDepo = vehicleDepo;
            Dispatchers = dispatchers;
            OwnedFrames = ownedFrames;
            DecadePass = decadePass;
            Dispatches = dispatches;
            LiveEvents = liveEvents;
            SideCities = sideCities;
            Tutorials = tutorials;
            Factories = factories;
            Warehouse = warehouse;
            Contracts = contracts;
            DebugInfo = debugInfo;
            _eventBus = eventBus;
            Progress = progress;
            Vehicles = vehicles;
            Account = account;
            Offers = offers;
            Tasks = tasks;
            Projects = projects;
            City = city;

            _allComponents.Add(Progress);
            _allComponents.Add(OwnedValuables);
            _allComponents.Add(Vehicles);
            _allComponents.Add(Offers);
            _allComponents.Add(Account);
            _allComponents.Add(Warehouse);
            _allComponents.Add(Contracts);
            _allComponents.Add(SideCities);
            _allComponents.Add(AnimatedCurrencies);
            _allComponents.Add(Dispatches);
            _allComponents.Add(City);
            _allComponents.Add(DecadePass);
            _allComponents.Add(GlobalVariables);
            _allComponents.Add(Factories);
            _allComponents.Add(FuelStation);
            _allComponents.Add(Tutorials);
            _allComponents.Add(VehicleDepo);
            _allComponents.Add(RechargerValidator);
            _allComponents.Add(LiveEvents);
            _allComponents.Add(Dispatchers);
            _allComponents.Add(OwnedFrames);
            _allComponents.Add(Tasks);
            _allComponents.Add(Projects);

            InitComponents();
            InitialSync(dtoProvider.Dto);
        }

        public void Initialize()
        {
            _eventBus.Subscribe<CServerHitsProcessedSignal>(OnHitsProcessed);
        }

        private void OnHitsProcessed(CServerHitsProcessedSignal signal)
        {
            _dataSync.SyncHits(signal.Hits, this);
        }

        public void Tick()
        {
            Dispatches.Tick();
        }

        private void InitComponents()
        {
            foreach (CBaseUserComponent component in _allComponents)
            {
                component.Initialize(this);
            }
        }
        private void InitialSync(CUserDto dto)
        {
            IsValid = dto != null;

            if(!IsValid)
                return;

            Progress.InitialSync(dto.Progress);
            SideCities.InitialSync(dto.SideCities);
            OwnedValuables.InitialSync(dto.OwnedValuables);
            Vehicles.InitialSync(dto.Vehicles);
            Offers.InitialSync(dto.Offers);
            Warehouse.InitialSync(dto.Warehouse);
            Contracts.InitialSync(dto.Contracts);
            LiveEvents.InitialSync(dto.LiveEvents);
            Account.InitialSync(dto.Account);
            DecadePass.InitialSync(dto.DecadePass);
            Dispatches.InitialSync(dto.Dispatches);
            GlobalVariables.InitialSync(dto.GlobalVariables);
            City.InitialSync(dto.Cities);
            Factories.InitialSync(dto.Factories);
            FuelStation.InitialSync(dto.FuelStation);
            VehicleDepo.InitialSync(dto.VehicleDepo);
            Tutorials.InitialSync(dto.Tutorials);
            DebugInfo.InitialSync(dto.DebugInfo);
            Dispatchers.InitialSync(dto.Dispatchers);
            OwnedFrames.InitialSync(dto.OwnedFrames);
            Tasks.InitialSync(dto.Tasks);
            Projects.InitialSync(dto.Projects);
        }

        public void OnContextDestroy(bool appExits)
        {
            Dispose();
        }

        private void Dispose()
        {
            foreach (CBaseUserComponent component in _allComponents)
            {
                component.Dispose();
            }
        }

        public IEnumerable<ELiveEvent> GetRequiredEvents()
        {
            foreach (ELiveEvent liveEvent in Vehicles.GetRequiredEvents())
            {
                yield return liveEvent;
            }

            foreach (ELiveEvent liveEvent in City.GetRequiredEvents())
            {
                yield return liveEvent;
            }
        }
    }
}
