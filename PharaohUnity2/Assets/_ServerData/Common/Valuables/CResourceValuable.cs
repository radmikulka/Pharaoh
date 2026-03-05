// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

namespace ServerData
{
	public class CResourceValuable : ICountableValuable
	{
		public EValuable Id => EValuable.Resource;
		public SResource Resource { get; set; }

		public int Value => Resource.Amount;

		public CResourceValuable()
		{
		}

		public CResourceValuable(EResource id, int amount)
		{
			Resource = new SResource(id, amount);
		}

		public CResourceValuable(SResource resource)
		{
			Resource = resource;
		}

		public CResourceValuable Reverse()
		{
			return new CResourceValuable(Resource.Id, -Resource.Amount);
		}

		public CResourceValuable Double()
		{
			return new CResourceValuable(Resource.Id, Resource.Amount * 2);
		}

		public ICountableValuable Multiply(int multiplier)
		{
			return new CResourceValuable(Resource.Id, Resource.Amount * multiplier);
		}

		public string GetAnalyticsValue()
		{
			return Resource.Amount.ToString();
		}

		public override string ToString()
		{
			return $"{nameof(Id)}: {Id}, {nameof(Resource)}: {Resource}";
		}
		
		public string GetOfferRewardAnalyticsValue()
		{
			return $"re{(int)Resource.Id}:{Resource.Amount}";
		}
	}
}