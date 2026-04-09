using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProtoHub.Api.Controllers;
using ProtoHub.Database;
using System.Reflection;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api
{
    public class Startup
    {
        /// <summary>
        /// 配置信息接口
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// 构造函数，加载配置信息
        /// </summary>
        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        /// <summary>
        /// 配置依赖
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            //初始化配置库
            var assembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddInitDbWithPostgresql(assembly, Configuration["DefaultConnectionString"]);

            //注册 ProtoHubDbContext
            services.AddDbContextPool<ProtoHubDbContext>(options =>
            {
                options.UseNpgsql(Configuration["ConnectionString"], e => e.MigrationsAssembly("ProtoHub.Database"));
                // 抑制 PendingModelChangesWarning（EF Core 10 新行为，ValueConverter + jsonb 会导致 snapshot 误报）
                //options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            });

            services.AddResponseCompression();
            //自定义判断token是否过期
            services.AddSingleton<IAccountTokenTimeCache, CustomAccountTokenTimeCache>();
            //配置限流
            services.AddThirdNetIpAndApplicationPathRateLimiting(Configuration);

            services.AddThirdNetDefaultRSAJwt(Configuration);
            services.AddScoped<IAccountValidator, DefaultAccountValidator>();

            services.AddThirdNetMvcWithPostgresql(Configuration);


            services.Configure<HelpPageOptions>(Configuration.GetSection("HelpPage"));
            services.AddThirdNetVersioningHelpPage(Configuration.GetSection("HelpPage").Get<HelpPageOptions>());
            services.AddControllers();
        }

        /// <summary>
        /// 配置应用
        /// </summary>
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory,
                                    IApiVersionDescriptionProvider provider, IOptions<HelpPageOptions> options)
        {
            app.InitializeDatabase();
            app.InitializeProtoHubDatabase();

            //启用压缩，开启数据压缩功能
            app.UseResponseCompression();
            //跨域
            var log = loggerFactory.CreateLogger<ILogger<Startup>>();
            app.UseCors(builder =>
            {
                builder.SetIsOriginAllowed(e =>
                {
                    var origin_bol = true;
                    if (!origin_bol) { log.LogError($"跨域的请求:{e}"); }
                    return origin_bol;
                })
                .AllowAnyHeader()
                .WithMethods("GET", "POST", "PUT", "DELETE")
                .AllowCredentials();
            });

            //启用帮助页面，帮助页面，根据api及配置文件，自动生成帮助文档，地址：基地址/help
            app.UseThirdNetVersioningHelpPage(provider, options.Value);


            app.UseThirdNetMvc(loggerFactory);
        }
    }
}
