// =========================================
// AUTHOR: Marek Karaba
// DATE:   05.03.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CInspectVehicleButton : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private CTycoonUiButton _inspectButton;

		private IEventBus _eventBus;
		private EVehicle _vehicleId;

		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			_inspectButton.AddClickListener(OpenInspectVehicle);
		}

		public void SetVehicle(EVehicle vehicleId)
		{
			_vehicleId = vehicleId;
		}
		
		private void OpenInspectVehicle()
		{
			if (_vehicleId == EVehicle.None)
			{
				Debug.LogError("Selected vehicle id is None. We need to set it before opening inspect vehicle menu.");
				return;
			}
			
			_eventBus.ProcessTask(new COpenInspectVehicleMenuTask(_vehicleId, false));
			_eventBus.Send(new CVehicleShownSignal(EVehicleShownType.VehicleDetail, _vehicleId));
		}
	}
}