using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThirdNet.Core.AspNetCore;
using ThirdNet.Core.Common;

namespace ProtoHub.Api.Controllers
{
    [ApiController]
    [Authorize(Policy ="Logon")]
    [Route("api/cache")]
    public class CacheController : Controller
    {
        List<ISessionRefresh> _list;

        /// <summary>
        /// 传入需要更新的缓存
        /// </summary>
        public CacheController(ISessionReader<string, ApplicationInfoMap> app_cache,
                                ISessionReader<IpKey, IpWhiteList> white_cache,
                                ISessionReader<string, IpBlackList> black_cache,
                                ISessionReader<string, List<ApplicationAuthority>> auth_cache,
                                ISessionReader<string, List<RolesAuthority>> role_cache)
        {
            _list = new List<ISessionRefresh>
            {
                app_cache as ISessionRefresh,
                white_cache as ISessionRefresh,
                black_cache as ISessionRefresh,
                auth_cache as ISessionRefresh,
                role_cache as ISessionRefresh
            };
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        [HttpGet]
        public async Task Get()
        {
            foreach (var item in _list)
            {
                await item.RefreshAsync();
            }
        }
    }
}
