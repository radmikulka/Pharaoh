// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.10.2023
// =========================================

namespace TycoonBuilder
{
	public class CUserDataNullableLong : CData<long?>
	{
		public CUserDataNullableLong(long? defaultVal) : base(defaultVal)
		{
		}

		public override bool IsConsistent()
		{
			return LocalValue == ServerValue;
		}
		
		public static implicit operator long?(CUserDataNullableLong d) => d.LocalValue;
	}
}