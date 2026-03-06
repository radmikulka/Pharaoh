// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.09.2023
// =========================================

namespace Pharaoh
{
	public class CCommunicationTokenProvider
	{
		public string CommunicationToken { get; private set; }

		public bool IsCommunicationInitialized => !string.IsNullOrEmpty(CommunicationToken);

		public void SetCommunicationToken(string communicationToken)
		{
			CommunicationToken = communicationToken;
		}
	}
}