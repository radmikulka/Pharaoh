// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.10.2023
// =========================================

namespace TycoonBuilder
{
	public class CUserDataNullableInt : CData<int?>
	{
		public CUserDataNullableInt(int? defaultVal) : base(defaultVal)
		{
		}

		public override bool IsConsistent()
		{
			return LocalValue == ServerValue;
		}
		
		public static implicit operator int?(CUserDataNullableInt d) => d.LocalValue;
	}
}