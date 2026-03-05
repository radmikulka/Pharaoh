// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;

namespace TycoonBuilder
{
	public class CDispatchers : CBaseUserComponent, ITickable
	{
		private readonly Dictionary<EDispatcher, CDispatcher> _dispatchers = new();
		private readonly IServerTime _serverTime;
		private readonly IAnalytics _analytics;

		private int _lastKnownActiveDispatchersCount;

		public CDispatchers(IAnalytics analytics, IServerTime serverTime)
		{
			_serverTime = serverTime;
			_analytics = analytics;
		}

		public void InitialSync(CDispatchersDto dto)
		{
			foreach (CDispatcherDto dispatcher in dto.Dispatchers)
			{
				_dispatchers.Add(dispatcher.Id, new CDispatcher(dispatcher.Id, dispatcher.ExpirationTime));
			}
			
			UpdateActiveDispatchersCountAnalytics();
		}

		public void Tick()
		{
			UpdateActiveDispatchersCountAnalytics();
		}

		private void UpdateActiveDispatchersCountAnalytics()
		{
			long time = _serverTime.GetTimestampInMs();
			int activeDispatchersCount = GetActiveDispatchersCount(time);
			if (activeDispatchersCount == _lastKnownActiveDispatchersCount) 
				return;
			_lastKnownActiveDispatchersCount = activeDispatchersCount;
			_analytics.SetUserProperty("DispatchersOwned", activeDispatchersCount);
		}
		
		public void AddDispatcher(EDispatcher dispatcherId, long? expirationTime)
		{
			if (!_dispatchers.TryGetValue(dispatcherId, out var dispatcher))
			{
				dispatcher = new CDispatcher(dispatcherId, expirationTime);
				_dispatchers.Add(dispatcherId, dispatcher);
				return;
			}
			dispatcher.SetExpirationTime(expirationTime);
			UpdateActiveDispatchersCountAnalytics();
		}

		public bool? HaveDispatcher(EDispatcher dispatcher, long time)
		{
			if(_dispatchers.TryGetValue(dispatcher, out CDispatcher existingDispatcher))
			{
				return !existingDispatcher.IsExpired(time);
			}

			return false;
		}
		
		public bool IsAnyDispatcherAvailable(long time)
		{
			int freeDispatchersCount = GetFreeDispatchersCount(time);
			return freeDispatchersCount > 0;
		}
		
		public int GetActiveDispatchersCount(long time)
		{
			int count = 0;
			foreach (KeyValuePair<EDispatcher, CDispatcher> dispatcher in _dispatchers)
			{
				bool isExpired = dispatcher.Value.IsExpired(time);
				if(isExpired)
					continue;
				++count;
			}
			return count;
		}

		public int GetUsedDispatchersCount()
		{
			int activeDispatchesCount = User.Dispatches.GetActiveDispatchesCount();
			return activeDispatchesCount;
		}

		private int GetFreeDispatchersCount(long time)
		{
			int activeDispatchesCount = User.Dispatches.GetActiveDispatchesCount();
			int activeDispatchersCount = GetActiveDispatchersCount(time);
			return activeDispatchersCount - activeDispatchesCount;
		}

		public CDispatcher GetDispatcher(EDispatcher dispatcher)
		{
			return _dispatchers[dispatcher];
		}

		public bool IsDispatcherActive(EDispatcher dispatcher, long timestampInMs)
		{
			CDispatcher existingDispatcher = _dispatchers.GetValueOrDefault(dispatcher);
			if (existingDispatcher == null)
				return false;
			
			bool isExpired = existingDispatcher.IsExpired(timestampInMs);
			return !isExpired;
		}

		public bool ExpirableDispatcherExists(long timestampInMs)
		{
			foreach (CDispatcher dispatcher in _dispatchers.Values)
			{
				if (!dispatcher.ExpirationTime.HasValue)
					continue;
				
				bool isExpired = dispatcher.IsExpired(timestampInMs);
				if (!isExpired)
					return true;
			}
			return false;
		}

		public int GetPermanentDispatchersCount()
		{
			int count = _dispatchers.Values.Count(dispatcher => !dispatcher.ExpirationTime.HasValue);
			return count;
		}
	}
}