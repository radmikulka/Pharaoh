// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.03.2026
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
    public class CMissionDataDto
    {
        [JsonProperty] public int WorkerCountLevel { get; set; }
        [JsonProperty] public int WorkerSpeedLevel { get; set; }
        [JsonProperty] public int ProfitLevel      { get; set; }
        [JsonProperty] public int SoftCurrency     { get; set; }
    }
}
