// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.ComponentModel;
using AldaEngine;

namespace ServerData
{
	public interface IOfferParam : IMapAble
	{
		EOfferParam Id { get; }
		T GetValue<T>();
		T GetValueOrDefault<T>();
	}
}