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
		[SerializeField] private CUiComponentText _text;
		
		public void UpdateProgress(float progress)
		{
			_slider.SetValue(progress);

			int progressValue = CMath.CeilToInt(progress * 100f);
			_text.SetValue($"{progressValue}%");
		}

		public void SetActive(bool active)
		{
			gameObject.SetActive(active);
		}
	}
}