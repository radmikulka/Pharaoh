// =========================================
// AUTHOR: Marek Karaba
// DATE:   07.08.2025
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public interface IFloatingWindowData
	{
		Transform WorldPoint { get; }
		Transform OwnerTransform { get; }
	}
}