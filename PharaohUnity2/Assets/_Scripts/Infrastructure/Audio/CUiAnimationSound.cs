// =========================================
// AUTHOR: Marek Karaba
// DATE:   27.08.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Infrastructure
{
	public class CUiAnimationSound : MonoBehaviour, IAldaFrameworkComponent
	{
		private IAudioManager _audioManager;
		
		private readonly List<SRunningSound> _runningSounds = new ();
		
		[Inject]
		private void Inject(IAudioManager audioManager)
		{
			_audioManager = audioManager;
		}

		public void anim_PlayAudio(CAudioClip audioClip)
		{
			SRunningSound runningSound = _audioManager.PlaySound2D(audioClip);
			_runningSounds.Add(runningSound);
		}

		public void anim_StopAudio(CAudioClip audioClip)
		{
			foreach (SRunningSound runningSound in _runningSounds.
				         Where(runningSound => audioClip.ContainsClip(runningSound.AudioSource.clip)))
			{
				_audioManager.Stop(runningSound.Guid);
			}
		}
		
		public void _anim_PlayLoopAudio(CAudioClip audioClip)
		{
			SRunningSound runningSound = _audioManager.PlaySound2D(audioClip, true);
			_runningSounds.Add(runningSound);
		}
	}
}