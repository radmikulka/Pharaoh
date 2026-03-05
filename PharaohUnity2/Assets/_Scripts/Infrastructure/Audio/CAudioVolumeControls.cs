// =========================================
// AUTHOR: Juraj Joscak
// DATE:   29.08.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace TycoonBuilder
{
	public class CAudioVolumeControls : MonoBehaviour, IInitializable, IDestroyable
	{
		[SerializeField] private AudioMixerGroup _musicAudioMixerGroup;
		[SerializeField] private AudioMixerGroup _uiAudioMixerGroup;
		[SerializeField] private AudioMixerGroup _ambienceAudioMixerGroup;
		[Space]
		[SerializeField] private CAudioClip _music;
		[SerializeField] private CAudioClip _ambience;
		[SerializeField] private CAudioClip _introMusic;
        
		private IAudioManager _audioManager;
		private CSettingsData _settingsData;
		private IEventBus _eventBus;

		private SRunningSound _runningAmbience;
		private SRunningSound _runningIntro;
		private SRunningSound _runningCoreMusic;

		[Inject]
		private void Inject(IAudioManager audioManager, CSettingsData settingsData, IEventBus eventBus)
		{
			_audioManager = audioManager;
			_settingsData = settingsData;
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			_settingsData.MusicEnabled.OnValueChanged += OnMusicEnabledValueChanged;
			_settingsData.SoundsEnabled.OnValueChanged += OnSoundsEnabledValueChanged;
            
			OnMusicEnabledValueChanged(_settingsData.MusicEnabled.Value);
			OnSoundsEnabledValueChanged(_settingsData.SoundsEnabled.Value);

			_eventBus.Subscribe<CGameModeStartedSignal>(OnGameModeStarted);
			_eventBus.Subscribe<CIntroStartedSignal>(OnIntroStarted);
			_eventBus.Subscribe<CIntroFinishedSignal>(OnIntroFinished);
		}

		private void OnIntroStarted(CIntroStartedSignal signal)
		{
			PlayIntroMusic();
		}

		private void OnGameModeStarted(CGameModeStartedSignal signal)
		{
			if (signal.Data.GameModeId != EGameModeId.CoreGame)
				return;
			
			PlayCoreMusic();
		}

		private void OnIntroFinished(CIntroFinishedSignal signal)
		{
			_audioManager.FadeOut(_runningIntro.Guid, 3f);
		}

		private void PlayIntroMusic()
		{
			_audioManager.Stop(_runningCoreMusic.Guid);
			_audioManager.Stop(_runningAmbience.Guid);

			bool isPlaying = IsPlaying(_runningIntro);
			if (isPlaying)
				return;
			
			_runningIntro = _audioManager.PlaySound2D(_introMusic);
		}

		private bool IsPlaying(SRunningSound runningSound)
		{
			return runningSound.AudioSource && runningSound.AudioSource.isPlaying;
		}

		private void PlayCoreMusic()
		{
			bool isPlaying = IsPlaying(_runningCoreMusic);
			if (isPlaying)
				return;
			
			_runningCoreMusic = _audioManager.PlaySound2D(_music, true);
			_runningAmbience = _audioManager.PlaySound2D(_ambience, true);
		}

		public void OnContextDestroy(bool appExits)
		{
			_settingsData.MusicEnabled.OnValueChanged -= OnMusicEnabledValueChanged;
			_settingsData.SoundsEnabled.OnValueChanged -= OnSoundsEnabledValueChanged;
		}

		private void OnMusicEnabledValueChanged(bool value)
		{
			_audioManager.SetVolume(_musicAudioMixerGroup, "musicVolume", value ? 1f : 0f);
		}
        
		private void OnSoundsEnabledValueChanged(bool value)
		{
			_audioManager.SetVolume(_uiAudioMixerGroup, "uiVolume", value ? 1f : 0f);

			if (value)
			{
				_audioManager.FadeIn2D(_runningAmbience.Guid, 2f);
			}
			else
			{
				_audioManager.FadeOut(_runningAmbience.Guid, 2f);
			}
		}
	}
}