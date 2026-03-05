// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.02.2026
// =========================================

using System;
using AldaEngine;
using UnityEngine;

namespace TycoonBuilder
{
	[Serializable]
	public class CVehicleDetailRecords
	{
		[SerializeField] private string _manufacturer;
		[SerializeField] private int _year;
		[SerializeField] private ECountryCode _country;
		[SerializeField] private string _engine;
		[SerializeField] private int _maxSpeed;
		[SerializeField] private CVehicleDetailRecord _additionalRecords;
		
		public string Manufacturer => _manufacturer;
		public int Year => _year;
		public ECountryCode Country => _country;
		public string Engine => _engine;
		public int MaxSpeed => _maxSpeed;
		
		public CVehicleDetailRecord GetAdditionalRecordOrDefault(EVehicleDetailRecord recordType)
		{
			return _additionalRecords.RecordType == recordType ? _additionalRecords : null;
		}
	}
}