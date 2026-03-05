// =========================================
// AUTHOR: Juraj Joscak
// DATE:   30.10.2025
// =========================================

using AldaEngine;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CButtonTransitionTintOutside : CButtonTransitionTint
	{
		[SerializeField] private CUiButton _targetButton;
		
		public override void Construct()
		{
			base.Construct();
			Button = _targetButton;
		}
	}
}