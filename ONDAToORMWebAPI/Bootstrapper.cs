﻿using Contracts;
using Contracts.Interfaces;
using DataAccess;
using Domain.Repositories;
using Services;
using Services.Mapper;

namespace ONDAToORMWebAPI
{
    public static class Bootstrapper
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IInstanceMapper, InstanceMapper>();

            services.AddTransient<IGenerateTokenService, GenerateTokenService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IConverterService, ConverterService>();
            services.AddTransient<ITemporaryDatabaseRepositoryFactory, TemporaryDatabaseRepositoryFactory>();


            services.AddTransient<IUserRepository, UserRepository>();

            return services;
        }
    }
}
