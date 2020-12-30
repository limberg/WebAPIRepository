using Entities;
using Entities.Contracts;
using LoggerService;
using LoggerService.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIRepository.Extensions
{
    public static class ServiceExtenstions
    {

        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerManager, LoggerManager>();
        }

        public static void ConfigureSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            string connString = configuration.GetConnectionString("NorthwindConnection");

            services.AddDbContext<RepositoryContext>(options =>
            {
                options.UseSqlServer(connString);
            });
        }

        public static void ConfigureRepositoryWrapper(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
        }
    }
}
