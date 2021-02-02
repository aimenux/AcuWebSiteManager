using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	public class ProductImageResponse : IEntityResponse<ProductImageData>
	{
		[JsonProperty("image")]
		public ProductImageData Data { get; set; }
	}

	public class ProductImagesResponse : IEntitiesResponse<ProductImageData>
	{
		[JsonProperty("images")]
		public IEnumerable<ProductImageData> Data { get; set; }
	}

	[JsonObject(Description = "Product -> Product Image")]
	[Description("Product Image")]
	public class ProductImageData : BCAPIEntity
	{

		/// <summary>
		/// The date and time when the product image was created. The API returns this value in ISO 8601 format.
		/// </summary>
		[JsonProperty("created_at")]
		[ShouldNotSerialize]
		public DateTime? DateCreatedAt { get; set; }

		/// <summary>
		/// A unique numeric identifier for the product image.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public long? Id { get; set; }

		/// <summary>
		/// The order of the product image in the list. The first product image is at position 1 and is the "main" image for the product.
		/// </summary>
		[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
		public int? Position { get; set; }

		/// <summary>
		/// The id of the product associated with the image.
		/// </summary>
		[JsonProperty("product_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? ProductId { get; set; }

		/// <summary>
		/// An array of variant ids associated with the image.
		/// </summary>
		[JsonProperty("variant_ids", NullValueHandling = NullValueHandling.Ignore)]
		public long?[] VariantIds { get; set; }

		/// <summary>
		/// Specifies the location of the product image. This parameter supports URL filters that you can use to retrieve modified copies of the image. 
		/// For example, add _small, to the filename to retrieve a scaled copy of the image at 100 x 100 px (for example, ipod-nano_small.png), 
		/// or add _2048x2048 to retrieve a copy of the image constrained at 2048 x 2048 px resolution (for example, ipod-nano_2048x2048.png).
		/// </summary>
		[JsonProperty("src", NullValueHandling = NullValueHandling.Ignore)]
		[ShouldNotSerialize]
		public String Src { get; set; }

		/// <summary>
		/// The alt tag content for the image
		/// </summary>
		[JsonProperty("alt", NullValueHandling = NullValueHandling.Ignore)]
		public String Alt { get; set; }

		/// <summary>
		/// Width dimension of the image which is determined on upload.
		/// </summary>
		[JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
		[ShouldNotSerialize]
		public int? Width { get; set; }

		/// <summary>
		/// Height dimension of the image which is determined on upload.
		/// </summary>
		[JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
		[ShouldNotSerialize]
		public int? Height { get; set; }

		/// <summary>
		/// The  image data
		/// </summary>
		[JsonProperty("attachment", NullValueHandling = NullValueHandling.Ignore)]
		[ShouldNotDeserialize]
		public String Attachment { get; set; }

		/// <summary>
		/// The image file name
		/// </summary>
		[JsonProperty("filename", NullValueHandling = NullValueHandling.Ignore)]
		[ShouldNotDeserialize]
		public String Filename { get; set; }

		/// <summary>
		/// The date and time when the product image was last modified. The API returns this value in ISO 8601 format.
		/// </summary>
		[JsonProperty("updated_at")]
		[ShouldNotSerialize]
		public String DateModifiedAt { get; set; }

		/// <summary>
		/// Attaches additional metadata to a shop's resources:
		///key(required) : An identifier for the metafield(maximum of 30 characters).
		///namespace(required): A container for a set of metadata(maximum of 20 characters). Namespaces help distinguish between metadata that you created and metadata created by another individual with a similar namespace.
		///value (required): Information to be stored as metadata.
		///value_type(required): The value type.Valid values: string and integer.
		///description(optional): Additional information about the metafield.
		/// </summary>
		[JsonProperty("metafields", NullValueHandling = NullValueHandling.Ignore)]
		public List<MetafieldData> Metafields { get; set; }
	}


}
