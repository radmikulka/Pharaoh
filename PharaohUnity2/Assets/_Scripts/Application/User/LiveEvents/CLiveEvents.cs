// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.10.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using UnityEngine;

namespace TycoonBuilder
{
	public class CLiveEvents : CBaseUserComponent, ITickable
	{
		private readonly Dictionary<ELiveEvent, ILiveEvent> _events = new();
		private readonly HashSet<ELiveEvent> _runningEventIds = new();
		private readonly HashSet<ELiveEvent> _runningLeaderboards = new();

		private readonly IRewardQueue _rewardQueue;
		private readonly CHitBuilder _hitBuilder;
		private readonly IServerTime _serverTime;
		private readonly IEventBus _eventBus;
		private readonly IMapper _mapper;
		
		private ELiveEvent[] _futureEventIds;
		
		public IReadOnlyList<ELiveEvent> FutureEventIds => _futureEventIds;
		private long _currentTimestamp;

		public CLiveEvents(
			IRewardQueue rewardQueue,
			IServerTime serverTime,
			CHitBuilder hitBuilder,
			IEventBus eventBus,
			IMapper mapper)
		{
			_rewardQueue = rewardQueue;
			_serverTime = serverTime;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
			_mapper = mapper;
		}

		public void InitialSync(CLiveEventsDto dto)
		{
			_futureEventIds = dto.FutureEvents.Select(eventDto => eventDto.Id).ToArray();
			SyncEvents(dto.LiveEvents);
			RefreshRunningEvents();
			RefreshRunningLeaderboards();
		}

		public void SyncEvents(CLiveEventDto[] liveEvents)
		{
			if (liveEvents == null)
				return;

			foreach (CLiveEventDto eventDto in liveEvents)
			{
				ILiveEvent newEvent = ConvertFromDto(eventDto);
				_events[eventDto.Id] = newEvent;
			}

			RefreshRunningEvents();
			RefreshRunningLeaderboards();
			_eventBus.Send(new CLiveEventsSyncedSignal());
		}

		public ILiveEvent GetActiveEventOrDefault(ELiveEvent id)
		{
			_events.TryGetValue(id, out ILiveEvent liveEvent);
			return liveEvent;
		}
		
		public ILiveEvent GetActiveEventOrDefault()
		{
			return _events.Values.FirstOrDefault();
		}

		public ILiveEvent GetRunningEventOrDefault()
		{
			return _events.Values.FirstOrDefault(liveEvent => _runningEventIds.Contains(liveEvent.Id));
		}

		public IEnumerable<ELiveEvent> GetActiveEvents()
		{
			foreach (KeyValuePair<ELiveEvent, ILiveEvent> liveEvent in _events)
			{
				yield return liveEvent.Key;
			}
		}

		public ILiveEvent GetActiveEvent(ELiveEvent id)
		{
			return _events[id];
		}

		public CLiveEvent<T> GetActiveEvent<T>(ELiveEvent id) where T : ILiveEventContent
		{
			return (CLiveEvent<T>)_events[id];
		}

		public T GetEventContent<T>(ELiveEvent id) where T : ILiveEventContent
		{
			ILiveEvent liveEvent = _events[id];
			return (T)liveEvent.BaseContent;
		}

		public T GetEventContentOrDefault<T>(ELiveEvent id) where T : class, ILiveEventContent
		{
			ILiveEvent liveEvent = _events[id];
			return liveEvent.BaseContent as T;
		}

		private IEventWithLeaderboard GetContentWithLeaderboard(string leaderboardUid)
		{
			foreach (ILiveEvent liveEvent in _events.Values)
			{
				if (liveEvent.BaseContent is not IEventWithLeaderboard withLeaderboard) 
					continue;

				if (withLeaderboard.Leaderboard?.Uid != leaderboardUid)
					continue;
				
				return withLeaderboard;
			}

			throw new Exception($"No event with leaderboard uid {leaderboardUid} found");
		}
		
		public bool IsAnyEventRunning(long timestamp)
		{
			foreach (ILiveEvent liveEvent in _events.Values)
			{
				if (timestamp < liveEvent.EndTimeInMs && timestamp >= liveEvent.StartTimeInMs)
					return true;
			}
			return false;
		}

		public CLiveEvent<CNormalEventContent> GetNormalEvent(ELiveEvent id)
		{
			return GetActiveEvent<CNormalEventContent>(id);
		}

		private ILiveEvent ConvertFromDto(CLiveEventDto dto)
		{
			switch (dto.Content)
			{
				case CNormalEventContentDto normal:
				{
					CNormalEventContent content = new(_rewardQueue, _serverTime, _hitBuilder, dto.Id, _eventBus,
						_mapper, User);
					content.InitialSync(normal);
					return new CLiveEvent<CNormalEventContent>(content, dto.Id, dto.EndTimeInMs, dto.StartTimeInMs,
						dto.CancellationTimeInMs);
				}
				case CSmallEventContentDto small:
				{
					CSmallEventContent content = new(_rewardQueue, _serverTime, _hitBuilder, dto.Id, _eventBus,
						_mapper, User);
					content.InitialSync(small);
					return new CLiveEvent<CSmallEventContent>(content, dto.Id, dto.EndTimeInMs, dto.StartTimeInMs,
						dto.CancellationTimeInMs);
				}
			}

			throw new NotImplementedException();
		}

		private IEnumerable<IEventWithBattlePass> GetEventsWithBattlePass()
		{
			foreach (KeyValuePair<ELiveEvent, ILiveEvent> liveEvent in _events)
			{
				if (liveEvent.Value.BaseContent is IEventWithBattlePass withBattlePass)
				{
					yield return withBattlePass;
				}
			}
		}

		public CContract GetEventContractOrDefault(EStaticContractId id)
		{
			foreach (KeyValuePair<ELiveEvent, ILiveEvent> liveEvent in _events)
			{
				if (liveEvent.Value.BaseContent is not IEventWithContracts normalEventContent)
					continue;

				CContract contract = normalEventContent.GetContractOrDefault(id);
				if (contract == null)
					continue;
				return contract;
			}

			return null;
		}

		public IEnumerable<CContract> AllActiveContracts()
		{
			foreach (KeyValuePair<ELiveEvent, ILiveEvent> liveEvent in _events)
			{
				if (liveEvent.Value.BaseContent is not IEventWithContracts normalEventContent)
					continue;

				foreach (CContract contract in normalEventContent.AllActiveContracts())
				{
					yield return contract;
				}
			}
		}

		public bool IsEventContractCompleted(EStaticContractId id)
		{
			foreach (KeyValuePair<ELiveEvent, ILiveEvent> liveEvent in _events)
			{
				if (liveEvent.Value.BaseContent is not IEventWithContracts normalEventContent)
					continue;
				
				bool isCompleted = normalEventContent.IsContractCompleted(id);
				if (isCompleted)
				{
					return true;
				}
			}

			return false;
		}

		public bool RemoveContract(EStaticContractId contractId)
		{
			foreach (KeyValuePair<ELiveEvent, ILiveEvent> liveEvent in _events)
			{
				if (liveEvent.Value.BaseContent is not IEventWithContracts normalEventContent)
					continue;

				bool removed = normalEventContent.RemoveContract(contractId);
				if (removed)
				{
					return true;
				}
			}

			return false;
		}

		public void SyncContracts(CContractDto[] dto)
		{
			if (dto == null)
				return;

			foreach (CContractDto contractDto in dto)
			{
				CContract contract = _mapper.Map<CContractDto, CContract>(contractDto);
				IEventWithContracts eventContent = GetEventContent<IEventWithContracts>(contract.EventData.EventId);
				eventContent.Sync(contract);
			}
		}

		public void SyncCompetitions(CLiveEventLeaderboardDto[] dto)
		{
			if (dto == null)
				return;

			foreach (CLiveEventLeaderboardDto competitionDto in dto)
			{
				IEventWithLeaderboard eventContent = GetEventContent<IEventWithLeaderboard>(competitionDto.LiveEvent);
				eventContent.Sync(competitionDto);
			}
		}

		public ELiveEvent GetFirstActiveEventId()
		{
			return _events.Count == 0 ? ELiveEvent.None : _events.First().Key;
		}

		public void AddEventCoins(CEventCoinValuable value)
		{
			IEventWithStore content = GetEventContent<IEventWithStore>(value.LiveEvent);
			content.ModifyEventCoins(value.Amount);
			_eventBus.Send(new COwnedValuableChangedSignal(value));
		}

		public void AddEventPoints(CEventPointValuable value)
		{
			IEventWithBattlePass content = GetEventContent<IEventWithBattlePass>(value.LiveEvent);
			content.ModifyEventPoints(value.Amount);
		}

		public bool HaveEventPoint(CEventPointValuable eventPoint)
		{
			IEventWithBattlePass content = GetEventContent<IEventWithBattlePass>(eventPoint.LiveEvent);
			return content.HavePoints(eventPoint.Amount);
		}

		public bool HaveEventCoin(CEventCoinValuable eventCoin)
		{
			IEventWithStore content = GetEventContent<IEventWithStore>(eventCoin.LiveEvent);
			return content.HaveCoins(eventCoin.Amount);
		}

		public void AddCompletedContract(CContract eventContract)
		{
			IEventWithContracts content = GetEventContent<IEventWithContracts>(eventContract.EventData.EventId);
			content.AddCompletedContract(eventContract);
		}

		public bool IsIntroSeen(ELiveEvent eventId)
		{
			CLiveEvent<CNormalEventContent> liveEvent = GetNormalEvent(eventId);
			return liveEvent.Content.IntroSeen;
		}

		public void MarkLiveEventIntroAsSeen(ELiveEvent eventId)
		{
			CLiveEvent<CNormalEventContent> liveEvent = GetNormalEvent(eventId);
			liveEvent.Content.MarkLiveEventIntroAsSeen();
			_hitBuilder.GetBuilder(new CMarkEventIntroAsSeenRequest(eventId)).BuildAndSend();
		}

		public COffer GetOfferOrDefault(string guid)
		{
			foreach (IEventWithBattlePass pass in GetEventsWithBattlePass())
			{
				if (pass.PremiumEventPassOffer.Guid == guid)
					return pass.PremiumEventPassOffer;
				if (pass.ExtraPremiumEventPassOffer.Guid == guid)
					return pass.ExtraPremiumEventPassOffer;
			}

			return null;
		}

		public void SyncLeaderboardComplements(CLeaderboardComplementDto[] dto)
		{
			if (dto == null)
				return;

			foreach (CLeaderboardComplementDto complementDto in dto)
			{
				IEventWithLeaderboard eventContent = GetContentWithLeaderboard(complementDto.LeaderboardUid);
				eventContent.SyncComplement(complementDto);
			}
		}

		public IEventWithBattlePass GetEventWithPremiumOfferOrDefault(string offerGuid)
		{
			IEnumerable<IEventWithBattlePass> eventsWithBattlePass = GetEventsWithBattlePass();
			return eventsWithBattlePass.FirstOrDefault(battlePass =>
				battlePass.PremiumEventPassOffer.Guid == offerGuid ||
				battlePass.ExtraPremiumEventPassOffer.Guid == offerGuid);
		}

		public ELiveEvent GetEventId(IEventWithBattlePass eventWithBattlePass)
		{
			foreach (KeyValuePair<ELiveEvent, ILiveEvent> liveEvent in _events)
			{
				if (liveEvent.Value.BaseContent == eventWithBattlePass)
					return liveEvent.Key;
			}

			throw new Exception("Event with battle pass not found");
		}

		public bool HavePremiumPass(EBattlePassPremiumStatus status)
		{
			long serverTime = _serverTime.GetDayRefreshTimeInMs();
			foreach (IEventWithBattlePass battlePass in GetEventsWithBattlePass())
			{
				ILiveEvent liveEvent = GetActiveEvent(battlePass.EventId);
				bool isCompleted = liveEvent.IsFinished(serverTime);
				if(isCompleted)
					continue;
				
				if(battlePass.BattlePassData.PremiumStatus.HasFlag(status))
					return true;
			}
			return false;
		}

		public ILiveEvent[] GetRunningEventsOrDefault()
		{
			long timestamp = _serverTime.GetTimestampInMs();
			return _events.Values.Where(liveEvent => timestamp < liveEvent.CancellationTimeInMs && timestamp >= liveEvent.StartTimeInMs).ToArray();
		}

		public bool IsEventWithRegion(ILiveEvent liveEvent)
		{
			bool isEventWithRegion = liveEvent.BaseContent is not CSmallEventContent;
			return isEventWithRegion;
		}

		public void Tick()
		{
			_currentTimestamp = _serverTime.GetTimestampInMs();
			TryToUpdateRunningEvents();
			TryToUpdateLeaderboards();
		}

		private void TryToUpdateRunningEvents()
		{
			foreach (ILiveEvent liveEvent in _events.Values)
			{
				bool wasRunning = _runningEventIds.Contains(liveEvent.Id);
				bool isRunning = _currentTimestamp >= liveEvent.StartTimeInMs && _currentTimestamp < liveEvent.EndTimeInMs;

				switch (wasRunning)
				{
					case false when isRunning:
						_runningEventIds.Add(liveEvent.Id);
						break;
					case true when !isRunning:
						_runningEventIds.Remove(liveEvent.Id);
						_eventBus.Send(new CLiveEventFinishedSignal(liveEvent.Id));
						break;
				}
			}
		}

		private void RefreshRunningEvents()
		{
			_currentTimestamp = _serverTime.GetTimestampInMs();
			_runningEventIds.Clear();
			
			foreach (ILiveEvent liveEvent in _events.Values)
			{
				bool isRunning = _currentTimestamp >= liveEvent.StartTimeInMs && _currentTimestamp < liveEvent.EndTimeInMs;
				if (isRunning)
				{
					_runningEventIds.Add(liveEvent.Id);
				}
			}
		}

		private void TryToUpdateLeaderboards()
		{
			foreach (ILiveEvent liveEvent in _events.Values)
			{
				if (liveEvent.BaseContent is not IEventWithLeaderboard withLeaderboard)
					continue;

				bool wasRunning = _runningLeaderboards.Contains(liveEvent.Id);
				bool isRunning = _currentTimestamp < withLeaderboard.Leaderboard?.EndTime;
				
				switch (wasRunning)
				{
					case false when isRunning:
						_runningLeaderboards.Add(liveEvent.Id);
						break;
					case true when !isRunning:
						_runningLeaderboards.Remove(liveEvent.Id);
						_eventBus.Send(new CEventLeaderboardFinishedSignal(liveEvent.Id));
						break;
				}
			}
		}
		
		private void RefreshRunningLeaderboards()
		{
			_currentTimestamp = _serverTime.GetTimestampInMs();
			_runningLeaderboards.Clear();
			
			foreach (ILiveEvent liveEvent in _events.Values)
			{
				if (liveEvent.BaseContent is not IEventWithLeaderboard withLeaderboard)
					continue;
				
				bool isRunning = _currentTimestamp < withLeaderboard.Leaderboard?.EndTime;
				if (isRunning)
				{
					_runningLeaderboards.Add(liveEvent.Id);
				}
			}
		}
	}
}