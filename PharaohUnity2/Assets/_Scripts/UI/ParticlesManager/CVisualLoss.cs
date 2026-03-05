// =========================================
// NAME: Marek Karaba
// DATE: 02.10.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using DG.Tweening;
using TycoonBuilder.Configs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CVisualLoss : MonoBehaviour, IInitializable
	{
		private const float MoveUpDistance = 150f;
		
		[SerializeField] private CUiComponentText _text;
		[SerializeField] private CUiComponentImage _iconImage;
		
		private CUiGlobalsConfig _uiGlobalsConfig;

		[Inject]
		private void Inject(CUiGlobalsConfig uiGlobalsConfig)
		{
			_uiGlobalsConfig = uiGlobalsConfig;
		}
		
		public bool IsAvailable { get; private set; }

		public void Initialize()
		{
			IsAvailable = true;
		}

		public void Run(int amount, RectTransform source, Sprite icon)
		{
			IsAvailable = false;
			transform.position = source.position;
			
			Reset();

			_text.SetValue(amount, new CMinusSignNumberFormatter());
			_iconImage.SetSprite(icon);
			RunVisualLossSequence();
		}

		private void RunVisualLossSequence()
		{
			Vector3 targetPosition = transform.position + Vector3.up * MoveUpDistance;
			
			DOTween.Sequence()
				.Append(transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack))
				.Join(transform.DOMoveY(targetPosition.y, 1.2f).SetEase(Ease.OutQuad))
				.Join(_text.DOColor(_uiGlobalsConfig.NotEnoughCurrencyColor, 0.3f))
				.Join(_iconImage.DOColor(Color.white, duration: 0.3f))
				.AppendInterval(0.4f)
				.Append(_text.DOColor(Color.clear, 0.4f))
				.Join(_iconImage.DOColor(Color.clear, duration: 0.4f))
				.Join(transform.DOScale(0.8f, 0.4f).SetEase(Ease.InQuad))

				.AppendCallback(() => IsAvailable = true);
		}

		private void Reset()
		{
			transform.localScale = Vector3.zero;
			_text.SetColor(Color.clear, true);
		}
	}
}