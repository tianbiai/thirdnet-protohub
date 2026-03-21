using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Identity.API.Controllers
{

    [Authorize("Basic")]
    [ApiController]
    [Route("connect/token")]
    public class JwtTokenController(JwtTokenManager manager, IAccountValidator validator) : Controller
    {
        /// <summary>
        /// 验证账号、密码，验证通过后返回token
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] string username, [FromForm] string password, [FromForm]string scope)
        {
            var scopes = scope?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

            //1.验证用户名、密码
             var custom_claims = await validator.Validate(username, password, scopes);

            //2.设置token相关参数
            var client_id = HttpContext.GetCurrentClientId();
            var claims = HttpContext.User.Claims.Where(e => !string.IsNullOrEmpty(e.Value)).ToList();


            //将自定义的claims加入access_token的claims中
            claims.AddRange(custom_claims);
            if (scopes.Contains("offline_access"))
            {
                //需要获取refresh_token
                var access_token = await manager.CreateAccessToken(username, client_id, claims);
                var refresh_token = await manager.CreateRefreshToken(username, client_id, custom_claims);
                return Ok(new JwtTokenResult
                {
                    access_token = access_token,
                    refresh_token = refresh_token
                });
            }
            else
            {
                //只获取access_token
                var access_token = await manager.CreateAccessToken(username, client_id, claims);
                return Ok(new JwtTokenResult
                {
                    access_token = access_token
                });
            }
        }

        /// <summary>
        /// 使用refresh_token获取新的token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Post([FromForm]string refresh_token)
        {
            var result = await manager.CheckToken(refresh_token, "refresh_token", true);
            if (result.IsValid)
            {
                //生成新的access_token和refresh_token。
                var client_id = HttpContext.GetCurrentClientId();
                var claims = HttpContext.User.Claims.Where(e => !string.IsNullOrEmpty(e.Value)).ToList();
                if(result.Claims.TryGetValue("name", out var username))
                {
                    var custom_claims = CreateCustomClaims(result.ClaimsIdentity.Claims);

                    claims.AddRange(custom_claims);
                    //需要获取refresh_token
                    var access_token = await manager.CreateAccessToken(username.ToString(), client_id, claims);
                    var newrefresh_token = await manager.CreateRefreshToken(username.ToString(), client_id, custom_claims);
                    return Ok(new JwtTokenResult
                    {
                        access_token = access_token,
                        refresh_token = newrefresh_token
                    });
                }
            }
            throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "refresh_token无效或已过期，请重新获取。");
        }

        /// <summary>
        /// 根据token中的claims，找出自定义的claims并返回。
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        List<Claim> CreateCustomClaims(IEnumerable<Claim> claims)
        {
            var custom_claims = new List<Claim>();
            //从refresh_token中尝试找到额外自定义的claim，加入access_token中
            var idp = claims.Where(e => e.Type == "idp").FirstOrDefault();
            if(idp != null)
            {
                custom_claims.Add(new Claim(idp.Type, idp.Value));
            }
            return custom_claims;
        }
    }
}
