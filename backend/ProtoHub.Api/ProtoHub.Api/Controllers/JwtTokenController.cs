using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api.Controllers
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
        /// 根据token中的claims，提取自定义的claims（permission + idp）返回。
        /// </summary>
        List<Claim> CreateCustomClaims(IEnumerable<Claim> claims)
        {
            var custom_claims = new List<Claim>();

            // 提取 idp claim
            var idp = claims.FirstOrDefault(e => e.Type == "idp");
            if (idp != null)
            {
                custom_claims.Add(new Claim(idp.Type, idp.Value));
            }

            // 提取所有 permission claims（token 中只存 permission，不存 role）
            foreach (var claim in claims.Where(e => e.Type == "permission"))
            {
                custom_claims.Add(new Claim("permission", claim.Value));
            }

            // 提取基础 claims
            var nameId = claims.FirstOrDefault(e => e.Type == ClaimTypes.NameIdentifier);
            if (nameId != null)
            {
                custom_claims.Add(new Claim(ClaimTypes.NameIdentifier, nameId.Value));
            }

            var name = claims.FirstOrDefault(e => e.Type == ClaimTypes.Name);
            if (name != null)
            {
                custom_claims.Add(new Claim(ClaimTypes.Name, name.Value));
            }

            var nickName = claims.FirstOrDefault(e => e.Type == "nick_name");
            if (nickName != null)
            {
                custom_claims.Add(new Claim("nick_name", nickName.Value));
            }

            return custom_claims;
        }
    }
}
