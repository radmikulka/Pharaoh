// =========================================
// AUTHOR: Juraj Joscak
// DATE:   08.09.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CTopBarItemsUpperHolder : ValidatedMonoBehaviour, IConstructable
	{
		[SerializeField, Self] private CUiComponentCanvasGroup _canvasGroup;
		public Transform Transform { get; private set; }
		public CUiComponentCanvasGroup CanvasGroup => _canvasGroup;
		
		public void Construct()
		{
			Transform = transform;
		}
	}
}