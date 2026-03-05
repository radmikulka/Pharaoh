// =========================================
// AUTHOR: Marek Karaba
// DATE:   15.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CSectionUiItem : CUiPoolItem<CSectionUiData>, IInitializable
	{
		[SerializeField] private CUiComponentText _titleText;

		private ITranslation _translation;
		
		private CSectionButton _button;
		
		[Inject]
		private void Inject(ITranslation translation)
		{
			_translation = translation;
		}
		
		public void Initialize()
		{
			_translation.OnLanguageChanged.Subscribe(OnLanguageChanged);
		}
		
		protected override void OnGetFromPool()
		{
			_titleText.SetValue(_translation.GetText(PoolData.Title));
			_button = GetComponent<CSectionButton>();
			
			_button.SetContent(PoolData.SectionContentBase);
			_button.SetTabHandler(PoolData.SectionHandler);
			_button.SetIcon(PoolData.Icon);
		}
		
		private void OnLanguageChanged(ITranslation _)
		{
			if(PoolData == null)
				return;
			
			_titleText.SetValue(_translation.GetText(PoolData.Title));
		}
	}
}