// =========================================
// AUTHOR: Marek Karaba
// DATE:   21.07.2025
// =========================================

using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CPopUpNumberFactory : MonoBehaviour, IConstructable
	{
		[SerializeField] private CPopUpNumber _template;

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

		public CPopUpNumber Create(Transform parent)
		{
			CPopUpNumber popUpNumber = _aldaInstantiator.Instantiate(_template, parent, _diContainer);
			popUpNumber.gameObject.SetActive(true);
			return popUpNumber;
		}
	}
}