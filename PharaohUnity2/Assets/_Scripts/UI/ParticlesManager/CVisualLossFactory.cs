// =========================================
// NAME: Marek Karaba
// DATE: 02.10.2025
// =========================================

using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CVisualLossFactory : MonoBehaviour, IConstructable
	{
		[SerializeField] private CVisualLoss _template;

		private CAldaInstantiator _aldaInstantiator;
		private DiContainer _diContainer;

		[Inject]
		private void Inject(DiContainer diContainer, CAldaInstantiator aldaInstantiator)
		{
			_aldaInstantiator = aldaInstantiator;
			_diContainer = diContainer;
		}

		public void Construct()
		{
			_template.gameObject.SetActive(false);
		}

		public CVisualLoss Create(Transform parent)
		{
			CVisualLoss visualLoss = _aldaInstantiator.Instantiate(_template, parent, _diContainer);
			visualLoss.gameObject.SetActive(true);
			return visualLoss;
		}
	}
}