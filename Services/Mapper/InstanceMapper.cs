using AutoMapper;
using Contracts;
using Contracts.Dtos;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Mapper
{
	public class InstanceMapper : IInstanceMapper
	{
		private readonly IMapper mapper;
		private readonly MapperConfiguration config;

		public InstanceMapper()
		{
			config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<User, UserDto>();
			});

			mapper = config.CreateMapper();
		}

		public TDestination Map<TDestination>(object source)
		{
			if (source == null)
				return default;
			return mapper.Map<TDestination>(source);
		}
	}
}
