// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.01.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CEventLeaderboardSectionUiItem : CUiPoolItem<CEventLeaderboardSectionUiData>, IAldaFrameworkComponent
	{
		[SerializeField] private CEventLeaderboardMarker _leaderboardMarker;
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
			_leaderboardMarker.SetMarkerState(PoolData.LiveEventId, false);
			
			_translation.OnLanguageChanged.Subscribe(OnLanguageChanged);
			PoolData.OnLeaderboardFinished += OnLeaderboardFinished;
		}

		public override void OnReturnToPool()
		{
			_translation.OnLanguageChanged.Unsubscribe(OnLanguageChanged);
			PoolData.OnLeaderboardFinished -= OnLeaderboardFinished;
			
			base.OnReturnToPool();
		}

		private void OnLanguageChanged(ITranslation _)
		{
			if(PoolData == null)
				return;
			
			_titleText.SetValue(_translation.GetText(PoolData.Title));
		}

		private void OnLeaderboardFinished()
		{
			_leaderboardMarker.SetMarkerState(PoolData.LiveEventId, false);
		}
	}
}