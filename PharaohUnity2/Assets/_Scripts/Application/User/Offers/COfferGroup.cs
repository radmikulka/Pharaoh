// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using ServerData;

namespace Server
{
	public class COfferGroup
	{
		private readonly EOfferTag[] _tags;
		private readonly Dictionary<EOfferParam, IOfferParam> _params = new();

		public IOfferParam[] Params => _params.Values.ToArray();
		public IReadOnlyList<EOfferTag> Tags => _tags;
		
		public string GroupId => GetParamValueOrDefault<string>(EOfferParam.GroupId);

		public COfferGroup(EOfferTag[] tags, IOfferParam[] @params)
		{
			_tags = tags;

			foreach (IOfferParam param in @params)
			{
				_params.Add(param.Id, param);
			}
		}

		public T GetParamValueOrDefault<T>(EOfferParam paramType)
		{
			if (_params.TryGetValue(paramType, out IOfferParam param) && param is COfferParam typedParam)
				return typedParam.GetValueOrDefault<T>();
			return default;
		}
		
		public EOfferType GetOfferType()
		{
			if (_params.TryGetValue(EOfferParam.OfferType, out IOfferParam param) && param is COfferParam typedParam)
				return typedParam.GetValue<EOfferType>();

			return EOfferType.SimpleOffer;
		}

		public int GetFrontendOrderPriority()
		{
			if (_params.TryGetValue(EOfferParam.FrontendOrderPriority, out IOfferParam param) && param is COfferParam typedParam)
				return typedParam.GetValue<int>();
			
			return 0;
		}
	}
}