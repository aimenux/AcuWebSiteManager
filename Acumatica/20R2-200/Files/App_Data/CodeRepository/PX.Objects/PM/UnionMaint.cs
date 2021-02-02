using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Collections;


namespace PX.Objects.PM
{
	public class UnionMaint : PXGraph<UnionMaint>, PXImportAttribute.IPXPrepareItems
	{
		[PXImport(typeof(PMUnion))]
		public PXSelect<PMUnion> Items;
		public PXSavePerRow<PMUnion> Save;
		public PXCancel<PMUnion> Cancel;

		#region PMImport Implementation
		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public void PrepareItems(string viewName, IEnumerable items) { }
		#endregion
	}
}
