// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CContractConfig
	{
		private readonly CContractTask[] _tasks;

		public readonly CTripPrice TripPrice;

		public IReadOnlyList<CContractTask> Tasks => _tasks;
		public int TasksCount => Tasks.Count;

		protected CContractConfig(
			CContractTask[] tasks, 
			CTripPrice tripPrice
			)
		{
			TripPrice = tripPrice;
			_tasks = tasks;
		}
		
		public CContractTask GetTaskConfig(int taskIndex)
		{
			return _tasks[taskIndex];
		}
	}
}