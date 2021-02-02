using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Collections;

namespace PX.Objects.TX
{
	public class TaxImportDataMaint : PXGraph<TaxImportDataMaint>
	{
		public PXSelect<TXImportFileData> Data;
        public PXSavePerRow<TXImportFileData> Save;
        public PXCancel<TXImportFileData> Cancel;

		private bool _importing;
		private bool _cleared = false;

		protected virtual void TXImportFileData_StateCode_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null) _importing = sender.GetValuePending(e.Row, PXImportAttribute.ImportFlag) != null;
		}


		protected virtual void TXImportFileData_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			TXImportFileData row = e.Row as TXImportFileData;
			if (row != null)
			{
				if (_importing && !_cleared)
				{
					PXDatabase.Delete<TXImportFileData>();
					_cleared = true;
				}

			}
		}

		public override void Persist()
		{
			base.Persist();
			Clear();
		}
	}

	public class TaxImportZipDataMaint : PXGraph<TaxImportZipDataMaint>
	{
		public PXSelect<TXImportZipFileData> Data;
        public PXSavePerRow<TXImportZipFileData> Save;
        public PXCancel<TXImportZipFileData> Cancel;

		private bool _importing;
		private bool _cleared = false;

		protected virtual void TXImportZipFileData_ZipCode_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null) _importing = sender.GetValuePending(e.Row, PXImportAttribute.ImportFlag) != null;
		}


		protected virtual void TXImportZipFileData_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			TXImportZipFileData row = e.Row as TXImportZipFileData;
			if (row != null)
			{
				if (_importing && !_cleared)
				{
					PXDatabase.Delete<TXImportZipFileData>();
					_cleared = true;
				}

			}
		}

		public override void Persist()
		{
			base.Persist();
			Clear();
		}
	}
}
