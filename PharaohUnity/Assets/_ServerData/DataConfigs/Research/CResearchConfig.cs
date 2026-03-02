// =========================================
// DATE:   02.03.2026
// =========================================

namespace ServerData
{
	public class CResearchConfig
	{
		public EResearchId Id { get; }
		public string DisplayName { get; }
		public EResearchId[] Prerequisites { get; }
		public SResource[] Cost { get; }
		public IResearchEffect[] Effects { get; }

		public CResearchConfig(
			EResearchId id,
			string displayName,
			EResearchId[] prerequisites,
			SResource[] cost,
			IResearchEffect[] effects)
		{
			Id = id;
			DisplayName = displayName;
			Prerequisites = prerequisites ?? System.Array.Empty<EResearchId>();
			Cost = cost ?? System.Array.Empty<SResource>();
			Effects = effects ?? System.Array.Empty<IResearchEffect>();
		}
	}
}
