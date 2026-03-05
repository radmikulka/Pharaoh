// =========================================
// AUTHOR: Juraj Joscak
// DATE:   02.09.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CIntroPlayer : MonoBehaviour, IInitializable
	{
		[SerializeField] private Animation[] _animations;
		[SerializeField] private Camera _camera;

		private ICtsProvider _ctsProvider;
		private IEventBus _eventBus;
		
		private CancellationTokenSource _cts;
		
		[Inject]
		private void Inject(ICtsProvider ctsProvider, IEventBus eventBus)
		{
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			foreach (Animation anim in _animations)
			{
				anim.gameObject.SetActiveObject(false);
			}
			
			_cts?.Cancel();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(_ctsProvider.Token);
			PlayIntro(_cts.Token).Forget();

			InitFov();
		}

		private void InitFov()
		{
			float raito20_9 = 16f / 9f;
			float raito4_3 = 4f / 3f;
			float currentRaito = (float)Screen.width / Screen.height;
			float currentRaitoT = CMath.InverseLerp(raito20_9, raito4_3, currentRaito);
			float fov = CMath.Lerp(60f, 86.4f, currentRaitoT);
			_camera.fieldOfView = fov;
		}
		
		private async UniTask PlayIntro(CancellationToken token)
		{
			_eventBus.Send(new CIntroStartedSignal());
			_eventBus.ProcessTask(new CSetActiveMainCanvasTask(false));

			await CSkipAbleUniTask.Play(PlayAnimations, _eventBus, token, true, 2f);
			_ctsProvider.Token.ThrowIfCancellationRequested();
			
			_eventBus.ProcessTask(new CSetActiveMainCanvasTask(true));
			_eventBus.Send(new CIntroFinishedSignal());
		}

		private async UniTask PlayAnimations(CancellationToken token)
		{
			for(int i = 0; i < _animations.Length; i++)
			{
				_animations[i].gameObject.SetActiveObject(true);
				_animations[i].Play();
				
				await UniTask.WaitForSeconds(_animations[i].clip.length, cancellationToken: token);
				
				_animations[i].gameObject.SetActiveObject(false);
			}
			
			token.ThrowIfCancellationRequested();
			StopAllAnimations();
		}

		private void StopAllAnimations()
		{
			foreach (Animation anim in _animations)
			{
				anim.Stop();
				anim.gameObject.SetActiveObject(false);
			}
		}
	}
}