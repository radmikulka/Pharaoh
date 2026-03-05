// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.1.2024
// =========================================

namespace TycoonBuilder
{
	public class CUserDataBool : CData<bool>
	{
		public CUserDataBool(bool defaultVal) : base(defaultVal)
		{
		}

		public override bool IsConsistent()
		{
			return LocalValue == ServerValue;
		}
		
		public static implicit operator bool(CUserDataBool d) => d.LocalValue;
	}
}