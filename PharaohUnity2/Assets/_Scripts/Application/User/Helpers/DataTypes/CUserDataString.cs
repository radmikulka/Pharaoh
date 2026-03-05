// =========================================
// AUTHOR: Juraj Joscak
// DATE:   11.3.2024
// =========================================

namespace TycoonBuilder
{
	public class CUserDataString : CData<string>
	{
		public CUserDataString(string defaultVal) : base(defaultVal)
		{
		}

		public override bool IsConsistent()
		{
			return LocalValue == ServerValue;
		}
		
		public static implicit operator string(CUserDataString d) => d.LocalValue;
	}
}