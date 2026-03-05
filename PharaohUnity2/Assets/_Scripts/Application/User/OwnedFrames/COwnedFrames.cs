// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.01.2026
// =========================================

using System.Collections.Generic;
using ServerData;

namespace TycoonBuilder
{
	public class COwnedFrames : CBaseUserComponent
	{
		private readonly HashSet<EProfileFrame> _ownedFrames = new();

		public void InitialSync(COwnedFramesDto dto)
		{
			_ownedFrames.UnionWith(dto.OwnedFrames);
		}

		public void Add(EProfileFrame frame)
		{
			_ownedFrames.Add(frame);
		}

		public bool Contains(EProfileFrame frame)
		{
			return _ownedFrames.Contains(frame);
		}
	}
}