// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CFactoryConfigBuilder
	{
		private readonly List<CFactoryLevelConfigBuilder> _levels = new(10);
		private readonly IUnlockRequirement _unlockRequirement;
		private readonly EFactory _id;
		
		private CFactoryLevelConfigBuilder _baseLevel;
		private ELiveEvent _liveEvent;

		public CFactoryConfigBuilder(EFactory id, EYearMilestone unlockRequirement)
		{
			_id = id;
			_unlockRequirement = IUnlockRequirement.Year(unlockRequirement);
		}
		
		public CFactoryConfigBuilder(EFactory id)
		{
			_id = id;
			_unlockRequirement = IUnlockRequirement.Null();
		}
		
		public CFactoryLevelConfigBuilder SetBaseLevel()
		{
			_baseLevel = new CFactoryLevelConfigBuilder(0);
			return _baseLevel;
		}

		public CFactoryLevelConfigBuilder AddLevel(long upgradeDurationInSeconds)
		{
			CFactoryLevelConfigBuilder levelBuilder = new(upgradeDurationInSeconds);
			_levels.Add(levelBuilder);
			return levelBuilder;
		}

		public CFactoryConfigBuilder SetLiveEvent(ELiveEvent liveEvent)
		{
			_liveEvent = liveEvent;
			return this;
		}
		
		public CFactoryConfig Build()
		{
			CFactoryLevelConfig baseLevel = _baseLevel.Build();
			CFactoryLevelConfig[] levels = new CFactoryLevelConfig[_levels.Count];
			for (int i = 0; i < _levels.Count; i++)
			{
				levels[i] = _levels[i].Build();
			}
			return new CFactoryConfig(_id, _liveEvent, _unlockRequirement, baseLevel, levels);
		}
	}
}