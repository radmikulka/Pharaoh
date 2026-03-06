// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.10.2024
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using ServerData.Hits;

namespace TycoonBuilder
{
	public class CUserProgress : CBaseUserComponent
	{
		public EMissionId Mission { get; private set; }

		public void InitialSync(CProgressDto dto)
		{
			Mission = dto.MissionId;
		}
	}
}