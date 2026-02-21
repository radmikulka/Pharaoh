// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System.Collections.Generic;
using System;
using AldaEngine.Tcp;

namespace ServerData.Hits
{
	public abstract class CResponseHit : CBaseHit
	{
		protected CResponseHit(EHit hitType) : base(hitType)
		{
		}
	}
}