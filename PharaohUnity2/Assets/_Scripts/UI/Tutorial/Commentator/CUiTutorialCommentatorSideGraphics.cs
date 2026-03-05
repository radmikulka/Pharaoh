// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.10.2025
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public class CUiTutorialCommentatorSideGraphics : MonoBehaviour
	{
		[SerializeField] private ITutorialCommentator.ESide _side;
		
		public void SetSide(ITutorialCommentator.ESide side)
		{
			gameObject.SetActive(_side == side);
		}
	}
}