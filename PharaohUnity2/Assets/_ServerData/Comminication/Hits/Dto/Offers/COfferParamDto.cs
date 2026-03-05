// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.ComponentModel;
using System.Diagnostics;
using AldaEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServerData
{
    public class COfferParamDto : IMapAble
    {
        [JsonProperty] public EOfferParam Id { get; set; }
        [JsonProperty] public string StringValue { get; set; }
    
        public COfferParamDto()
        {
        }

        public COfferParamDto(EOfferParam id, string stringValue)
        {
            Id = id;
            StringValue = stringValue;
        }
    }
}