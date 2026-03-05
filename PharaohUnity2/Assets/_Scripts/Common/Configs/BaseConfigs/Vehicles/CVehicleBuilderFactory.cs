// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.11.2025
// =========================================

using ServerData;
using Zenject;

namespace TycoonBuilder
{
	public class CVehicleBuilderFactory : IVehicleBuilderFactory
	{
		private readonly DiContainer _container;

		public CVehicleBuilderFactory(DiContainer container)
		{
			_container = container;
		}

		public CVehicleBuilder GetNewVehicleBuilder()
		{
			return _container.Resolve<CVehicleBuilder>();
		}
	}
}