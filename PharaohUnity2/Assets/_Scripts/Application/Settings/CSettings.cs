using AldaEngine;
using Zenject;

namespace Pharaoh
{
	public class CSettings : ISettings
	{
		private const string QualityKey = "pharaoh_quality";
		private const string SoundKey = "pharaoh_sound";
		private const string MusicKey = "pharaoh_music";
		private const string VibrationsKey = "pharaoh_vibrations";
		private const string LanguageKey = "pharaoh_language";

		private IEventBus _eventBus;
		private ITranslation _translation;

		public EGraphicsQuality Quality { get; private set; }
		public ELanguageCode Language { get; private set; }
		public bool Vibrations { get; private set; }
		public bool Sound { get; private set; }
		public bool Music { get; private set; }

		public CSettings()
		{
			Quality = (EGraphicsQuality)CPlayerPrefs.Get(QualityKey, (int)EGraphicsQuality.Medium);
			Sound = CPlayerPrefs.Get(SoundKey, 1) == 1;
			Music = CPlayerPrefs.Get(MusicKey, 1) == 1;
			Vibrations = CPlayerPrefs.Get(VibrationsKey, 1) == 1;
			Language = (ELanguageCode)CPlayerPrefs.Get(LanguageKey, 0);
		}

		[Inject]
		private void Inject(IEventBus eventBus, ITranslation translation)
		{
			_eventBus = eventBus;
			_translation = translation;
		}

		public void SetQuality(EGraphicsQuality quality)
		{
			Quality = quality;
			CPlayerPrefs.Set(QualityKey, (int)quality);
			_eventBus.Send(new CGraphicsQualityChangedSignal(quality));
		}

		public void SetSound(bool enabled)
		{
			Sound = enabled;
			CPlayerPrefs.Set(SoundKey, enabled ? 1 : 0);
		}

		public void SetMusic(bool enabled)
		{
			Music = enabled;
			CPlayerPrefs.Set(MusicKey, enabled ? 1 : 0);
		}

		public void SetVibrations(bool enabled)
		{
			Vibrations = enabled;
			CPlayerPrefs.Set(VibrationsKey, enabled ? 1 : 0);
		}

		public void SetLanguage(ELanguageCode language)
		{
			Language = language;
			CPlayerPrefs.Set(LanguageKey, (int)language);
			_translation.SetLanguage(language);
		}
	}
}
