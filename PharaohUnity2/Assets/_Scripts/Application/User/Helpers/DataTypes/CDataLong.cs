// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

namespace TycoonBuilder
{
	public class CUserDataLong : CData<long>
	{
		public CUserDataLong(long defaultVal) : base(defaultVal)
		{
		}

		public override bool IsConsistent()
		{
			return LocalValue == ServerValue;
		}
		
		public static implicit operator long(CUserDataLong d) => d.LocalValue;
	}
}