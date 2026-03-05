// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.10.2025
// =========================================

using System;
using ServerData;

namespace TycoonBuilder
{
	public class COpenTermsOfUseScreenTask
	{
		public readonly string PrivacyPolicyLink;
		public readonly string TermsOfUseLink;
		public readonly Action OnConfirmed;

		public COpenTermsOfUseScreenTask(string privacyPolicyLink, string termsOfUseLink, Action onConfirmed)
		{
			PrivacyPolicyLink = privacyPolicyLink;
			TermsOfUseLink = termsOfUseLink;
			OnConfirmed = onConfirmed;
		}
	}
}