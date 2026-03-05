using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;

namespace TycoonBuilder
{
	public class CUiLockIconAnimator : ValidatedMonoBehaviour
	{
		[SerializeField] private AnimationClip[] _shakeAnimationClips;
		[SerializeField] private AnimationClip _unlockAnimationClip;
		[SerializeField, Self] private Animation _animation;
		
		private int _lastShakeIndex = -1;
		
		public RectTransform RectTransform => transform as RectTransform;
	
		public void PlayShake()
		{
			AnimationClip shakeClip = GetShakeAnimationClip();
			_animation.clip = shakeClip;
			PlayAnimation();
		}

		public async UniTask PlayUnlockAsync(CancellationToken ct)
		{
			_animation.clip = _unlockAnimationClip;
			PlayAnimation();
			await UniTask.WaitUntil(() => !_animation.isPlaying, cancellationToken: ct);
		}

		public void PlayUnlock()
		{
			_animation.clip = _unlockAnimationClip;
			PlayAnimation();
		}

		public void RewindUnlock()
		{
			_animation.Play(_unlockAnimationClip.name);
			_animation.Rewind();
			_animation.Sample();
			_animation.Stop();
		}

		private void PlayAnimation()
		{
			_animation.Stop();
			_animation.Rewind();
			_animation.Play();
		}
		
		private AnimationClip GetShakeAnimationClip()
		{
			if (_shakeAnimationClips.Length < 2)
				return _shakeAnimationClips[0];
			
			int randomIndex  = GetRandomIndex();
			while (randomIndex == _lastShakeIndex)
			{
				randomIndex = GetRandomIndex();
			}
			_lastShakeIndex = randomIndex;
			return _shakeAnimationClips[randomIndex];
		}

		private int GetRandomIndex()
		{
			return UnityEngine.Random.Range(0, _shakeAnimationClips.Length);
		}
	}
}