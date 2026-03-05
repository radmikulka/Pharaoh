// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.10.2025
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public class CUiTutorialTooltipSideGraphics : MonoBehaviour
	{
		[SerializeField] private ETutorialTooltipSide _side;
		
		public void SetSide(ETutorialTooltipSide side)
		{
			gameObject.SetActive(_side == side);
		}
	}
}