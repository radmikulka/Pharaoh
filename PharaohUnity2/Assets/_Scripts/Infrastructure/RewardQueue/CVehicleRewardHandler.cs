// =========================================
// AUTHOR: Marek Karaba
// DATE:   18.07.2025
// =========================================

using System.Threading;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TycoonBuilder.Infrastructure
{
	public class CVehicleRewardHandler : BaseRewardHandler, IAldaFrameworkComponent
	{
		private readonly IVehicleShowcase _vehicleShowcase;
		private readonly CUser _user;

		public CVehicleRewardHandler(IVehicleShowcase vehicleShowcase, CUser user)
		{
			_vehicleShowcase = vehicleShowcase;
			_user = user;
		}

		public async UniTask Claim(CVehicleValuable vehicle, CancellationToken ct, CValueModifyParams modifyParams)
		{
			await _vehicleShowcase.Show(vehicle.Vehicle, ct);
			_user.OwnedValuables.ModifyValuableInternal(vehicle, modifyParams);
		}
	}
}