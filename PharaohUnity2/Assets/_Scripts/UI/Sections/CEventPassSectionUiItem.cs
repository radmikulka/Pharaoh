// =========================================
// AUTHOR: Marek Karaba
// DATE:   20.02.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CEventPassSectionUiItem : CUiPoolItem<CEventPassSectionUiData>, IAldaFrameworkComponent
	{
		[SerializeField] private CEventPassMarker _eventPassMarker;
		[SerializeField] private CUiComponentText _titleText;

		private ITranslation _translation;
		
		private CSectionButton _button;
		
		[Inject]
		private void Inject(ITranslation translation)
		{
			_translation = translation;
		}

		protected override void OnGetFromPool()
		{
			_titleText.SetValue(_translation.GetText(PoolData.Title));
			_button = GetComponent<CSectionButton>();
			
			_button.SetContent(PoolData.SectionContentBase);
			_button.SetTabHandler(PoolData.SectionHandler);
			_button.SetIcon(PoolData.Icon);
			_eventPassMarker.SetMarkerState(PoolData.LiveEventId, false);
			
			_translation.OnLanguageChanged.Subscribe(OnLanguageChanged);
			PoolData.OnRewardsStateChanged += OnRewardsStateChanged;
		}

		public override void OnReturnToPool()
		{
			_translation.OnLanguageChanged.Unsubscribe(OnLanguageChanged);
			PoolData.OnRewardsStateChanged -= OnRewardsStateChanged;
			
			base.OnReturnToPool();
		}

		private void OnLanguageChanged(ITranslation _)
		{
			if(PoolData == null)
				return;
			
			_titleText.SetValue(_translation.GetText(PoolData.Title));
		}

		private void OnRewardsStateChanged()
		{
			_eventPassMarker.SetMarkerState(PoolData.LiveEventId, false);
		}
	}
}