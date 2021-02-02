using System;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CR;

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public delegate DateTime? String2DateConverterFunc(string s);
	public class CCProcessingContext
	{
		public CCProcessingCenter processingCenter = null;
		public int? aPMInstanceID = null;
		public int? aCustomerID = 0;
		public string aCustomerCD = null;
		public string PrefixForCustomerCD = null;
		public string aDocType = null;
		public string aRefNbr = null;
		public PXGraph callerGraph = null;
		public String2DateConverterFunc expirationDateConverter = null;
	}
}
