// =================================
// AUTHOR:		Wojciech Drzymała
// DATE			05.12.2025
// =================================

using AldaEngine;
using AldaEngine.AldaFramework;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using KBCore.Refs;

namespace TycoonBuilder
{
	public class CRegionBirds : ValidatedMonoBehaviour, IInitializable
	{
		[Header("References")]
		[SerializeField, Child] private SplineAnimate[] _splines;
		
		public void Initialize()
		{
			InitializeBirds();
		}

		private void InitializeBirds()
		{
			var shuffled = _splines.OrderBy(_ => Random.value).ToArray();
			for (int i = 0; i < shuffled.Length; i++)
			{
				shuffled[i].StartOffset = (float) i / shuffled.Length;
				shuffled[i].MaxSpeed = Random.Range(38f, 45f);
				shuffled[i].Play();
			}
		}
	}
}