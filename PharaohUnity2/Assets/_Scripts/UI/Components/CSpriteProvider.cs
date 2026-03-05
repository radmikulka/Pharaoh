// =========================================
// AUTHOR: Marek Karaba
// DATE:   07.11.2025
// =========================================

using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CSpriteProvider : MonoBehaviour
	{
		[SerializeField] private Sprite[] _sprites;
		public Sprite GetSprite(int index)
		{
			if (index < 0 || index >= _sprites.Length)
			{
				Debug.LogError($"Sprite index {index} is out of bounds.");
				return null;
			}
			return _sprites[index];
		}
	}
}