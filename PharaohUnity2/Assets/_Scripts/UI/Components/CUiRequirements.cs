// =========================================
// AUTHOR: Marek Karaba
// DATE:   03.09.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CUiRequirements : MonoBehaviour, IConstructable
	{
		[SerializeField] private CUiRequirement _requirementTemplate;
		
		private CAldaInstantiator _instantiator;
		private DiContainer _diContainer;
		
		private readonly List<CUiRequirement> _requirements = new();
		private RectTransform _rectTransform;
		
		[Inject]
		private void Inject(CAldaInstantiator instantiator, DiContainer container)
		{
			_instantiator = instantiator;
			_diContainer = container;
		}
		
		public void Construct()
		{
			_rectTransform = (RectTransform)transform;
			_requirementTemplate.gameObject.SetActiveObject(false);
		}

		public void SetValuables(IValuable[] valuables)
		{
			SetRequirements(valuables.Select(valuable => (IUpgradeRequirement)IUpgradeRequirement.Valuable(valuable)));
		}
		
		public void SetRequirements(IEnumerable<IUpgradeRequirement> requirements)
		{
			int i = 0;
			foreach (IUpgradeRequirement requirement in requirements)
			{
				if(_requirements.Count <= i)
				{
					SpawnNew();
				}
				
				SetRequirement(i, requirement);
				i++;
			}
			
			SetPositions(i);
			
			for(; i < _requirements.Count; i++)
			{
				_requirements[i].gameObject.SetActiveObject(false);
			}
		}
		
		private void SetRequirement(int index, IUpgradeRequirement requirement)
		{
			_requirements[index].gameObject.SetActiveObject(true);

			switch (requirement)
			{
				case CYearMilestoneRequirement year:
					_requirements[index].SetYear(year.Year, true);
					break;
				case CValuableRequirement valuable:
					_requirements[index].SetValuable(valuable.Valuable);
					break;
				default:
					throw new Exception($"Invalid requirement type: {requirement.GetType()}");
			}
		}
		
		private void SpawnNew()
		{
			CUiRequirement newRequirement = _instantiator.Instantiate(_requirementTemplate, transform, _diContainer);
			_requirements.Add(newRequirement);
		}

		private void SetPositions(int count)
		{
			float width = _rectTransform.rect.width;
			float segment = width / count;

			for (int i = 0; i < count; i++)
			{
				RectTransform rewardTransform = (RectTransform)_requirements[i].transform;
				rewardTransform.localPosition = new Vector3(segment / 2 + segment * i, rewardTransform.localPosition.y, 0);
			}
		}
	}
}