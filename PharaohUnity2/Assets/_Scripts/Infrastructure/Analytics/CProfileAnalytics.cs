// =========================================
// AUTHOR: Juraj Joscak
// DATE:   03.12.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;

namespace TycoonBuilder
{
	public class CProfileAnalytics : IAldaFrameworkComponent
	{
		private readonly IAnalytics _analytics;
		
		private readonly Dictionary<string, object> _cachedParams = new();
		
		public CProfileAnalytics(IAnalytics analytics)
		{
			_analytics = analytics;
		}

		public void UXProfileFrameChange(EProfileFrame frame)
		{
			//Odešle se když uživatel změní profile frame za podmínek v obecném principu
			//Name - Název framu
			//Index - Pořadové číslo framu z enumu
			_cachedParams.Clear();
			
			_cachedParams.Add("Name", frame.ToString());
			_cachedParams.Add("Index", (int)frame);
			
			_analytics.SendData("UXProfileFrameChange", _cachedParams);
		}

		public void UXProfileAvatarChange(EProfileAvatar avatar)
		{
			//Odešle se když uživatel změní profile avatar za podmínek v obecném principu
			//Name - Název avataru
			//Index - Pořadové číslo avataru z enumu
			_cachedParams.Clear();
			
			_cachedParams.Add("Name", avatar.ToString());
			_cachedParams.Add("Index", (int)avatar);
			
			_analytics.SendData("UXProfileAvatarChange", _cachedParams);
		}

		public void UXProfileNameChange(string name)
		{
			//Odešle se když uživatel změní profile name za podmínek v obecném principu
			//Name - S jakým jménem uživatel nakonec skončil
			
			_cachedParams.Clear();
			
			_cachedParams.Add("Name", name);
			
			_analytics.SendData("UXProfileNameChange", _cachedParams);
		}
		
		public void UXSettingChange(string settingName, string originalValue, string finalValue)
		{
			//Odešle se když uživatel změní nějaké nastavení v profilu za podmínek v obecném principu
			
			_cachedParams.Clear();
			
			_cachedParams.Add("Name", settingName);
			_cachedParams.Add("OriginalValue", originalValue);
			_cachedParams.Add("FinalValue", finalValue);
			
			_analytics.SendData("UXSettingChange", _cachedParams);
		}
	}
}