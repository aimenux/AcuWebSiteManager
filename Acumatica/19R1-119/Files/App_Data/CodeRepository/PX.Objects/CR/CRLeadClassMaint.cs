using PX.Data;

namespace PX.Objects.CR
{
	public class CRLeadClassMaint : PXGraph<CRLeadClassMaint, CRContactClass>
	{
		[PXViewName(Messages.LeadClass)]
		public PXSelect<CRContactClass> 
			LeadClass;

		[PXHidden]
		public PXSelect<CRContactClass, 
			Where<CRContactClass.classID, Equal<Current<CRContactClass.classID>>>>
			LeadClassCurrent;

        [PXViewName(Messages.Attributes)]
        public CSAttributeGroupList<CRContactClass, Contact> Mapping;

        [PXHidden]
		public PXSelect<CRSetup> 
			Setup;

		protected virtual void CRContactClass_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			var row = e.Row as CRContactClass;
			if (row == null) return;
			
            CRSetup s = Setup.Select();

			if (s != null && (s.DefaultLeadClassID == row.ClassID))
			{
				s.DefaultLeadClassID = null;
				Setup.Update(s);
			}

            if (s != null && (s.DefaultContactClassID == row.ClassID))
			{
                s.DefaultContactClassID = null;
				Setup.Update(s);
			}
		}

		protected virtual void CRContactClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRContactClass;
			if (row == null) return;

			Delete.SetEnabled(CanDelete(row));
			PXUIFieldAttribute.SetEnabled<CRContactClass.defaultOwnerWorkgroup>(sender, row, row.DefaultWorkgroupID == null);
		}

		protected virtual void CRContactClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var row = e.Row as CRContactClass;
			if (row == null) return;

			if (!CanDelete(row))
			{
				throw new PXException(Messages.RecordIsReferenced);
			}
		}

		private bool CanDelete(CRContactClass row)
		{
			if (row != null)
			{
				Contact c = PXSelect<Contact,
					Where<Contact.classID, Equal<Required<CRContactClass.classID>>>>.
					SelectWindowed(this, 0, 1, row.ClassID);
				if (c != null)
				{
					return false;
				}
			}
			return true;
		}
	}
}
