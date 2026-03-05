// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
    public class CValuableDto : IJsonMap
    {
        [JsonProperty] public string Json { get; set; }
    
        public CValuableDto()
        {
        }
    }    
}