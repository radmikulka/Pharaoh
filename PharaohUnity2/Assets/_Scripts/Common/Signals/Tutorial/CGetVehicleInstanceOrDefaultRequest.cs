// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.09.2025
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CGetVehicleInstanceOrDefaultRequest
	{
		public readonly EVehicle VehicleId;

		public CGetVehicleInstanceOrDefaultRequest(EVehicle vehicleId)
		{
			VehicleId = vehicleId;
		}
	}
	
	public class CGetVehicleInstanceOrDefaultResponse
	{
		public readonly Transform Transform;

		public CGetVehicleInstanceOrDefaultResponse(Transform transform)
		{
			Transform = transform;
		}
	}
}