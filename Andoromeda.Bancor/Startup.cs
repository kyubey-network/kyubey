using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Andoromeda.Bancor.Models;

namespace Andoromeda.Bancor
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfiguration(out var Config);
            services.AddMvc();
            services.AddEntityFrameworkMySql()
                .AddDbContext<KyubeyContext>(x =>
                {
                    x.UseMySql(Config["MySQL"]);
                });
            services.AddTimedJob();
            services.AddBlobStorage()
                .AddEntityFrameworkStorage<KyubeyContext>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseBlobStorage("/js/jquery.fileupload.js");
            app.UseMvcWithDefaultRoute();
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<KyubeyContext>().Database.EnsureCreated();
                app.UseTimedJob();
            }
        }
    }
}
