// =========================================
// AUTHOR: Radek Mikulka
// DATE:   21.06.2024
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public interface ITutorialHighlightPoint
	{
		Vector3 Point { get; }
		Vector2 Size { get; }
	}
}