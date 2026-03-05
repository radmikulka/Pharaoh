// =========================================
// AUTHOR: Juraj Joscak
// DATE:   24.09.2025
// =========================================

namespace TycoonBuilder.Ui
{
	public class CGetTrayIconRequest
	{
		public readonly string OfferId;
		
		public CGetTrayIconRequest(string offerId)
		{
			OfferId = offerId;
		}
	}
	
	public class CGetTrayIconResponse
	{
		public readonly CTrayIcon TrayIcon;

		public CGetTrayIconResponse(CTrayIcon trayIcon)
		{
			TrayIcon = trayIcon;
		}
	}
}