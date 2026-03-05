// =========================================
// AUTHOR: Juraj Joscak
// DATE:   16.10.2025
// =========================================

using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public interface IParticleTargetPart
	{
		public void DisableSync();
		public void EnableSync();
		public void ParticleStepFinished(IValuable diff);
	}
	
	public class CParticleTarget : ValidatedMonoBehaviour, IInitializable, IConstructable
	{
		[SerializeField] private EValuable _particleId;
		[SerializeField] private EResource[] _resourceIdsToInclude;
		[SerializeField] private EResource[] _resourceIdsToExclude;

		private ICtsProvider _ctsProvider;
		private IEventBus _eventBus;
		
		private int _lockers;
		private IParticleTargetPart[] _parts;
		
		[Inject]
		private void Inject(IEventBus eventBus, ICtsProvider  ctsProvider)
		{
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
		}
		
		public void Construct()
		{
			_parts = GetComponentsInChildren<IParticleTargetPart>(true);
		}
		
		public void Initialize()
		{
			_eventBus.Subscribe<CCurrencyParticleStartedSignal>(OnCurrencyParticleStarted);
			_eventBus.Subscribe<CCurrencyParticleFinishedSignal>(OnCurrencyParticleFinished);
			_eventBus.Subscribe<CCurrencyParticleStepFinishedSignal>(OnCurrencyParticleStepFinished);
		}
		
		private void OnCurrencyParticleStarted(CCurrencyParticleStartedSignal signal)
		{
			if (signal.CurrencyType != _particleId)
				return;
			
			if(signal.CurrencyType == EValuable.Resource && !IsResourceValid(signal.Resource))
				return;

			if (_lockers == 0)
			{
				foreach (IParticleTargetPart part in _parts)
				{
					part.DisableSync();
				}
			}
			_lockers++;
		}
		
		private void OnCurrencyParticleFinished(CCurrencyParticleFinishedSignal signal)
		{
			if (signal.CurrencyType != _particleId)
				return;
			
			if(signal.CurrencyType == EValuable.Resource && !IsResourceValid(signal.Resource))
				return;

			CurrencyParticleFinishedAsync(_ctsProvider.Token).Forget();
		}
		
		private void OnCurrencyParticleStepFinished(CCurrencyParticleStepFinishedSignal signal)
		{
			if (signal.Diff.Id != _particleId)
				return;

			if (signal.Diff.Id == EValuable.Resource)
			{
				CResourceValuable resourceDiff = (CResourceValuable)signal.Diff;
				if (!IsResourceValid(resourceDiff.Resource.Id))
					return;
			}
			
			foreach (IParticleTargetPart part in _parts)
			{
				part.ParticleStepFinished(signal.Diff);
			}
		}

		private bool IsResourceValid(EResource resource)
		{
			if(_resourceIdsToInclude.Length > 0)
				return _resourceIdsToInclude.Contains(resource);
			
			if(_resourceIdsToExclude.Length > 0)
				return !_resourceIdsToExclude.Contains(resource);
			
			throw new System.Exception("At least one of resource arrays must be filled");
		}
		
		private async UniTask CurrencyParticleFinishedAsync(CancellationToken ct)
		{
			await UniTask.WaitForSeconds(1f, cancellationToken: ct);
			
			_lockers--;
			if (_lockers <= 0)
			{
				_lockers = 0;
				foreach (IParticleTargetPart part in _parts)
				{
					part.EnableSync();
				}
			}
		}
	}
}