// =========================================
// AUTHOR: Juraj Joscak
// DATE:   25.07.2025
// =========================================

using AldaEngine;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CTabButtonSpriteSwapper : MonoBehaviour, ITabButtonVisualSwapper
	{
		[SerializeField] private CUiSpriteSwapper _spriteSwapper;

		public void Select()
		{
			_spriteSwapper.SetSprite(1);
		}

		public void Deselect()
		{
			_spriteSwapper.SetSprite(0);
		}
	}
}