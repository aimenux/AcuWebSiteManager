using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.BigCommerce.API.REST
{

    public class StoreTime
    {
		[JsonIgnore]
		public virtual DateTime? CurrentDateTime { get; set; }

		[JsonProperty("time")]
		public virtual string CurrentDateTimeUT
		{
			get => CurrentDateTime.TDToString();
			set => CurrentDateTime = value.ToDate();
		}
	}

}
