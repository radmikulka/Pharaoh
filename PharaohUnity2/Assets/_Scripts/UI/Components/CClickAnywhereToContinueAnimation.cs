// =========================================
// AUTHOR: Marek Karaba
// DATE:   19.01.2026
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CClickAnywhereToContinueAnimation : ValidatedMonoBehaviour
	{
		[SerializeField, Child] private CTweener _tweener;
		[SerializeField, Self] private Animation _animation;

		public async UniTask PlayAfterDelay(float delay, CancellationToken ct)
		{
			await UniTask.WaitForSeconds(delay, cancellationToken: ct);
			PlayAnimation();
		}

		public void Stop()
		{
			_animation.Play();
			_animation.Rewind();
			_animation.Sample();
			_animation.Stop();
			_tweener.Disable();
		}

		private void PlayAnimation()
		{
			_animation.Play();
			_tweener.Enable();
		}
	}
}