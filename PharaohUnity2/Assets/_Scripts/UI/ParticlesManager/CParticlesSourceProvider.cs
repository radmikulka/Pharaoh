// =========================================
// AUTHOR: Marek Karaba
// DATE:   21.07.2025
// =========================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CParticlesSourceProvider : MonoBehaviour, IInitializable
	{
		[SerializeField] private CParticleSource[] _particleSources;
		[SerializeField] private RectTransform _defaultRect;

		private IEventBus _eventBus;
		
		private readonly List<IParticleSource> _modifiedSources = new();
		
		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			_eventBus.AddTaskHandler<CAddModifiedSourcesTask>(OnAddModifiedSources);
			_eventBus.AddTaskHandler<CClearModifiedSourcesTask>(OnClearModifiedSources);
		}

		public void AddSourceRect(CParticleSource source) 
		{
			if (_particleSources.IsEmpty())
			{
				_particleSources = new [] { source };
			}
			else
			{
				CParticleSource[] newSources = new CParticleSource[_particleSources.Length + 1];
				for (int i = 0; i < _particleSources.Length; i++)
				{
					newSources[i] = _particleSources[i];
				}
				newSources[_particleSources.Length] = source;
				_particleSources = newSources;
			}
		}

		public RectTransform GetRectTransform(EValuable valuable)
		{
			foreach (IParticleSource source in _modifiedSources)
			{
				if (source.Id == valuable)
				{
					return source.SourceRect;
				}
			}
			
			foreach (IParticleSource source in _particleSources)
			{
				if (source.Id == valuable)
				{
					return source.SourceRect;
				}
			}
			return _defaultRect;
		}

		private void OnAddModifiedSources(CAddModifiedSourcesTask task)
		{
			_modifiedSources.AddRange(task.ModifiedSources);
		}

		private void OnClearModifiedSources(CClearModifiedSourcesTask task)
		{
			_modifiedSources.Clear();
		}
	}
}