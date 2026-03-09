using System;
using AldaEngine;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CSettingsScreen : CPharaohScreen
	{
		[SerializeField] private CUiButton _vibrationButton;
		[SerializeField] private CUiButton _soundButton;
		[SerializeField] private CUiButton _musicButton;
		[SerializeField] private CUiButton _qualityButton;
		[SerializeField] private CUiButton _languageButton;

		[SerializeField] private CUiComponentText _vibrationLabel;
		[SerializeField] private CUiComponentText _soundLabel;
		[SerializeField] private CUiComponentText _musicLabel;
		[SerializeField] private CUiComponentText _qualityLabel;
		[SerializeField] private CUiComponentText _languageLabel;

		private CSettings _settings;
		private IScreenManager _screenManager;

		public override int Id => (int)EScreenId.Settings;

		[Inject]
		private void Inject(CSettings settings, IScreenManager screenManager)
		{
			_settings = settings;
			_screenManager = screenManager;
		}

		public override void Initialize()
		{
			base.Initialize();

			_vibrationButton.AddClickListener(OnVibrationClick);
			_soundButton.AddClickListener(OnSoundClick);
			_musicButton.AddClickListener(OnMusicClick);
			_qualityButton.AddClickListener(OnQualityClick);
			_languageButton.AddClickListener(OnLanguageClick);
		}

		public override void OnScreenOpenStart()
		{
			base.OnScreenOpenStart();
			RefreshUi();
		}

		public override string GetScreenName()
		{
			return "Settings";
		}

		private void OnVibrationClick()
		{
			_settings.SetVibrations(!_settings.Vibrations);
			RefreshUi();
		}

		private void OnSoundClick()
		{
			_settings.SetSound(!_settings.Sound);
			RefreshUi();
		}

		private void OnMusicClick()
		{
			_settings.SetMusic(!_settings.Music);
			RefreshUi();
		}

		private void OnQualityClick()
		{
			int count = Enum.GetValues(typeof(EGraphicsQuality)).Length;
			int next = ((int)_settings.Quality + 1) % count;
			_settings.SetQuality((EGraphicsQuality)next);
			RefreshUi();
		}

		private void OnLanguageClick()
		{
			_screenManager.OpenMenu((int)EScreenId.LanguageSelect);
		}

		private void RefreshUi()
		{
			_vibrationLabel.SetValue(_settings.Vibrations ? "On" : "Off");
			_soundLabel.SetValue(_settings.Sound ? "On" : "Off");
			_musicLabel.SetValue(_settings.Music ? "On" : "Off");
			_qualityLabel.SetValue(_settings.Quality.ToString());
			_languageLabel.SetValue(_settings.Language.ToString());
		}
	}
}
