using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdNet.Core.AspNetCore
{
    public class CustomAccountTokenTimeCache : IAccountTokenTimeCache
    {
        public bool TryGet(string key, out DateTime time)
        {
            time = DateTime.Now;
            return true;
        }
    }
}
