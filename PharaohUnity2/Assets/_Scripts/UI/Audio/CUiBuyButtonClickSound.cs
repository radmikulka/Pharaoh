// =========================================
// AUTHOR: Juraj Joscak
// DATE:   29.08.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiBuyButtonClickSound : MonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField] private CAudioClip _audioClip;

		private IAudioManager _audioManager;
		
		private CUiButton _button;
		private CUiValuablePrice _price;

		[Inject]
		private void Inject(IAudioManager audioManager)
		{
			_audioManager = audioManager;
		}

		public void Construct()
		{
			_button = GetComponent<CUiButton>();
			_price = GetComponentInChildren<CUiValuablePrice>(true);
		}

		public void Initialize()
		{
			_button.AddClickListener(OnButtonClick);
		}

		private void OnButtonClick()
		{
			if(!_price.CanAfford())
				return;
			
			_audioManager.PlaySound2D(_audioClip);
		}
	}
}