// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.11.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public class CTutorialAnalyticsFunnel : CBaseAnalyticsFunnel
	{
		private readonly string _tutorialName;
		
		public CTutorialAnalyticsFunnel(string tutorialName, IAnalytics analytics) : base("TutorialStepComplete", tutorialName, analytics)
		{
			_tutorialName = tutorialName;
		}

		protected override void ModifyEventParams(Dictionary<string, object> eventParams)
		{
			base.ModifyEventParams(eventParams);
			
			eventParams.Add("Tutorial", _tutorialName);
		}
	}
}