using PX.Data;
using System.Collections;


namespace PX.Objects.PM
{
	public class WorkCodeMaint : PXGraph<WorkCodeMaint>, PXImportAttribute.IPXPrepareItems
	{
		[PXImport(typeof(PMWorkCode))]
		public PXSelect<PMWorkCode> Items;
		public PXSavePerRow<PMWorkCode> Save;
		public PXCancel<PMWorkCode> Cancel;

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
