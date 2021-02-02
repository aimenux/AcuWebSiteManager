using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce.API.WebDAV
{
    public interface IWebDAVEntity
    {
        string ImageUrl { get; set; }
        string ImageFile { get; set; }
    }
}
