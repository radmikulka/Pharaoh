// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.12.2024
// =========================================

using System.Collections.Generic;
using AldaEngine;
using Cysharp.Threading.Tasks;

namespace Pharaoh
{
	public class CTempNullAnalytics : IAnalytics
	{
		public UniTask InitializeAsync()
		{
			return UniTask.CompletedTask;
		}

		public void SetActiveAnalytics(bool isActive)
		{
            
		}

		public void SetUserId(string userId)
		{
            
		}

		public void SetUserProperty(string key, string value)
		{
            
		}

		public void SendData(string eventName, Dictionary<string, object> eventData = null)
		{
            
		}
	}
}