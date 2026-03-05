// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.12.2023
// =========================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AldaEngine;
using ServerData.Hits;

namespace TycoonBuilder
{
	public class CHitsQueue
	{
		private readonly ConcurrentQueue<CHitRecord> _queue = new();

		public bool IsEmpty => _queue.IsEmpty;

		public void Enqueue(CHitRecord hit)
		{
			_queue.Enqueue(hit);
		}

		public CHitRecord[] GetNextRecordsGroupOrDefault()
		{
			List<CHitRecord> records = null;
			while (_queue.TryDequeue(out CHitRecord hit))
			{
				records ??= new List<CHitRecord>();
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
			if (_queue.TryPeek(out CHitRecord hit))
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