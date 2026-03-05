// =========================================
// AUTHOR: Marek Karaba
// DATE:   06.08.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CGlobalVariable : IMapAble
	{
		public EGlobalVariable Id { get; }
		public string StringValue { get; private set; }
		public bool BoolValue => StringValue == "1";

		public CGlobalVariable(EGlobalVariable id)
		{
			Id = id;
			StringValue = string.Empty;
		}

		public void SetValue(string value)
		{
			StringValue = value;
		}

		public void SetValue(bool value)
		{
			StringValue = value ? "1" : "0";
		}
	}
}