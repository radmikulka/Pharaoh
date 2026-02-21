// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.10.2025
// =========================================

using AldaEngine;
using UnityEngine;

namespace Pharaoh
{
	public class CLoadingProgressBar : MonoBehaviour
	{
		[SerializeField] private CUiComponentSlider _slider;
		
		public void UpdateProgress(float progress)
		{
			_slider.SetValue(progress);
		}

		public void SetActive(bool active)
		{
			gameObject.SetActive(active);
		}
	}
}