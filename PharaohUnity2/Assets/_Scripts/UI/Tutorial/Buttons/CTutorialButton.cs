// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.10.2025
// =========================================

using DG.Tweening;
using KBCore.Refs;
using NaughtyAttributes;
using ServerData;
using TycoonBuilder.Ui;
using UnityEngine;

namespace TycoonBuilder
{
	public class CTutorialButton : ValidatedMonoBehaviour
	{
		[SerializeField, Required] private CParticleTargetPulser _particleTargetPulser;
		[SerializeField, Child] private CanvasGroup _canvasGroup;

		protected void ActivateAnimated()
		{
			SetActive(true);
			_canvasGroup.DOFade(1f, 0.2f)
				.From(0f)
				.OnComplete(() =>
				{
					_particleTargetPulser.ParticleStepFinished(CValuableFactory.Null);
				});
		}
		
		protected void SetActive(bool state)
		{
			gameObject.SetActive(state);
		}
	}
}