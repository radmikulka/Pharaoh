// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.Linq;
using AldaEngine;
using ServerData;
using ServerData.Dto;

namespace Pharaoh
{
	public class CUserMapper
	{
		private readonly IMapper _mapper;

		public CUserMapper(IMapper mapper)
		{
			_mapper = mapper;
		}
	}
}
