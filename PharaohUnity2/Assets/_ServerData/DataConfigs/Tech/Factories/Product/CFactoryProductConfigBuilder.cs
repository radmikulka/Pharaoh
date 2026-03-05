// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CFactoryProductConfigBuilder
	{
		private readonly List<SResource> _requirements = new(2);

		private readonly int _productionTimeSeconds;
		private readonly SResource _resource;

		public CFactoryProductConfigBuilder(int productionTimeSeconds, EResource resourceId, int amount)
		{
			_productionTimeSeconds = productionTimeSeconds;
			_resource = new SResource(resourceId, amount);
		}
		
		public CFactoryProductConfigBuilder AddRequirement(EResource resourceId, int amount)
		{
			_requirements.Add(new SResource(resourceId, amount));
			return this;
		}
		
		public CFactoryProductConfig Build()
		{
			return new CFactoryProductConfig(_requirements.ToArray(), _productionTimeSeconds, _resource);
		}
	}
}