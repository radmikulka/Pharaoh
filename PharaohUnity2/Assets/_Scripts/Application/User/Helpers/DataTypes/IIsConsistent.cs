// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System;
using System.Collections;
using System.Collections.Generic;

namespace TycoonBuilder
{
	public interface IUserData<out T> : IIsConsistent
	{
		public T LocalValue { get; }
		public T ServerValue { get; }
	}
	
	public interface IIsConsistent
	{
		public bool IsConsistent();
	}
	
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
	public class ValidatableDataAttribute : Attribute
	{
		
	}
}