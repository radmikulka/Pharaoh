// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using AldaEngine.AldaFramework;
using KBCore.Refs;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CRegionPoint : ValidatedMonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField, SearchableEnum] private ERegionPoint _pointId;

		private CRegionPoints _regionPoints;
		private CRegionController _region;
		
		public Vector3 Position { get; private set; }
		public virtual ERegionPoint PointId => _pointId;

		[Inject]
		private void Inject(CRegionPoints regionPoints, CRegionController region)
		{
			_regionPoints = regionPoints;
			_region = region;
		}

		public void Construct()
		{
			Position = transform.position;
		}

		public void Initialize()
		{
			_regionPoints.RegisterPoint(PointId, _region.RegionId, this);
		}
	}
}