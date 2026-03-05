// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.11.2025
// =========================================

using AldaEngine.AldaFramework;
using KBCore.Refs;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TycoonBuilder
{
	public class CUiScrollRect : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, SearchableEnum] private EUiScrollRect _scrollRectId;
		[SerializeField, Self] private ScrollRect _scrollRect;

		private CUiScrollRectsProvider _scrollRectsProvider;
        
		public ScrollRect ScrollRect => _scrollRect;
		public EUiScrollRect ScrollRectId => _scrollRectId;
        
		[Inject]
		private void Inject(CUiScrollRectsProvider scrollRectsProvider)
		{
			_scrollRectsProvider = scrollRectsProvider;
		}
        
		public void Initialize()
		{
			_scrollRectsProvider.Register(this);
		}
	}
}