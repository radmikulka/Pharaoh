// =========================================
// AUTHOR: Marek Karaba
// DATE:   11.09.2025
// =========================================

using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiCurrencyWithMaximumCapacity : MonoBehaviour, IAldaFrameworkComponent, ITopBarItemWithMaxCapacity
	{
		[SerializeField] private EValuable _id;
		
		private CUser _user;
		
		[Inject]
		private void Inject(CUser user)
		{
			_user = user;
		}
		
		public int GetMaxCapacity()
		{
			switch (_id)
			{
				case EValuable.Fuel:
					int maxCapacity = _user.FuelStation.GetFuelCapacity();
					return maxCapacity;
				default:
					throw new System.NotImplementedException("Max capacity not implemented for " + _id);
			}
		}
	}
}