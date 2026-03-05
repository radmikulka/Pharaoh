// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;
using ILogger = AldaEngine.ILogger;

namespace TycoonBuilder
{
	public class CRegionPoints : IAldaFrameworkComponent
	{
		private readonly ILogger _logger;
		private readonly Dictionary<SRegionPoint, CRegionPoint> _points = new();

		public CRegionPoints(ILogger logger)
		{
			_logger = logger;
		}

		public void RegisterPoint(ERegionPoint pointId, ERegion region, CRegionPoint regionPoint)
		{
			SRegionPoint key = new(pointId, region);
			if (_points.TryGetValue(key, out CRegionPoint existingPoint))
			{
				Debug.LogError($"Region point for {pointId} is already registered for {existingPoint.gameObject.name} - cant register new for {regionPoint.gameObject.name}", existingPoint.gameObject);
				return;
			}
			_points.Add(key, regionPoint);
		}

		public CRegionPoint GetPoint(ERegionPoint pointId, ERegion region)
		{
			SRegionPoint key = new(pointId, region);
			if (!_points.TryGetValue(key, out var regionPoint))
			{
				_logger.LogError($"Region point not found: {pointId} in region {region}");
			}
			return regionPoint;
		}
	}
}