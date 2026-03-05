// =========================================
// AUTHOR: Juraj Joscak
// DATE:   12.09.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CVignette : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField] private float _fadeDuration = 0.5f;
		[SerializeField] private float _lowIntensity;
		[SerializeField] private float _highIntensity;

		private IEventBus _eventBus;
		private ICtsProvider _ctsProvider;
		private CancellationTokenSource _cts;
		
		private static readonly int Alpha = Shader.PropertyToID("_Alpha");
		private Material _material;
		
		[Inject]
		private void Inject(IEventBus eventBus, ICtsProvider ctsProvider)
		{
			_eventBus = eventBus;
			_ctsProvider = ctsProvider;
		}
		
		public void Initialize()
		{
			_eventBus.AddTaskHandler<CDeepenVignetteTask>(OnDeepenVignette);
			_eventBus.AddTaskHandler<CLightenVignetteTask>(OnLightenVignette);
			
			Renderer rend = GetComponent<Renderer>();
			_material = rend.material;
		}
		
		private void OnDeepenVignette(CDeepenVignetteTask task)
		{
			_cts?.Cancel();
			_cts = _ctsProvider.GetNewLinkedCts();
			float currentValue = _material.GetFloat(Alpha);
			Fade(currentValue, _highIntensity, _cts.Token).Forget();
		}
		
		private void OnLightenVignette(CLightenVignetteTask task)
		{
			_cts?.Cancel();
			_cts = _ctsProvider.GetNewLinkedCts();
			float currentValue = _material.GetFloat(Alpha);
			Fade(currentValue, _lowIntensity, _cts.Token).Forget();
		}

		private async UniTaskVoid Fade(float startValue, float endValue, CancellationToken ct)
		{
			float time = 0f;
			while (time < _fadeDuration)
			{
				_material.SetFloat(Alpha, Mathf.Lerp(startValue, endValue, time / _fadeDuration));
				await UniTask.Yield(ct);
				time += Time.deltaTime;
			}
		}
	}
}