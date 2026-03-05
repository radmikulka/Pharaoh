// =========================================
// AUTHOR: Marek Karaba
// DATE:   19.01.2026
// =========================================

using AldaEngine.AldaFramework;
using AldaEngine.Ui;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TycoonBuilder.Ui
{
	public class CUiImageAlphaHitThresholdSetter : ValidatedMonoBehaviour, IConstructable
	{
		[SerializeField, Self] private Image _image;
		[SerializeField] private float _minAlphaToHit = 0.1f;
		
		public void Construct()
		{
			_image.alphaHitTestMinimumThreshold = _minAlphaToHit;
		}
	}
}