using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using RestSharp;
using RestSharp.Extensions;

namespace PX.Commerce.Shopify.API.REST
{
    public abstract class FilterBase : IFilter
    {
        protected const string ISO_8601_DATE_FORMAT = "{0:yyyy-MM-ddTHH:mm:sszzz}";

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
                        value = string.Format(ISO_8601_DATE_FORMAT, value);
                    }
					else if(value is Enum)
					{
						var memInfo = value.GetType().GetMember(value.ToString()).FirstOrDefault();
						var memAttr = memInfo?.GetAttribute<EnumMemberAttribute>();
						if(memAttr != null)
						{
							value = memAttr.Value;
						}
					}
                    request.AddParameter(key, value);
                }
            }            
        }
    }

	public class FilterWithID : FilterBase, IFilterWithIDs
	{
		/// <summary>
		/// Restrict results to customers specified by a comma-separated list of IDs.
		/// </summary>
		[Description("ids")]
		public string IDs { get; set; }
	}

	public class FilterWithSinceID : FilterBase, IFilterWithSinceID
	{
		/// <summary>
		/// Restrict results to those after the specified ID.
		/// </summary>
		[Description("since_id")]
		public string SinceID { get; set; }
	}

	public class FilterWithFields : FilterBase, IFilterWithFields
	{
		/// <summary>
		/// Show only certain fields, specified by a comma-separated list of field names.
		/// </summary>
		[Description("fields")]
		public string Fields { get; set; }
	}

	public class FilterWithLimit : FilterBase, IFilterWithLimit
	{
		/// <summary>
		/// The maximum number of results to show.
		/// (default: 50, maximum: 250)
		/// </summary>
		[Description("limit")]
		public int? Limit { get; set; }
	}

	public class FilterWithDateTime : FilterBase, IFilterWithDateTime
	{
		/// <summary>
		/// Show customers created after a specified date.
		///(format: 2014-04-25T16:15:47-04:00)
		/// </summary>
		[Description("created_at_min")]
		public DateTime? CreatedAtMin { get; set; }

		/// <summary>
		/// Show customers created before a specified date.
		///(format: 2014-04-25T16:15:47-04:00)
		/// </summary>
		[Description("created_at_max")]
		public DateTime? CreatedAtMax { get; set; }

		/// <summary>
		/// Show customers last updated after a specified date.
		///(format: 2014-04-25T16:15:47-04:00)
		/// </summary>
		[Description("updated_at_min")]
        public DateTime? UpdatedAtMin { get; set; }

        /// <summary>
        /// Show customers last updated before a specified date.
        ///(format: 2014-04-25T16:15:47-04:00)
        /// </summary>
        [Description("updated_at_max")]
		public DateTime? UpdatedAtMax { get; set; }
	}

	public class FilterWithDateTimeAndLimit : FilterBase, IFilterWithDateTime, IFilterWithLimit
	{
		/// <summary>
		/// The maximum number of results to show.
		/// (default: 50, maximum: 250)
		/// </summary>
		[Description("limit")]
		public int? Limit { get; set; }
		/// <summary>
		/// Show customers created after a specified date.
		///(format: 2014-04-25T16:15:47-04:00)
		/// </summary>
		[Description("created_at_min")]
		public DateTime? CreatedAtMin { get; set; }

		/// <summary>
		/// Show customers created before a specified date.
		///(format: 2014-04-25T16:15:47-04:00)
		/// </summary>
		[Description("created_at_max")]
		public DateTime? CreatedAtMax { get; set; }

		/// <summary>
		/// Show customers last updated after a specified date.
		///(format: 2014-04-25T16:15:47-04:00)
		/// </summary>
		[Description("updated_at_min")]
		public DateTime? UpdatedAtMin { get; set; }

		/// <summary>
		/// Show customers last updated before a specified date.
		///(format: 2014-04-25T16:15:47-04:00)
		/// </summary>
		[Description("updated_at_max")]
		public DateTime? UpdatedAtMax { get; set; }
	}

}
