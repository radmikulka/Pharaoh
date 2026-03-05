// =========================================
// NAME: Marek Karaba
// DATE: 07.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CVehicleNameProvider : IVehicleNameProvider
	{
		private readonly ITranslation _translation;

		public CVehicleNameProvider(ITranslation translation)
		{
			_translation = translation;
		}

		public string GetVehicleName(EVehicle vehicleId)
		{
			string text = _translation.GetText("Vehicle." + (int)vehicleId + ".Name");
			return text;
		}
	}
}