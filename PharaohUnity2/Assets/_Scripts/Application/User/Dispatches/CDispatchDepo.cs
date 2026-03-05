// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.08.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder
{
	public class CDispatchDepo
	{
		private readonly CDesignVehicleConfigs _vehicleConfigs;
		
		private readonly IServerTime _serverTime;

		public CDispatchDepo(IServerTime serverTime, CDesignVehicleConfigs vehicleConfigs)
		{
			_vehicleConfigs = vehicleConfigs;
			_serverTime = serverTime;
		}

		public long GetAndProcessNextDispatchTimeInMs(EMovementType movementType, CUser user)
		{
			long lastDispatchTime = GetLatestDispatchTime(movementType, user.Dispatches.Dispatches);
			long currentTime = _serverTime.GetTimestampInMs();
			long minDelayInMs = CDesignVehicleConfigs.MinTimeBetweenDispatchesInSec * CTimeConst.Second.InMilliseconds;
			long result = CMath.Max(
				lastDispatchTime + minDelayInMs,
				currentTime
			);
			
			return result;
		}
		
		private long GetLatestDispatchTime(EMovementType movementType, IReadOnlyList<CDispatch> dispatches)
		{
			return dispatches
				.Where(d =>
				{
					CVehicleConfig vehicleConfig = _vehicleConfigs.GetConfig(d.VehicleId);
					return vehicleConfig.MovementType == movementType;
				})
				.Select(d => d.DispatchStartTime)
				.DefaultIfEmpty(0)
				.Max();
		}
	}
}