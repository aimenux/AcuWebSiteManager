using System;
using System.ComponentModel;
using RestSharp;
using RestSharp.Extensions;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class Filter : IFilter
    {
        protected const string RFC2822_DATE_FORMAT = "{0:ddd, dd MMM yyyy HH:mm:ss} GMT";
        protected const string ISO_DATE_FORMAT = "{0:yyyy-MM-ddTHH:mm:ss}";        
        
        [Description("limit")]
        public int? Limit { get; set; }

        [Description("page")]
        public int? Page { get; set; }

        public virtual void AddFilter(IRestRequest request)
        {
           foreach (var propertyInfo in GetType().GetProperties())
            {
				DescriptionAttribute attr = propertyInfo.GetAttribute<DescriptionAttribute>();
                if (attr == null) continue;
                String key = attr.Description;
                Object value = propertyInfo.GetValue(this);
                if (value != null)
                {
                    if (propertyInfo.PropertyType == typeof(DateTime) || propertyInfo.PropertyType == typeof(DateTime?))
                    {
                        value = string.Format(ISO_DATE_FORMAT, value);
                    }
                
                    request.AddParameter(key, value);
                }
            }            
        }
    }
    
}
