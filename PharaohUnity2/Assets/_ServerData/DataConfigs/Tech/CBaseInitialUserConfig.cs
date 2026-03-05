// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.07.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CBaseInitialUserConfig
	{
		private readonly List<IValuable> _regularUserValuables = new();
		private readonly List<IValuable> _likeABossValuables = new();
	
		public IReadOnlyList<IValuable> RegularUserValuables => _regularUserValuables;
		public IReadOnlyList<IValuable> LikeABossValuables => _likeABossValuables;

		protected void AddRegularUserValuable(params IValuable[] valuable)
		{
			_regularUserValuables.AddRange(valuable);
		}

		protected void AddLikeABossValuable(params IValuable[] valuable)
		{
			_likeABossValuables.AddRange(valuable);
		}
	}
}
