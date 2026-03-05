// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.08.2025
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public interface IParticleSource
	{
		public EValuable Id { get; }
		public RectTransform SourceRect { get; }
	}
}