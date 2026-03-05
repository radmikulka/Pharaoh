// =========================================
// AUTHOR: Juraj Joscak
// DATE:   01.07.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;

namespace TycoonBuilder
{
	public class CShowDialogMenuTask
	{
		public CShowDialogTask DialogTask { get; private set; }
		
		public CShowDialogMenuTask(CShowDialogTask dialogTask)
		{
			DialogTask = dialogTask;
		}
	}
	
	public class CShowDialogTask
	{
		public bool CanBeClosed { get; private set; }
		public string Header { get; private set; }
		public bool HeaderIsLocalized { get; private set; }
		public object[] HeaderArgs { get; private set; }
		public string Content { get; private set; }
		public bool ContentIsLocalized { get; private set; }
		public object[] ContentArgs { get; private set; }
		public Sprite Picture { get; private set; }
		public string AnalyticsId { get; private set; }
		public CDialogButtonData Button1 { get; private set; }
		public CDialogButtonData Button2 { get; private set; }
		public bool IsOverlay { get; private set; }
		public string SubContent { get; private set; }
		public bool SubContentIsLocalized { get; private set; }
		public object[] SubContentArgs { get; private set; }
		public string SubContentTitle { get; private set; }
		public bool SubContentTitleIsLocalized { get; private set; }
		public object[] SubContentTitleArgs { get; private set; }
		
		public CShowDialogTask()
		{
			CanBeClosed = true;
			Header = null;
			Content = null;
			Picture = null;
			AnalyticsId = null;
			SubContentTitle = null;
			SubContent = null;
		}

		public CShowDialogTask SetCanBeClosed(bool canBeClosed)
		{
			CanBeClosed = canBeClosed;
			return this;
		}
		
		public CShowDialogTask SetHeader(string header)
		{
			Header = header;
			HeaderIsLocalized = false;
			return this;
		}

		public CShowDialogTask SetHeaderLocalized(string key, params object[] args)
		{
			Header = key;
			HeaderIsLocalized = true;
			HeaderArgs = args;
			return this;
		}
		
		public CShowDialogTask SetContent(string content)
		{
			Content = content;
			ContentIsLocalized = false;
			return this;
		}
		
		public CShowDialogTask SetContentLocalized(string key, params object[] args)
		{
			Content = key;
			ContentIsLocalized = true;
			ContentArgs = args;
			return this;
		}
		
		public CShowDialogTask SetSubContentLocalized(string key, params object[] args)
		{
			SubContent = key;
			SubContentIsLocalized = true;
			SubContentArgs = args;
			return this;
		}
		
		public CShowDialogTask SetSubContentTitleLocalized(string key, params object[] args)
		{
			SubContentTitle = key;
			SubContentTitleIsLocalized = true;
			SubContentTitleArgs = args;
			return this;
		}
		
		public CShowDialogTask SetPicture(Sprite picture)
		{
			Picture = picture;
			return this;
		}
		
		public CShowDialogTask SetAnalyticsId(string analyticsId)
		{
			AnalyticsId = analyticsId;
			return this;
		}
		
		public CShowDialogTask SetNoButtons()
		{
			Button1 = null;
			Button2 = null;
			return this;
		}
		
		public CShowDialogTask SetOneButton(CDialogButtonData buttonData)
		{
			Button1 = buttonData;
			return this;
		}
		
		public CShowDialogTask SetTwoButtons(CDialogButtonData button1, CDialogButtonData button2)
		{
			Button1 = button1;
			Button2 = button2;
			return this;
		}
		
		public CShowDialogTask SetOverlay()
		{
			IsOverlay = true;
			return this;
		}
	}
	
	public enum EDialogButtonColor
	{
		None = 0,
		Red,
		Green,
		Blue,
	}
	
	public class CDialogButtonData
	{
		public readonly string Text;
		public readonly Action Callback;
		public readonly EDialogButtonColor Color;
		public readonly bool UseLocalizations;
			
		public CDialogButtonData(string text, Action callback, EDialogButtonColor color, bool useLocalizations)
		{
			Text = text;
			Callback = callback;
			Color = color;
			UseLocalizations = useLocalizations;
		}
	}
}