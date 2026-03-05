// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

namespace TycoonBuilder
{
	public class CUserDataInt : CData<int>
	{
		public CUserDataInt(int defaultVal) : base(defaultVal)
		{
		}

		public override bool IsConsistent()
		{
			return LocalValue == ServerValue;
		}
		
		public static implicit operator int(CUserDataInt d) => d.LocalValue;
	}
}