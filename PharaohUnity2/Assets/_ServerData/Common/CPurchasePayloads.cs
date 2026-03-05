// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.01.2026
// =========================================

using System;
using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class CPurchasePayloads
	{
		[JsonProperty] public IPurchasePayload[] Payloads { get; set; }
		
		public static readonly CPurchasePayloads Empty = new();

		public CPurchasePayloads()
		{
			Payloads = Array.Empty<IPurchasePayload>();
		}

		public CPurchasePayloads(params IPurchasePayload[] payloads)
		{
			Payloads = payloads;
		}

		public T GetComponentOrDefault<T>() where T : IPurchasePayload
		{
			foreach (var payload in Payloads)
			{
				if (payload is T typedPayload)
				{
					return typedPayload;
				}
			}

			return default;
		}
	}

	public interface IPurchasePayload : IMapAble
	{
		
	}

	public class CEventPassPayload : IPurchasePayload
	{
		[JsonProperty] public bool IsExtraPremium { get; set; }
		[JsonProperty] public ELiveEvent LiveEventId { get; set; }

		public CEventPassPayload()
		{
		}

		public CEventPassPayload(bool isExtraPremium, ELiveEvent liveEventId)
		{
			IsExtraPremium = isExtraPremium;
			LiveEventId = liveEventId;
		}
	}

	public class CDecadePassPayload : IPurchasePayload
	{
		[JsonProperty] public bool IsExtraPremium { get; set; }

		public CDecadePassPayload()
		{
		}

		public CDecadePassPayload(bool isExtraPremium)
		{
			IsExtraPremium = isExtraPremium;
		}
	}
}