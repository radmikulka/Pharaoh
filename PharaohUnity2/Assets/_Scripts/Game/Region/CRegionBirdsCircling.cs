// =================================
// AUTHOR:		Wojciech Drzymała
// DATE			08.12.2025
// =================================

using AldaEngine;
using AldaEngine.AldaFramework;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;
using KBCore.Refs;
using Zenject;
using DG.Tweening;
using NaughtyAttributes;

namespace TycoonBuilder
{
	public class CRegionBirdsCircling : ValidatedMonoBehaviour, IInitializable
	{
		[Header("Settings")]
		[SerializeField, Range(1, 16)] private int _birdsAmount = 16;
		[SerializeField, MinMaxSlider(0.1f, 2f)] private Vector2 _birdSize;
		[SerializeField, MinMaxSlider(-360f, 360f)] private Vector2 _rotationStartingRange;
		[SerializeField, MinMaxSlider(3f, 60f)] private Vector2 _centerOffsetRange;
		[SerializeField, MinMaxSlider(6f, 40f)] private Vector2 _heightOffsetRange;
		[SerializeField, MinMaxSlider(8f, 40f)] private Vector2 _durationRange;
		
		[Header("References")]
		[SerializeField] private Transform[] _rootRotation;
		[SerializeField] private Transform[] _rootOffset;
		
		private ICtsProvider _ctsProvider;
		private CancellationTokenSource  _cts;
		
		[Inject]
		public void Inject(ICtsProvider ctsProvider)
		{
			_ctsProvider = ctsProvider;
		}

		public void Initialize()
		{
			if (_rootRotation.Length != _rootOffset.Length)
			{
				Debug.LogError("Rotation and Offset Arrays should be of equal length!");
				return;
			}
			
			StartAnimation();
		}

		[Button("Reset")]
		private void StartAnimation()
		{
			_cts?.Cancel();
			_cts = _ctsProvider.GetNewLinkedCts();
			
			for (int i = 0; i < _rootRotation.Length; i++)
			{
				if (i > _birdsAmount)
				{
					_rootRotation[i].gameObject.SetActive(false);
					continue;
				}
				
				float randomHeight = Random.Range(_heightOffsetRange.x, _heightOffsetRange.y);
				float centerOffset = Random.Range(_centerOffsetRange.x, _centerOffsetRange.y);
				_rootOffset[i].localPosition = new Vector3(-centerOffset, randomHeight, 0);
				
				float randomScale = Random.Range(_birdSize.x, _birdSize.y);
				_rootOffset[i].localScale = Vector3.one * randomScale;
				
				float randomY = Random.Range(_rotationStartingRange.x, _rotationStartingRange.y);
				Vector3 randomRotation = new Vector3(0, randomY, 0);
				_rootRotation[i].localRotation = Quaternion.Euler(randomRotation);
				
				float percentage = (centerOffset - _centerOffsetRange.x) / (_centerOffsetRange.y - _centerOffsetRange.x);
				float duration = CMath.Lerp(_durationRange.x, _durationRange.y, percentage);
				
				_rootRotation[i].gameObject.SetActive(true);
				_rootRotation[i].DOLocalRotate(new Vector3(0, randomY - 360f, 0), duration, RotateMode.FastBeyond360)
					.SetLoops(-1, LoopType.Restart)
					.SetEase(Ease.Linear)
					.WithCancellation(_cts.Token);
			}
		}

		[Button("Stop")]
		private void StopAnimation()
		{
			_cts?.Cancel();
		}
	}
}