// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.12.2024
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;

namespace Pharaoh
{
	public class COwnedValuables : CBaseUserComponent, IInitializable
	{
		private readonly Dictionary<EValuable, COwnedValuable> _valuables = new();
		private readonly COwnedValuableFactory _valuableFactory = new();

		private readonly IServerTime _serverTime;
		private readonly IEventBus _eventBus;
		private readonly IMapper _mapper;

		public COwnedValuables(
			IServerTime serverTime,
			IEventBus eventBus,
			IMapper mapper
		)
		{
			_serverTime = serverTime;
			_eventBus = eventBus;
			_mapper = mapper;
		}

		public void Initialize()
		{
			
		}

		public void InitialSync(COwnedValuablesDto dto)
		{
			COwnedValuableData[] valuables = _mapper.FromJson<COwnedValuableData>(dto.Valuables);
			foreach(COwnedValuableData data in valuables)
			{
				COwnedValuable valuable = GetOrCrateValuable(data.Id);
				valuable.InitialSync(data);
			}
		}

		public T GetValuable<T>(EValuable id) where T : COwnedValuable
		{
			COwnedValuable ownedValuable = GetOrCrateValuable(id);
			return (T)ownedValuable;
		}

		public CConsumableOwnedValuable GetConsumable(EValuable id)
		{
			return GetValuable<CConsumableOwnedValuable>(id);
		}

		public void ModifyValuableInternal(IValuable valuable, CValueModifyParams modifyParams = null)
		{
			COwnedValuable ownedValuable = GetOrCrateValuable(valuable.Id);
			ownedValuable.Modify(valuable, modifyParams);
			_eventBus.Send(new COwnedValuableChangedSignal(valuable));
		}

		public bool HaveValuable(IValuable valuable)
		{
			COwnedValuable ownedValuable = GetOrCrateValuable(valuable.Id);
			return ownedValuable.HaveValuable(valuable);
		}

		private COwnedValuable GetOrCrateValuable(EValuable valuableId)
		{
			if (_valuables.TryGetValue(valuableId, out var valuable)) 
				return valuable;
			
			valuable = _valuableFactory.Create(valuableId);
			_valuables.Add(valuableId, valuable);
			return valuable;
		}

		public override void Dispose()
		{
			base.Dispose();

			foreach (var ownedValuable in _valuables)
			{
				ownedValuable.Value.Dispose();
			}
		}
	}
}