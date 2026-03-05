// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.01.2026
// =========================================

using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CVehicleDepotTutorialLoadAnimation : MonoBehaviour, IInitializable
	{
		[SerializeField] private CAnimation _animation;
		
		private IEventBus _eventBus;
		
		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			_eventBus.AddAsyncTaskHandler<CPlayTutorialDepotLoadAnimation>(PlayAnim);
			_animation.gameObject.SetActive(false);
		}

		private async Task PlayAnim(CPlayTutorialDepotLoadAnimation task, CancellationToken ct)
		{
			_animation.gameObject.SetActive(true);
			await _animation.Play(ct);
			DisableOnNextFrame(ct).Forget();
		}
		
		private async Task DisableOnNextFrame(CancellationToken ct)
		{
			await UniTask.Yield(ct);
			await UniTask.Yield(ct);
			_animation.gameObject.SetActive(false);
		}
	}
}