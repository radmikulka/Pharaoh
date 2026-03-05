// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.10.2024
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using ServerData.Design;
using ServerData.Hits;

namespace TycoonBuilder
{
	public class CUserProgress : CBaseUserComponent
	{
		private readonly CDesignProgressConfig _progressConfig;
		private readonly CDesignRegionConfigs _regionConfigs;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		
		public ERegion Region { get; private set; }
		public EYearMilestone Year { get; private set; }
		public EYearMilestone SeenYear { get; private set; }
		public int XpInCurrentYear { get; private set; }

		public CUserProgress(
			CDesignProgressConfig progressConfig, 
			CDesignRegionConfigs regionConfigs, 
			CHitBuilder hitBuilder, 
			IEventBus eventBus
			)
		{
			_progressConfig = progressConfig;
			_regionConfigs = regionConfigs;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
		}

		public void InitialSync(CProgressDto dto)
		{
			XpInCurrentYear = dto.XpInCurrentYear;
			SeenYear = dto.SeenYear;
			Region = dto.Region;
			Year = dto.Year;
		}

		public void AddXp(int amount)
		{
			EYearMilestone maxRegionYear = _regionConfigs.GetMaxRegionYear(Region);
			if (Year >= maxRegionYear)
				return;
			
			int neededXp = _progressConfig.GetXpInYear(Year);
			int newXp = XpInCurrentYear + amount;
			if (Year >= CDesignProgressConfig.MaxYear)
			{
				int maxXp = CMath.Min(newXp, neededXp - 1);
				XpInCurrentYear = maxXp;
				_eventBus.Send(new CXpIncreasedSignal(XpInCurrentYear, Year));
				return;
			}

			if (newXp >= neededXp)
			{
				bool isDecadeMilestone = IsDecadeMilestoneYear(Year + 1);
				if (isDecadeMilestone)
				{
					XpInCurrentYear = neededXp;
					return;
				}

				Year = _progressConfig.GetNextYear(Year);
				XpInCurrentYear = 0;
				if (Year <= CDesignProgressConfig.MaxYear)
				{
					newXp -= neededXp;
					AddXp(newXp);
				}

				_eventBus.Send(new CYearIncreasedSignal(Year));
				return;
			}

			int finalXp = CMath.Min(newXp, neededXp);
			XpInCurrentYear = finalXp;
			_eventBus.Send(new CXpIncreasedSignal(XpInCurrentYear, Year));
		}
		
		private bool IsDecadeMilestoneYear(EYearMilestone year)
		{
			return (int)year % 10 == 0;
		}

		public void MarkYearAsSeen()
		{
			SeenYear = Year;
			_eventBus.Send(new CYearSeenSignal(SeenYear));
			_hitBuilder.GetBuilder(new CClaimYearRequest())
				.SetOnSuccess<CClaimYearResponse>(OnSuccess)
				.SetExecuteImmediately()
				.BuildAndSend()
				;

			void OnSuccess(CClaimYearResponse response)
			{
				User.LiveEvents.SyncEvents(response.LiveEventsDto);
			}
		}

		public void IncreaseRegion()
		{
			CRegionConfig nextRegion = _regionConfigs.GetNextRegion(Region);
			Region = nextRegion.Id;
			Year = _regionConfigs.GetYearFromRegion(nextRegion.Id);
			SeenYear = Year;
			XpInCurrentYear = 0;
			_eventBus.Send(new CRegionIncreasedSignal());
		}
	}
}