// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.2.2024
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public interface ITutorialTargetsFactory
	{
		ITutorialGraphicsTarget GetWordTarget(Transform transform);
		ITutorialGraphicsTarget GetRectTarget(RectTransform transform);
	}
}