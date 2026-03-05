// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System.Collections.Generic;
using System;
using AldaEngine.Tcp;
using Newtonsoft.Json;
using ServerData;

namespace ServerData.Hits
{
	public abstract class CBaseHit : IHit
	{
		[JsonProperty] public EHit HitType { get; set; }

		[JsonIgnore] public int HitId => (int) HitType;

		protected CBaseHit(EHit hitType)
		{
			HitType = hitType;
		}
	}
}