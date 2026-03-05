// =========================================
// AUTHOR: Marek Karaba
// DATE:   21.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using DG.Tweening;
using TycoonBuilder.Configs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CPopUpNumber : MonoBehaviour, IInitializable
	{
		[SerializeField] private CUiComponentText _text;
		
		public bool IsAvailable { get; private set; }

		public void Initialize()
		{
			IsAvailable = true;
		}

		public void RunFixedPosition(int amount, RectTransform source)
		{
			Run(amount, source, true);
		}

		private void Run(int amount, RectTransform source, bool fixedPosition)
		{
			RunText(amount, source, fixedPosition);
		}

		private void RunText(int value, RectTransform source, bool fixedPosition)
		{
			IsAvailable = false;
			transform.position = source.position + new Vector3(Random.value*80, Random.value*80);
			
			if (fixedPosition)
			{
				transform.position = source.position;
			}
			
			Reset();

			_text.SetValue(value, new CPlusSignNumberFormatter());
			RunCurrencyPopupSequence();
		}

		private void RunCurrencyPopupSequence()
		{
			DOTween.Sequence()
				.Append(transform.DOScale(1, 2f))
				.Insert(0f,_text.DOColor(Color.white, 0.5f))
				.Insert(1.3f,_text.DOColor(Color.clear, 0.7f))
				.Append(_text.DOColor(Color.clear, 0f))
				.AppendCallback(() => IsAvailable = true);
		}

		private void Reset()
		{
			transform.localScale = Vector3.zero;
			_text.SetColor(Color.clear, true);
		}
	}
}