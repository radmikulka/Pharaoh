// =========================================
// AUTHOR: Juraj Joscak
// DATE:   17.02.2026
// =========================================

using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiResourceWithMaxCapacity : MonoBehaviour, IAldaFrameworkComponent, ITopBarItemWithMaxCapacity
	{
		[SerializeField] private EResource _id;
		
		private CUser _user;
		private IServerTime _serverTime;
		
		[Inject]
		private void Inject(CUser user, IServerTime serverTime)
		{
			_user = user;
			_serverTime = serverTime;
		}
		
		public int GetMaxCapacity()
		{
			switch (_id)
			{
				case EResource.Passenger:
					long currentTime = _serverTime.GetTimestampInMs();
					int maxCapacity = _user.City.GetPassengersGenerator(currentTime).MaxCapacity(currentTime);
					return maxCapacity;
				default:
					throw new System.NotImplementedException("Max capacity not implemented for " + _id);
			}
		}
	}
}