// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.02.2026
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

namespace TycoonBuilder.Ui
{
	public class CUiScrollRectPositionSetter : ValidatedMonoBehaviour
	{
		[SerializeField, Self] private CUiScrollRectLayoutRepainter _scrollRectLayoutRepainter;
		[SerializeField, Self] private ScrollRect _scrollRect;
		[SerializeField] private bool _scrollVertically;
		[SerializeField] private bool _scrollHorizontally;
		
		public async UniTask ScrollToStart(CancellationToken ct)
		{
			await UniTask.WaitForEndOfFrame(ct);
			
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
		
		public async UniTask ScrollToEnd(CancellationToken ct)
		{
			await UniTask.WaitForEndOfFrame(ct);
			
			if (_scrollHorizontally)
			{
				_scrollRect.content.anchoredPosition = new Vector2(-_scrollRect.content.rect.width + _scrollRect.viewport.rect.width, _scrollRect.content.anchoredPosition.y);
			}
			if (_scrollVertically)
			{
				_scrollRect.content.anchoredPosition = new Vector2(_scrollRect.content.anchoredPosition.x, -_scrollRect.content.rect.height + _scrollRect.viewport.rect.height);
			}
			_scrollRectLayoutRepainter.Refresh();
		}
	}
}