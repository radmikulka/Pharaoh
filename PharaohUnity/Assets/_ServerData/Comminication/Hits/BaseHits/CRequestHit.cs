// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using AldaEngine;
using AldaEngine.Tcp;

namespace ServerData.Hits
{
	public abstract class CRequestHit : CBaseHit
	{
		protected CRequestHit(EHit hitType) : base(hitType)
		{
			
		}
	}
}