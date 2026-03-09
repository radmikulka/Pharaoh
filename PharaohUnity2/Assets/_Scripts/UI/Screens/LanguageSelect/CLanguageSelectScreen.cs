using System.Collections.Generic;
using AldaEngine;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CLanguageSelectScreen : CPharaohScreen
	{
		[SerializeField] private Transform _listParent;
		[SerializeField] private CUiButton _languageButtonPrefab;

		private CSettings _settings;
		private ITranslation _translation;

		private readonly List<GameObject> _spawnedButtons = new();

		public override int Id => (int)EScreenId.LanguageSelect;

		[Inject]
		private void Inject(CSettings settings, ITranslation translation)
		{
			_settings = settings;
			_translation = translation;
		}

		public override void OnScreenOpenStart()
		{
			base.OnScreenOpenStart();
			BuildLanguageList();
		}

		public override void OnScreenCloseEnd()
		{
			base.OnScreenCloseEnd();
			DestroySpawnedButtons();
		}

		public override string GetScreenName()
		{
			return "LanguageSelect";
		}

		private void BuildLanguageList()
		{
			DestroySpawnedButtons();

			List<ELanguageCode> languages = _translation.GetSupportedLanguages();
			foreach (ELanguageCode code in languages)
			{
				CUiButton button = Instantiate(_languageButtonPrefab, _listParent);
				_spawnedButtons.Add(button.gameObject);

				CUiComponentText label = button.GetComponentInChildren<CUiComponentText>();
				if (label != null)
				{
					label.SetValue(code.ToString());
				}

				ELanguageCode capturedCode = code;
				button.AddClickListener(() => OnLanguageSelected(capturedCode));
			}
		}

		private void OnLanguageSelected(ELanguageCode code)
		{
			_settings.SetLanguage(code);
			CloseThisScreen();
		}

		private void DestroySpawnedButtons()
		{
			foreach (GameObject go in _spawnedButtons)
			{
				Destroy(go);
			}
			_spawnedButtons.Clear();
		}
	}
}
