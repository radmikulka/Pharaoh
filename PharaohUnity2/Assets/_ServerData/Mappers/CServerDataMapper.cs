// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using AldaEngine;
using ServerData.Dto;

namespace ServerData
{
    [NonLazy]
    public class CServerDataMapper
    {
        private readonly IMapper _mapper;

        public CServerDataMapper(IMapper mapper)
        {
            _mapper = mapper;

            AddMappings();
            AddParams();
        }

        private void AddMappings()
        {
            _mapper.AddJsonMapConstructor(() => new CValuableDto());
        }

        private void AddParams()
        {
            _mapper.AddMap<COfferParamDto, IOfferParam>(o 
                => COfferParam.New(o.Id, o.StringValue));
            
            _mapper.AddMap<IOfferParam, COfferParamDto>(o =>
            {
                COfferParam param = (COfferParam)o;
                return new COfferParamDto(o.Id, param.StringValue);
            });
        }
    }
}