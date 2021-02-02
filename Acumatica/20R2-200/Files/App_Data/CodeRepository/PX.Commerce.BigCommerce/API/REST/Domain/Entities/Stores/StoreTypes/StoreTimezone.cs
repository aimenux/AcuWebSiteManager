using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{

    public class StoreTimezone
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("raw_offset")]
        public int RawOffset { get; set; }

        [JsonProperty("dst_offset")]
        public int DstOffset { get; set; }

        [JsonProperty("dst_correction")]
        public bool DstCorrection { get; set; }

        [JsonProperty("date_format")]
        public StoreDateFormat DateFormat { get; set; }
    }

}
