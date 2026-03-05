// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.08.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CDecadePassHudButtonClaimVisual : MonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField] private RectTransform _rectTransform;
		[SerializeField] private Material _waveMaterial;
		[SerializeField] private Image _bgImage;
		
		private CUiMarkerProvider _markerProvider;

		[Inject]
		private void Inject(CUiMarkerProvider markerProvider)
		{
			_markerProvider = markerProvider;
		}
		
		public void SetActive(bool isActive)
		{
			gameObject.SetActive(isActive);

			if (isActive)
			{
				_markerProvider.SetMarker(5, _rectTransform, EMarkerType.ExclamationMark);
				_bgImage.material = _waveMaterial;
				return;
			}
			
			_markerProvider.DisableMarker(_rectTransform);
			_bgImage.material = null;
		}
	}
}