using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using ThirdNet.Core.AspNetCore;
using ProtoHub.Identity.Database;
using ProtoHub.Identity.API.Services;

namespace ProtoHub.Identity.API
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

            // 注册 ProtoHub 数据库上下文
            services.AddDbContext<ProtoHubDbContext>(options =>
                options.UseNpgsql(Configuration["DefaultConnectionString"]));

            // 注册权限服务
            services.AddScoped<IPermissionService, PermissionService>();

            //启用压缩,若web 服务器支持服务器层的压缩，则建议直接配置web服务器层压缩（效率会比框架的压缩高）
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
            //app.InitializeDatabase();
            //初始化token表
            //app.InitializeTokenDatabase();

            //启用压缩，开启数据压缩功能
            app.UseResponseCompression();

            //启用帮助页面，帮助页面，根据api及配置文件，自动生成帮助文档，地址：基地址/help
            app.UseThirdNetVersioningHelpPage(provider, options.Value);


            app.UseThirdNetMvc(loggerFactory);
        }
    }
}
