// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.10.2025
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public interface ISmartArrow
	{
		void ShowAt(Vector3 targetWorldPos, Vector2 screenSpaceOffset);
		void SetVisible(bool state, float blendDuration);
		void DestroySelf();
	}
}