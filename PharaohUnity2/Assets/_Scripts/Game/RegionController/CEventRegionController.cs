// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.11.2025
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CEventRegionController : CRegionController
	{
		[SerializeField] private ELiveEvent _liveEvent;
		
		public ELiveEvent LiveEvent => _liveEvent;
	}
}