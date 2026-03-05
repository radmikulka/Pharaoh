// =========================================
// AUTHOR: Marek Karaba
// DATE:   12.08.2025
// =========================================

using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CDepotSectionUiItem : CUiPoolItem<CDepotSectionUiData>
	{
		[SerializeField] private CYearVisualSetter _yearVisualSetter;
		[SerializeField] private GameObject _yearVisual;
		[SerializeField] private GameObject _baseVisual;
		[SerializeField] private CUiComponentText _baseTitleText;
		[SerializeField] private GameObject _lockedVisual;

		private CSectionButton _button;
		
		protected override void OnGetFromPool()
		{
			_button = GetComponent<CSectionButton>();
			
			_button.SetContent(PoolData.SectionContentBase);
			_button.SetTabHandler(PoolData.SectionHandler);
			
			if (PoolData.DecadeMilestone == EDecadeMilestone.None)
			{
				_yearVisual.SetActive(false);
				_baseVisual.SetActive(true);
				_button.SetIcon(PoolData.Icon);
				_baseTitleText.SetValue(PoolData.Title);
			}
			else
			{
				_yearVisual.SetActive(true);
				_baseVisual.SetActive(false);
				_lockedVisual.SetActive(PoolData.Locked);
				_yearVisualSetter.SetDecade(PoolData.DecadeMilestone, PoolData.Locked);
			}
		}

		public bool IsLocked()
		{
			return PoolData.Locked;
		}

		public int GetUnlockYear()
		{
			return (int)PoolData.DecadeMilestone;
		}
	}
}