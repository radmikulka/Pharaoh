// =========================================
// AUTHOR: Marek Karaba
// DATE:   27.01.2026
// =========================================

using System;
using UnityEngine;

namespace TycoonBuilder
{
	public class CShowUserDetailsPopupTask
	{
		public RectTransform ItemRect { get; }
		public RectTransform ContentRect { get; }
		public Action OnClickAction { get; }

		public CShowUserDetailsPopupTask(
			RectTransform itemRect,
			RectTransform contentRect,
			Action onClickAction)
		{
			ItemRect = itemRect;
			ContentRect = contentRect;
			OnClickAction = onClickAction;
		}
	}
}