// =========================================
// AUTHOR: Marek Karaba
// DATE:   13.08.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CUiScrollToStartSectionPart : ValidatedMonoBehaviour, ISectionContentPart
	{
		[SerializeField, Self] private CUiScrollRectLayoutRepainter _scrollRectLayoutRepainter;
		[SerializeField, Self] private ScrollRect _scrollRect;
		[SerializeField] private bool _scrollVertically;
		[SerializeField] private bool _scrollHorizontally;

		public void OnShow()
		{
			ScrollToStart();
		}

		public void OnHide()
		{
			
		}

		public void ScrollToStart()
		{
			if (_scrollHorizontally)
			{
				_scrollRect.content.anchoredPosition = new Vector2(0f, _scrollRect.content.anchoredPosition.y);
			}
			if (_scrollVertically)
			{
				_scrollRect.content.anchoredPosition = new Vector2(_scrollRect.content.anchoredPosition.x, 0f);
			}
			_scrollRectLayoutRepainter.Refresh();
		}
	}
}