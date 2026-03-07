// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.12.2023
// =========================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AldaEngine;
using ServerData.Hits;

namespace Pharaoh
{
	public class CRequestQueue
	{
		private readonly ConcurrentQueue<CRequestRecord> _queue = new();

		public bool IsEmpty => _queue.IsEmpty;

		public void Enqueue(CRequestRecord hit)
		{
			_queue.Enqueue(hit);
		}

		public CRequestRecord[] GetNextRecordsGroupOrDefault()
		{
			List<CRequestRecord> records = null;
			while (_queue.TryDequeue(out CRequestRecord hit))
			{
				records ??= new List<CRequestRecord>();
				records.Add(hit);

				if (hit.SendAsSingleHit)
				{
					break;
				}
			}

			return records?.ToArray();
		}

		public DateTime? GetOldestRecordCreationTime()
		{
			if (_queue.TryPeek(out CRequestRecord hit))
			{
				return hit.CreationTime;
			}

			return null;
		}

		public void Clear()
		{
			_queue.Clear();
		}
	}
}