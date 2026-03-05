// =========================================
// AUTHOR: Juraj Joscak
// DATE:   29.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;

namespace TycoonBuilder
{
	public enum EMeasurementSystem
	{
		None = 0,
		Metric,
		Imperial
	}
	
	public class CSettingsData : IInitializable
	{
		public CField<bool> MusicEnabled { get; }
		public CField<bool> SoundsEnabled { get; }
		public CField<bool> BatterySaverEnabled { get; }
		public CField<bool> NotificationsEnabled { get; }
		public CField<bool> Vibrations { get; }
		
		private CField<int> _language;
		public CField<int> Language
		{
			get
			{
				if(_language == null)
				{
					InitializeLanguage();
				}
				
				return _language;
			}
			private set => _language = value;
		}
		public CField<int> Graphics { get; }
		public CField<int> MeasurementSystem { get; }

		private readonly CBatterySaverHandler _batterySaverHandler;
		private readonly CVibrationHandler _vibrationHandler;
		private readonly CLanguageHandler _languageHandler;
		private readonly CGraphicsHandler _graphicsHandler;
		private readonly CMeasurementSystemHandler _measurementSystemHandler;
		private readonly CMusicHandler _musicHandler;
		private readonly CSoundHandler _soundHandler;
		private readonly CNotificationsHandler _notificationsHandler;

		private readonly ITranslation _translation;

		public CSettingsData(ITranslation translation, IEventBus eventBus)
		{
			_translation = translation;

			CDefaultGraphicsQualityProvider defaultGraphicsQualityProvider = new();
			MusicEnabled = new CField<bool>(new CSaveHandlerBase<bool>("Main.MusicEnabled", true));
			SoundsEnabled = new CField<bool>(new CSaveHandlerBase<bool>("Main.SoundEnabled", true));
			BatterySaverEnabled = new CField<bool>(new CSaveHandlerBase<bool>("Main.BatterySaverEnabled", false));
			Vibrations = new CField<bool>(new CSaveHandlerBase<bool>("Main.Vibrations", true));
			Graphics = new CField<int>(new CSaveHandlerBase<int>("Main.Graphics", (int)defaultGraphicsQualityProvider.GetDefaultQuality()));
			MeasurementSystem = new CField<int>(new CSaveHandlerBase<int>("Main.MeasurementSystem", (int)EMeasurementSystem.Metric));
			NotificationsEnabled = new CField<bool>(new CSaveHandlerBase<bool>("Main.NotificationsEnabled", true));
			
			_musicHandler = new CMusicHandler();
			_soundHandler = new CSoundHandler();
			_vibrationHandler = new CVibrationHandler();
			_batterySaverHandler = new CBatterySaverHandler(eventBus);
			_languageHandler = new CLanguageHandler(translation);
			_graphicsHandler = new CGraphicsHandler();
			_measurementSystemHandler = new CMeasurementSystemHandler();
			_notificationsHandler = new CNotificationsHandler(eventBus);
		}
		
		public void Initialize()
		{
			bool willDeleteUser = CDebugUserDeletionHandler.WillDeleteUserInThisSession();
			if (willDeleteUser)
			{
				MusicEnabled.ResetToDefault();
				SoundsEnabled.ResetToDefault();
				BatterySaverEnabled.ResetToDefault();
				Graphics.ResetToDefault();
				MeasurementSystem.ResetToDefault();
				Vibrations.ResetToDefault();
				NotificationsEnabled.ResetToDefault();
			}
			
			_vibrationHandler.BindTo(Vibrations);
			_musicHandler.BindTo(MusicEnabled);
			_soundHandler.BindTo(SoundsEnabled);
			_batterySaverHandler.BindTo(BatterySaverEnabled);
			_graphicsHandler.BindTo(Graphics);
			_measurementSystemHandler.BindTo(MeasurementSystem);
			_notificationsHandler.BindTo(NotificationsEnabled);
		}

		private void InitializeLanguage()
		{
			_translation.OnLanguageChanged.Subscribe(OnLanguageChanged);
			
			ELanguageCode defaultLanguage = CDebugConfig.Instance.LanguageToDebug != ELanguageCode.None
				? CDebugConfig.Instance.LanguageToDebug
				: _translation.SystemLanguage;
			
			Language = new CField<int>(new CSaveHandlerBase<int>("Main.AppLanguage", (int)defaultLanguage));
			
			bool willDeleteUser = CDebugUserDeletionHandler.WillDeleteUserInThisSession();
			if (willDeleteUser)
			{
				Language.ResetToDefault();
			}
			
			_languageHandler.BindTo(Language);
		}

		private void OnLanguageChanged(ITranslation translation)
		{
			ELanguageCode defaultLanguage = CDebugConfig.Instance.LanguageToDebug != ELanguageCode.None
				? CDebugConfig.Instance.LanguageToDebug
				: translation.SystemLanguage;
			
			Language.LateSetDefault((int)defaultLanguage);
		}
	}
}