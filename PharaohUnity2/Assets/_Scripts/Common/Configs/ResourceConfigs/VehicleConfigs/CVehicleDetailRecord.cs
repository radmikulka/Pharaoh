// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.02.2026
// =========================================

using System;
using UnityEngine;

namespace TycoonBuilder
{
	[Serializable]
	public class CVehicleDetailRecord
	{
		[SerializeField] private EVehicleDetailRecord _recordType;
		[SerializeField] private string _value;
		
		public EVehicleDetailRecord RecordType => _recordType;
		public string Value => _value;
	}
}