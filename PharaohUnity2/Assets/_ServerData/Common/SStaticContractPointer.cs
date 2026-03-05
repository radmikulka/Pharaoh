// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.08.2025
// =========================================

using System;
using ServerData;

namespace ServerData
{
	public struct SStaticContractPointer : IEquatable<SStaticContractPointer>
	{
		public readonly EStaticContractId Id;
		public readonly int Task;
		
		public SStaticContractPointer(EStaticContractId id, int task)
		{
			Id = id;
			Task = task;
		}

		public override string ToString()
		{
			return $"{nameof(Id)}: {Id}, {nameof(Task)}: {Task}";
		}

		public static bool operator ==(SStaticContractPointer a, SStaticContractPointer b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(SStaticContractPointer a, SStaticContractPointer b)
		{
			return !(a == b);
		}

		public bool Equals(SStaticContractPointer other)
		{
			return Id == other.Id && Task == other.Task;
		}

		public override bool Equals(object obj)
		{
			return obj is SStaticContractPointer other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine((int)Id, Task);
		}
	}
}