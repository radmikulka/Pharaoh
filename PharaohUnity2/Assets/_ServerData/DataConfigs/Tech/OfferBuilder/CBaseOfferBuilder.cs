// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData.Design;
using ServerData.Dto;

namespace ServerData
{
	public abstract class CBaseOfferBuilder<TBuilderType, TParamsBuilderType> 
		where TBuilderType : CBaseOfferBuilder<TBuilderType, TParamsBuilderType>
		where TParamsBuilderType : CParamsBuilder<TParamsBuilderType>, new()
	{
		protected List<EOfferTag> _tags;
		protected readonly List<IValuable> _rewards = new();

		public readonly TParamsBuilderType Params = new();
		public IValuable Price { get; protected set; }
		public IReadOnlyList<IValuable> Rewards => _rewards;
		public IReadOnlyList<EOfferTag> Tags => _tags;
		public bool ShowNewOfferMarker { get; protected set; } = true;

		public bool HaveTag(EOfferTag tag)
		{
			return _tags != null && _tags.Contains(tag);
		}
		
		public TBuilderType AddReward(params IValuable[] reward)
		{
			_rewards.AddRange(reward);
			return (TBuilderType)this;
		}
		
		public TBuilderType OverrideRewards(params IValuable[] reward)
		{
			foreach (IValuable valuable in reward)
			{
				_rewards.RemoveFirst(v => v.Id == valuable.Id);
				_rewards.Add(valuable);
			}
			return (TBuilderType)this;
		}

		public TBuilderType SetPrice(IValuable price)
		{
			Price = price;
			return (TBuilderType)this;
		}
		
		public TBuilderType SetAdvertisementPrice(EAdPlacement adPlacement)
		{
			Price = CValuableFactory.Advertisement(adPlacement);
			return (TBuilderType)this;
		}
		
		public TBuilderType SetFreePrice()
		{
			Price = CFreeValuable.Instance;
			return (TBuilderType)this;
		}
		
		public TBuilderType SetFreeNoHitPrice()
		{
			Price = CFreeNoHitValuable.Instance;
			return (TBuilderType)this;
		}
		
		public TBuilderType SetNewOfferMarker(bool showNewOfferMarker)
		{
			ShowNewOfferMarker = showNewOfferMarker;
			return (TBuilderType)this;
		}
		
		public TBuilderType AddTag(params EOfferTag[] tag)
		{
			_tags ??= new List<EOfferTag>();
			_tags.AddRange(tag);
			return (TBuilderType)this;
		}
	}
}