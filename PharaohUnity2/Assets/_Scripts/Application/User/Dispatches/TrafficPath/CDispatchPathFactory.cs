// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.01.2026
// =========================================

using System;
using AldaEngine;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder
{
	public class CDispatchPathFactory
	{
		private readonly CDesignVehicleConfigs _vehicleConfigs;
		private readonly CWorldMap _worldMap;

		public CDispatchPathFactory(CDesignVehicleConfigs vehicleConfigs, CWorldMap worldMap)
		{
			_vehicleConfigs = vehicleConfigs;
			_worldMap = worldMap;
		}
		
		private CTrafficPath GetPath(SRegionPoint target, EMovementType movementType, ERegion userRegion, long startTime, long waitAtDestinationTime = 0)
		{
			bool isInSameRegion = target.Region == userRegion;
			CSerializedStationPoint station = _worldMap.GetStationPoint(movementType, userRegion);
			CTrafficPathBuilder trafficPathBuilder = new(startTime);

			if (isInSameRegion)
			{
				STrafficLineId toLine = new(station.RegionPoint, target);
				STrafficLineId fromLine = new(target, station.RegionPoint);
				long travelToTime = _worldMap.GetTravelTimeInMs(movementType, toLine);
				long travelFromTime = _worldMap.GetTravelTimeInMs(movementType, fromLine);
				trafficPathBuilder.AddTravelPhase(toLine, travelToTime, true);
				if (waitAtDestinationTime > 0)
					trafficPathBuilder.AddWaitPhase(waitAtDestinationTime);
				trafficPathBuilder.AddTravelPhase(fromLine, travelFromTime, false);
				return trafficPathBuilder.Build();
			}

			ERegionPoint exit = CBaseRegionConfigs.GetRegionExitPoint(movementType);
			SRegionPoint userRegionExit = new(exit, userRegion);
			SRegionPoint targetRegionExit = new(exit, target.Region);
			STrafficLineId exitRegionLine = new(station.RegionPoint, userRegionExit);
			STrafficLineId enterRegionLine = new(userRegionExit, station.RegionPoint);
			STrafficLineId targetRegionEntranceLine = new(targetRegionExit, target);
			STrafficLineId targetRegionExitLine = new(target, targetRegionExit);

			long exitRegionLineTravelTime = _worldMap.GetTravelTimeInMs(movementType, exitRegionLine);
			long targetRegionEntranceLineTravelTime = _worldMap.GetTravelTimeInMs(movementType, targetRegionEntranceLine);
			long targetRegionExitLineTravelTime = _worldMap.GetTravelTimeInMs(movementType, targetRegionExitLine);
			long enterRegionLineTravelTime = _worldMap.GetTravelTimeInMs(movementType, enterRegionLine);

			trafficPathBuilder.AddTravelPhase(exitRegionLine, exitRegionLineTravelTime, true);
			
			if (waitAtDestinationTime > 0)
				trafficPathBuilder.AddWaitPhase(waitAtDestinationTime);
			
			trafficPathBuilder.AddTravelPhase(targetRegionEntranceLine, targetRegionEntranceLineTravelTime, true);
			trafficPathBuilder.AddTravelPhase(targetRegionExitLine, targetRegionExitLineTravelTime, false);
			trafficPathBuilder.AddTravelPhase(enterRegionLine, enterRegionLineTravelTime, false);

			return trafficPathBuilder.Build();
		}

		public CTrafficPath GetPath(EStaticContractId contractId, EVehicle vehicleId, ERegion startRegion, long startTime, EContractType contractType = EContractType.Story)
		{
			CVehicleConfig vehicleConfig = _vehicleConfigs.GetConfig(vehicleId);
			SRegionPoint regionPoint;
			bool contractInstanceExists = _worldMap.ContractPointExists(contractId);
			if (contractInstanceExists)
			{
				CSerializedContractPoint contractPoint = _worldMap.GetContractPoint(contractId);
				regionPoint = contractPoint.RegionPoint;
			}
			else
			{
				var exitPoint = CBaseRegionConfigs.GetRegionExitPoint(vehicleConfig.MovementType);
				regionPoint = new SRegionPoint(exitPoint, startRegion);
			}
			bool isInSameRegion = regionPoint.Region == startRegion;
			bool applyWait = contractType == EContractType.Event || !isInSameRegion;
			long waitAtDestinationTime = applyWait ? CDesignStoryContractConfigs.WaitAtDestinationTimeWhenCrossingRegionsInMs : 0;
			return GetPath(regionPoint, vehicleConfig.MovementType, startRegion, startTime, waitAtDestinationTime);
		}
		
		public CTrafficPath GetPath(EResource resource, EVehicle vehicleId, ERegion startRegion, long startTime)
		{
			CVehicleConfig vehicleConfig = _vehicleConfigs.GetConfig(vehicleId);
			SRegionPoint regionPoint;
			bool resourcePointExists = _worldMap.ResourcePointExists(resource);
			if (resourcePointExists)
			{
				regionPoint = _worldMap.GetResourcePoint(resource).RegionPoint;
			}
			else
			{
				ERegionPoint exitPoint = CBaseRegionConfigs.GetRegionExitPoint(vehicleConfig.MovementType);
				regionPoint = new SRegionPoint(exitPoint, startRegion);
			}
			return GetPath(regionPoint, vehicleConfig.MovementType, startRegion, startTime);
		}

		public CTrafficPath GetPath(ECity cityId, EVehicle vehicleId, ERegion startRegion, long startTime)
		{
			CVehicleConfig vehicleConfig = _vehicleConfigs.GetConfig(vehicleId);
			SRegionPoint regionPoint;
			bool resourcePointExists = _worldMap.CityPointExists(cityId);
			if (resourcePointExists)
			{
				regionPoint = _worldMap.GetCity(cityId).RegionPoint;
			}
			else
			{
				ERegionPoint exitPoint = CBaseRegionConfigs.GetRegionExitPoint(vehicleConfig.MovementType);
				regionPoint = new SRegionPoint(exitPoint, startRegion);
			}
			return GetPath(regionPoint, vehicleConfig.MovementType, startRegion, startTime);
		}
	}
}