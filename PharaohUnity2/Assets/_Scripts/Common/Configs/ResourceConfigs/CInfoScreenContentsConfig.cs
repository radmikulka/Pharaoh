// =========================================
// AUTHOR: Marek Karaba
// DATE:   28.07.2025
// =========================================

using System;
using System.Collections.Generic;
using AldaEngine;
using UnityEngine;
using UnityEngine.Serialization;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/InfoScreenContents", fileName = "cfg_infoScreenContent")]
	public class CInfoScreenContentsConfig : ScriptableObject, IIHaveBundleLinks
	{
		public SInfoScreenContent[] _contents;

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			foreach (SInfoScreenContent content in _contents)
			{
				foreach (SInfoScreenContentPart contentPart in content._parts)
				{
					yield return contentPart._image;
				}
			}
		}

		public SInfoScreenContent GetContent(EScreenInfoId id)
		{
			return FindContent(id); 
		}

		private SInfoScreenContent FindContent(EScreenInfoId id)
		{
			foreach (SInfoScreenContent content in _contents)
			{
				if (content._contentId == id)
					return content;
			}

			throw new Exception($"Info content {id} not found");
		}
		
		[Serializable]
		public struct SInfoScreenContent
		{
			public EScreenInfoId _contentId;
			public bool _useAlternateScreen;
			public SInfoScreenContentPart[] _parts;
			public string _bottomTextLocalizationKey;
		}

		[Serializable]
		public struct SInfoScreenContentPart
		{
			[FormerlySerializedAs("_textLocalizationKey")] public string _titleLocalizationKey;
			public string _descriptionLocalizationKey;
			[BundleLink(true, typeof(Sprite))] public CBundleLink _image;
			[BundleLink(true, typeof(Sprite))] public CBundleLink _icon;
		}
	}
}