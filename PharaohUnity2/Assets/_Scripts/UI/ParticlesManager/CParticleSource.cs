// =========================================
// AUTHOR: Marek Karaba
// DATE:   21.07.2025
// =========================================

using System;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[Serializable]
	public class CParticleSource : IParticleSource
	{
		[SerializeField] private EValuable _id;
		[SerializeField] private RectTransform _sourceRect;
		
		public EValuable Id => _id;
		public RectTransform SourceRect => _sourceRect;

		public CParticleSource(EValuable id, RectTransform sourceRect)
		{
			_id = id;
			_sourceRect = sourceRect;
		}
	}
}