using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.TX
{
	public class TaxImportSettings : PXGraph<TaxImportSettings>
	{
		public PXSave<TXImportSettings> Save;
		public PXCancel<TXImportSettings> Cancel;
		public PXSelect<TXImportSettings> Settings;
		public PXSelect<TXImportState> Items;
	}
}
