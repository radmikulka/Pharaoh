// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData.Dto;

namespace ServerData
{
	public abstract class CBaseOfferGroupBuilder<TBuilderType, TParamsBuilderType> 
		where TBuilderType : CBaseOfferGroupBuilder<TBuilderType, TParamsBuilderType>
		where TParamsBuilderType : CParamsBuilder<TParamsBuilderType>, new()
	{
		public readonly TParamsBuilderType Params = new();
		protected List<EOfferTag> _tags;
		public IReadOnlyList<EOfferTag> Tags => _tags;
		
		public string GroupId => Params.GetParamValueOrDefault<string>(EOfferParam.GroupId);

		public TBuilderType SetGroupId(string groupId)
		{
			Params.SetParam(COfferParams.GroupId(groupId));
			return (TBuilderType) this;
		}
    
		public TBuilderType AddTag(params EOfferTag[] tag)
		{
			_tags ??= new List<EOfferTag>();
			_tags.AddRange(tag);
			return (TBuilderType)this;
		}
	}
}