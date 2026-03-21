using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Identity.API
{
    public class DefaultAccountValidator : IAccountValidator
    {
        public async Task<List<Claim>> Validate(string account, string password, string[] scopes)
        {
            //根据scope验证帐号密码


            //创建自定义claims。当有refresh_token时，需要修改CreateCustomClaims，创建对应的claims。
            //claims不应保存敏感信息，只保存标识信息。
            var custom_claims = new List<Claim>();
            //身份提供者,对应之前的用户身份wechat，tourist，wechatapp等
            custom_claims.Add(new Claim("idp", "wechat"));
            //住户id
            custom_claims.Add(new Claim(ClaimTypes.NameIdentifier, "13444"));


            return custom_claims;
        }
    }
}
