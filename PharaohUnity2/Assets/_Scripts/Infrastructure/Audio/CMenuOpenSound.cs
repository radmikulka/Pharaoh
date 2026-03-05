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
	public class CMenuOpenSound : MonoBehaviour, IScreenOpenStart, IScreenCloseStart, IAldaFrameworkComponent
	{
		[SerializeField] private CAudioClip _audioClip;

		private IAudioManager _audioManager;
		
		[Inject]
		private void Inject(IAudioManager audioManager)
		{
			_audioManager = audioManager;
		}
		
		public void OnScreenOpenStart()
		{
			_audioManager.PlaySound2D(_audioClip);
		}

		public void OnScreenCloseStart()
		{
			_audioManager.PlaySound2D(_audioClip);
		}
	}
}