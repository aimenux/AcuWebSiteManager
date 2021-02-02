using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductsOpenGraphType
    {
        [EnumMember(Value = "product")]
        Product = 0,

        [EnumMember(Value = "album")]
        Album = 1,

        [EnumMember(Value = "book")]
        Book = 2,

        [EnumMember(Value = "drink")]
        Drink = 3,

        [EnumMember(Value = "food")]
        Food = 4,

        [EnumMember(Value = "game")]
        Game = 5,

        [EnumMember(Value = "movie")]
        Movie = 6,

        [EnumMember(Value = "song")]
        Song = 7,

        [EnumMember(Value = "tv_show")]
        TvShow = 8
    }
}
