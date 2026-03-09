using AldaEngine;
using UnityEngine;

namespace Pharaoh
{
	[CreateAssetMenu(menuName = "____Pharaoh/Configs/TranslationConfig")]
	public class CTranslationConfig : ScriptableObject
	{
		[SerializeField] [BundleId] private int _bundleId;
		[SerializeField] private string _sourceFilesPath = "Assets/_Sources/Localizations";
		[SerializeField] private string _runtimeFilesPath = "Assets/_Sources/Localizations/Generated";
		[SerializeField] private ELanguageCode _defaultLanguage = ELanguageCode.En;
		[SerializeField] private ELanguageCode[] _supportedLanguages = { ELanguageCode.En };

		public int BundleId => _bundleId;
		public string SourceFilesPath => _sourceFilesPath;
		public string RuntimeFilesPath => _runtimeFilesPath;
		public ELanguageCode DefaultLanguage => _defaultLanguage;
		public ELanguageCode[] SupportedLanguages => _supportedLanguages;

		public CBundleLink GetBundleLink(string fileName)
		{
#if UNITY_EDITOR
			TextAsset asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"{_runtimeFilesPath}/{fileName}.txt");
			return new CBundleLink(_bundleId, asset);
#else
			return new CBundleLink(_bundleId, $"{_runtimeFilesPath}/{fileName}.txt");
#endif
		}
	}
}
