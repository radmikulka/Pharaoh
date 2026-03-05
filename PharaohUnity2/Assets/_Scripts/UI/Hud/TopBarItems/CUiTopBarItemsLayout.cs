// =========================================
// AUTHOR: Juraj Joscak
// DATE:   25.09.2025
// =========================================

using System.Linq;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CUiTopBarItemsLayout : ValidatedMonoBehaviour, IConstructable
	{
		[SerializeField] private float _spacing;
		
		private CUiTopBarItemFader[] _visuals;
		
		public void Construct()
		{
			_visuals = GetComponentsInChildren<CUiTopBarItemFader>(true);
		}
		
		public void Reposition(ETopBarItem[] visibleCurrencies)
		{
			float position = 0;
			for(int i = 0; i < _visuals.Length; i++)
			{
				if(visibleCurrencies.All(item => item != _visuals[i].Id))
					continue;
				
				_visuals[i].MoveToPosition(-position);
				position += _visuals[i].RectTransform.rect.width + _spacing;
			}
		}
	}
}