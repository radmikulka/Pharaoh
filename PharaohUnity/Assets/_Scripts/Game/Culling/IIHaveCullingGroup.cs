// =========================================
// AUTHOR: Radek Mikulka
// DATE:   17.12.2025
// =========================================

using UnityEngine;

namespace Pharaoh
{
	public interface IIHaveCullingGroup
	{
		Vector3 Position { get; }
		float Radius { get; }
		bool UpdatePosition { get; }

		void OnStateChange(bool isVisible);
	}
}