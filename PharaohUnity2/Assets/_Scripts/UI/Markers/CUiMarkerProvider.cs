// =========================================
// AUTHOR: Juraj Joscak
// DATE:   04.07.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public enum EMarkerType
	{
		None = 0,
		Number,
		ExclamationMark,
		CheckMark,
		New,
	}

	public enum EMarkerColor
	{
		None = 0,
		Red,
		Green,
		Blue,
		Yellow,
	}
	
	public class CUiMarkerProvider : MonoBehaviour, IAldaFrameworkComponent, IIHaveBundleLinks
	{
		[SerializeField] private SMarkerSprite[] _markerSprites;
		[SerializeField] private CUiMarker _markerTemplate;

		private IBundleManager _bundleManager;
		private CAldaInstantiator _instantiator;
		private DiContainer _diContainer;
		
		private readonly List<CUiMarker> _freeMarkers = new();
		private readonly Dictionary<RectTransform, CUiMarker> _activeMarkers = new();

		[Inject]
		private void Inject(IBundleManager bundleManager, CAldaInstantiator instantiator, DiContainer diContainer)
		{
			_bundleManager = bundleManager;
			_instantiator = instantiator;
			_diContainer = diContainer;
		}

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			foreach (SMarkerSprite sprite in _markerSprites)
			{
				yield return sprite.Sprite;
			}
		}

		public void SetMarker(int value, RectTransform target, EMarkerType markerType = EMarkerType.Number, EMarkerColor color = EMarkerColor.Red, bool pulse = true, bool isMaskable = true)
		{
			bool activeFound = _activeMarkers.TryGetValue(target, out CUiMarker marker);
			if(value == 0 && activeFound)
			{
				marker.Disable();
				_freeMarkers.Add(marker);
				_activeMarkers.Remove(target);
			}
			else
			{
				if(!activeFound)
				{
					marker = GetFreeMarker();
					_activeMarkers.Add(target, marker);
					marker.Set(target, pulse, isMaskable);
				}

				marker.UpdateValue(value, markerType, GetColorSprite(color));
			}
		}

		public void DisableMarker(RectTransform target)
		{
			bool activeFound = _activeMarkers.TryGetValue(target, out CUiMarker marker);
			if (!activeFound)
				return;
			
			marker.Disable();
			_freeMarkers.Add(marker);
			_activeMarkers.Remove(target);
		}
		
		private CUiMarker GetFreeMarker()
		{
			if(_freeMarkers.Count > 0)
			{
				CUiMarker marker = _freeMarkers[0];
				_freeMarkers.Remove(marker);
				return marker;
			}

			CUiMarker newMarker = _instantiator.Instantiate(_markerTemplate, transform, _diContainer);
			return newMarker;
		}

		private Sprite GetColorSprite(EMarkerColor color)
		{
			for (int i = 0; i < _markerSprites.Length; i++)
			{
				if(_markerSprites[i].Color == color)
					return _bundleManager.LoadItem<Sprite>(_markerSprites[i].Sprite, EBundleCacheType.Persistent);
			}

			return null;
		}

		[Serializable]
		private class SMarkerSprite
		{
			[SerializeField] private EMarkerColor _color;
			[SerializeField][BundleLink(true, typeof(Sprite))] private CBundleLink _sprite;
			
			public EMarkerColor Color => _color;
			public IBundleLink Sprite => _sprite;
		}
	}
}