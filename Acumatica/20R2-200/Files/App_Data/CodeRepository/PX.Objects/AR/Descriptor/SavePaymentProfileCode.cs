using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.Common;

namespace PX.Objects.AR
{ 
	static class SavePaymentProfileCode 
	{
		public const string Allow = "A";
		public const string Force = "F";
		public const string Prohibit = "P";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base( ValueLabelPairs() )
			{
			
			}
		}

		public static Tuple<string, string>[] ValueLabelPairs()
		{
			var arr = new Tuple<string, string>[]
			{
				new Tuple<string,string>( Allow, Messages.CCUponConfirmationSave),
				new Tuple<string, string>( Force, Messages.CCAlwaysSave),
				new Tuple<string, string>( Prohibit, Messages.CCNeverSave)
			};
			return arr;
		}
	}
}
