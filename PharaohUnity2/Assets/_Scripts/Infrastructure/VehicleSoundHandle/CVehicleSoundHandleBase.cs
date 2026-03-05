// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.03.2026
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
	public abstract class CVehicleSoundHandleBase : MonoBehaviour, IInitializable, IDestroyable
	{
		private const float FadeDuration = 0.6f;

		[SerializeField] private CSerializableDictionary<EMovementType, CAudioClip> _movementSounds;

		private CDesignVehicleConfigs _vehicleConfigs;
		private IAudioManager _audioManager;
		private Guid _activeSoundId;

		protected IEventBus EventBus { get; private set; }

		[Inject]
		private void Inject(IEventBus eventBus, CDesignVehicleConfigs vehicleConfigs, IAudioManager audioManager)
		{
			_vehicleConfigs = vehicleConfigs;
			_audioManager = audioManager;
			EventBus = eventBus;
		}

		public void Initialize()
		{
			RegisterTaskHandlers();
		}

		protected abstract void RegisterTaskHandlers();

		protected void PlayVehicleSound(EVehicle vehicleId)
		{
			CVehicleConfig vehicleConfig = _vehicleConfigs.GetConfig(vehicleId);
			PlaySound(vehicleConfig.MovementType);
		}

		private void PlaySound(EMovementType movementType)
		{
			StopSound();

			CAudioClip clip = _movementSounds[movementType];
			SRunningSound sound = _audioManager.PlaySound2D(clip, true);
			_audioManager.FadeIn2D(sound.Guid, FadeDuration, true);
			_activeSoundId = sound.Guid;
		}

		protected void StopSound()
		{
			_audioManager.FadeOut(_activeSoundId, FadeDuration, true);
		}

		public void OnContextDestroy(bool appExits)
		{
			if(appExits)
				return;
			_audioManager.Stop(_activeSoundId);
		}
	}
}