// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CContractBuilder<T> where T : CContractBuilder<T>
	{
		protected readonly List<CContractTaskBuilder> Tasks = new();
		protected readonly List<IUnlockRequirement> UnlockRequirements = new();
		protected ECustomer Customer;
		protected ERegion Region;
		
		public readonly CTripPriceBuilder TripPrice = new();

		public T SetUnlockYear(EYearMilestone unlockYear)
		{
			UnlockRequirements.Add(IUnlockRequirement.Year(unlockYear));
			return (T)this;
		}

		public CContractTaskBuilder AddTask()
		{
			CContractTaskBuilder task = new();
			Tasks.Add(task);
			return task;
		}
		
		public T SetCustomer(ECustomer customer)
		{
			Customer = customer;
			return (T)this;
		}
		
		public T SetRegion(ERegion region)
		{
			Region = region;
			return (T)this;
		}
		
		protected CContractTask[] BuildTasks()
		{
			CContractTask[] tasks = new CContractTask[Tasks.Count];
			for (int i = 0; i < Tasks.Count; i++)
			{
				tasks[i] = Tasks[i].Build();
			}
			return tasks;
		}
	}
}

