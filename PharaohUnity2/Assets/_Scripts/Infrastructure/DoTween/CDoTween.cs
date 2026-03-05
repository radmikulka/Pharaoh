// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.3.2024
// =========================================

using AldaEngine;
using DG.Tweening;

namespace TycoonBuilder.Infrastructure
{
	[NonLazy]
	public class CDoTween
	{
		public CDoTween()
		{
			DOTween.SetTweensCapacity(500, 50);
		}
	}
}