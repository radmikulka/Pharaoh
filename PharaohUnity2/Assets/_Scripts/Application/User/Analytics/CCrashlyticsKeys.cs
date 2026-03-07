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

namespace Pharaoh
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
			_crashlytics.SetCustomKey("SystemLanguage", Application.systemLanguage.ToString());
			_crashlytics.SetCustomKey("InstallMode", Application.installMode.ToString());
			_crashlytics.SetCustomKey("InstallerName", Application.installerName);
		}
	}
}