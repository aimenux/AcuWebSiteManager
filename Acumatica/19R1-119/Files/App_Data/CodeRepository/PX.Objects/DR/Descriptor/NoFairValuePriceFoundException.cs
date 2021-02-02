using System;
using System.Runtime.Serialization;
using PX.Data;

namespace PX.Objects.DR
{
	public class NoFairValuePriceFoundException : PXException
	{
		public NoFairValuePriceFoundException(string InventoryCD, string UOM, string CuryID, DateTime DocDate) : base(Messages.NoFairValuePriceFoundForItem, InventoryCD.Trim(), UOM, CuryID, DocDate.ToShortDateString()) { }
		public NoFairValuePriceFoundException(string ComponentCD, string InventoryCD, string UOM, string CuryID, DateTime DocDate) : base(Messages.NoFairValuePriceFoundForComponent, ComponentCD, InventoryCD.Trim(), UOM, CuryID, DocDate.ToShortDateString()) { }
		public NoFairValuePriceFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	public class NoFairValuePricesFoundException : PXException
	{
		public NoFairValuePricesFoundException(string message) : base(message) { }
		public NoFairValuePricesFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
