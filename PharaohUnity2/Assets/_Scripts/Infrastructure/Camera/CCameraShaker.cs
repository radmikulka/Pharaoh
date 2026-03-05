// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.09.2025
// =========================================

using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CCameraShaker : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private CinemachineBasicMultiChannelPerlin _perlin;

		private IEventBus _eventBus;
		
		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.AddAsyncTaskHandler<CPlayCameraShakeTask>(HandlePlayCameraShakeTask);
		}

		private async Task HandlePlayCameraShakeTask(CPlayCameraShakeTask task, CancellationToken ct)
		{
			await Shake(task.Duration, task.Amplitude, task.Frequency, ct);
		}

		private async UniTask Shake(float duration, float amplitude, float frequency, CancellationToken ct)
		{
			float time = 0f;
			
			_perlin.FrequencyGain = frequency;

			while (time < duration)
			{
				time += Time.deltaTime;
				float t = CEasings.EaseOutQuad(0f, 1f, CMath.Clamp01(time / duration));
				_perlin .AmplitudeGain = amplitude * (1f - t);
				
				await UniTask.Yield(ct);
			}
		}
	}
}