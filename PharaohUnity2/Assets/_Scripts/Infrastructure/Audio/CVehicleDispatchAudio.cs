// =========================================
// AUTHOR: Juraj Joscak
// DATE:   29.08.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Design;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CVehicleDispatchAudio : MonoBehaviour, IInitializable
	{
		[SerializeField] private CAudioClip _defaultClips;
		[SerializeField] private CSerializableDictionary<EMovementType, CAudioClip> _audioClips;

		private IAudioManager _audioManager;
		private IEventBus _eventBus;
		private CDesignVehicleConfigs _vehicleConfigs;
		
		[Inject]
		private void Inject(IAudioManager audioManager, IEventBus eventBus, CDesignVehicleConfigs vehicleConfigs)
		{
			_audioManager = audioManager;
			_eventBus = eventBus;
			_vehicleConfigs = vehicleConfigs;
		}
		
		public void Initialize()
		{
			_eventBus.Subscribe<CVehicleDispatchedSignal>(OnVehicleDispatched);
		}
		
		private void OnVehicleDispatched(CVehicleDispatchedSignal signal)
		{
			CVehicleConfig cfg = _vehicleConfigs.GetConfig(signal.Dispatch.VehicleId);
			if (_audioClips.TryGetValue(cfg.MovementType, out CAudioClip clip))
			{
				_audioManager.PlaySound2D(clip);
			}
			else
			{
				_audioManager.PlaySound2D(_defaultClips);
			}
		}
	}
}