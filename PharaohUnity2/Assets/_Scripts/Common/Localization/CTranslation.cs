using System;
using System.Collections.Generic;
using System.IO;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CTranslation : ITranslation, IInitializable, IDisposable
	{
		private IBundleManager _bundleManager;
		private CTranslationConfig _config;
		private ISettings _settings;

		private Dictionary<string, string> _entries = new();
		private Dictionary<string, string> _fallbackEntries = new();
		private List<ELanguageCode> _supportedLanguages = new();

		public ELanguageCode SystemLanguage => MapSystemLanguage();
		public ELanguageCode CurrentLanguage { get; private set; }
		public CEvent<ITranslation> OnLanguageChanged { get; } = new("");

		[Inject]
		private void Inject(IBundleManager bundleManager, CTranslationConfig config, ISettings settings)
		{
			_bundleManager = bundleManager;
			_config = config;
			_settings = settings;
		}

		public void Initialize()
		{
			_supportedLanguages = new List<ELanguageCode>(_config.SupportedLanguages);
			_fallbackEntries = LoadLanguage(_config.DefaultLanguage);

			ELanguageCode targetLanguage = _settings.Language;
			if (targetLanguage == ELanguageCode.None)
			{
				targetLanguage = SystemLanguage;
			}

			if (!_supportedLanguages.Contains(targetLanguage))
			{
				targetLanguage = _config.DefaultLanguage;
			}

			SetLanguage(targetLanguage);
		}

		public void Dispose()
		{
		}

		public string GetText(string key)
		{
			if (_entries.TryGetValue(key, out string value))
			{
				return value;
			}

			if (_fallbackEntries.TryGetValue(key, out string fallback))
			{
				return fallback;
			}

			return key;
		}

		public string GetText(string key, params object[] args)
		{
			return string.Format(GetText(key), args);
		}

		public void SetLanguage(ELanguageCode language)
		{
			_entries = LoadLanguage(language);
			CurrentLanguage = language;
			OnLanguageChanged.Invoke(this);
		}

		public List<ELanguageCode> GetSupportedLanguages()
		{
			return _supportedLanguages;
		}

		private Dictionary<string, string> LoadLanguage(ELanguageCode language)
		{
			Dictionary<string, string> result = new();
			string content = LoadFileContent(language.ToString());
			if (string.IsNullOrEmpty(content))
			{
				return result;
			}

			string[] lines = content.Split('\n');
			foreach (string line in lines)
			{
				string trimmed = line.Trim();
				if (string.IsNullOrEmpty(trimmed))
				{
					continue;
				}

				int separatorIndex = trimmed.IndexOf(';');
				if (separatorIndex < 0)
				{
					continue;
				}

				string key = trimmed.Substring(0, separatorIndex);
				string remaining = trimmed.Substring(separatorIndex + 1);

#if UNITY_EDITOR
				int secondSeparator = remaining.IndexOf(';');
				string value = secondSeparator >= 0 ? remaining.Substring(0, secondSeparator) : remaining;
#else
				string value = remaining;
#endif

				result[key] = value;
			}

			return result;
		}

		private string LoadFileContent(string fileName)
		{
#if UNITY_EDITOR
			string filePath = Path.Combine(_config.SourceFilesPath, $"{fileName}.txt");
			if (File.Exists(filePath))
			{
				return File.ReadAllText(filePath);
			}

			string runtimePath = Path.Combine(_config.RuntimeFilesPath, $"{fileName}.txt");
			if (File.Exists(runtimePath))
			{
				return File.ReadAllText(runtimePath);
			}

			return null;
#else
			CBundleLink bundleLink = _config.GetBundleLink(fileName);
			TextAsset textAsset = _bundleManager.LoadItem<TextAsset>(bundleLink, EBundleCacheType.Persistent);
			return textAsset != null ? textAsset.text : null;
#endif
		}

		private static ELanguageCode MapSystemLanguage()
		{
			switch (Application.systemLanguage)
			{
				case UnityEngine.SystemLanguage.English:
					return ELanguageCode.En;
				case UnityEngine.SystemLanguage.Czech:
					return ELanguageCode.Cs;
				case UnityEngine.SystemLanguage.German:
					return ELanguageCode.De;
				case UnityEngine.SystemLanguage.Spanish:
					return ELanguageCode.Es;
				case UnityEngine.SystemLanguage.French:
					return ELanguageCode.Fr;
				case UnityEngine.SystemLanguage.Italian:
					return ELanguageCode.It;
				case UnityEngine.SystemLanguage.Japanese:
					return ELanguageCode.Ja;
				case UnityEngine.SystemLanguage.Korean:
					return ELanguageCode.Ko;
				case UnityEngine.SystemLanguage.Portuguese:
					return ELanguageCode.Pt;
				case UnityEngine.SystemLanguage.Russian:
					return ELanguageCode.Ru;
				case UnityEngine.SystemLanguage.Turkish:
					return ELanguageCode.Tr;
				case UnityEngine.SystemLanguage.Chinese:
				case UnityEngine.SystemLanguage.ChineseSimplified:
				case UnityEngine.SystemLanguage.ChineseTraditional:
					return ELanguageCode.Zh;
				case UnityEngine.SystemLanguage.Polish:
					return ELanguageCode.Pl;
				case UnityEngine.SystemLanguage.Dutch:
					return ELanguageCode.Nl;
				default:
					return ELanguageCode.En;
			}
		}
	}
}
