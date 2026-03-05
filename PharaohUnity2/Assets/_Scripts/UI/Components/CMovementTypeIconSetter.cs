// =========================================
// AUTHOR: Marek Karaba
// DATE:   20.08.2025
// =========================================

using AldaEngine;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CMovementTypeIconSetter : MonoBehaviour
	{
		[SerializeField] private CUiComponentImage _icon;
		[SerializeField] private CUiComponentImage _bg;
		[SerializeField] private bool _hasBg;
		[SerializeField] private Color _bgColor;

		public void Disable()
		{
			_icon.SetActive(false);
			_bg.SetActive(false);
		}

		public void SetIcon(Sprite icon)
		{
			_bg.SetActive(_hasBg);
			if (_hasBg)
			{
				_bg.SetColor(_bgColor, true);
			}
			_icon.SetActive(true);
			_icon.SetSprite(icon);
		}
	}
}