using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Pomelo.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace Andoromeda.Kyubey.IBO
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfiguration(out var config);
            services.AddMvc();
            services.AddPomeloLocalization(x =>
            {
                x.AddCulture(new string[] { "en", "en-US", "en-GB" }, new JsonLocalizedStringStore(Path.Combine("Localization", "en-US.json")));
                x.AddCulture(new string[] { "zh", "zh-CN", "zh-Hans", "zh-Hans-CN", "zh-cn" }, new JsonLocalizedStringStore(Path.Combine("Localization", "zh-CN.json")));
                x.AddCulture(new string[] { "ja", "ja-JP" }, new JsonLocalizedStringStore(Path.Combine("Localization", "ja-JP.json")));
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseFrontendLocalizer("/js/localization.js");
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
