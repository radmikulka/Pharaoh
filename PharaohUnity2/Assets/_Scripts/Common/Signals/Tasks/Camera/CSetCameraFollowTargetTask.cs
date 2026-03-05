// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.09.2025
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public class CSetCameraFollowTargetTask
	{
		public readonly Transform Target;
		
		public CSetCameraFollowTargetTask(Transform target)
		{
			Target = target;
		}
	}
}