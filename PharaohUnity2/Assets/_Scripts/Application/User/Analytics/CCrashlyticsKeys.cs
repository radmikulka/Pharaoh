// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.11.2023
// =========================================

using System;
using System.Globalization;
using AldaEngine;
using ServerData;
using ServiceEngine;
using UnityEngine;

namespace TycoonBuilder
{
	public class CCrashlyticsKeys
	{
		private readonly ICrashlytics _crashlytics;

		public CCrashlyticsKeys(ICrashlytics crashlytics)
		{
			_crashlytics = crashlytics;
		}

		public void SetDeviceInfo()
		{
			_crashlytics.SetCustomKey("InstallMode", Application.installMode.ToString());
			_crashlytics.SetCustomKey("InstallerName", Application.installerName);
		}
		
		public void SetLanguage(string language)
		{
			_crashlytics.SetCustomKey("Language", language);
		}
	}
}