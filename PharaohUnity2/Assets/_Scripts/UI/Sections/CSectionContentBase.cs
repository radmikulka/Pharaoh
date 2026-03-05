// =========================================
// AUTHOR: Marek Karaba
// DATE:   15.07.2025
// =========================================

using System;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CSectionContentBase : ValidatedMonoBehaviour, IConstructable
	{
		private ISectionContentPart[] _sectionContentParts;

		public virtual void Construct()
		{
			_sectionContentParts = GetComponentsInChildren<ISectionContentPart>();
		}

		public virtual void Show()
		{
			gameObject.SetActive(true);
			foreach (ISectionContentPart part in _sectionContentParts)
			{
				part.OnShow();
			}
		}

		public virtual void Hide()
		{
			gameObject.SetActive(false);
			foreach (ISectionContentPart part in _sectionContentParts)
			{
				part.OnHide();
			}
		}
	}
}