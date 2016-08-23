using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.Models
{
    public interface IUser
    {
        string UserName { get; set; }
    }
}
