using PX.Data;

namespace PX.Objects.CR
{
	/// <exclude/>
	public class CRLeadClassMaint : PXGraph<CRLeadClassMaint, CRLeadClass>
	{
		[PXViewName(Messages.LeadClass)]
		public PXSelect<CRLeadClass> 
			Class;

		[PXHidden]
		public PXSelect<CRLeadClass, 
			Where<CRLeadClass.classID, Equal<Current<CRLeadClass.classID>>>>
			ClassCurrent;

        [PXViewName(Messages.Attributes)]
        public CSAttributeGroupList<CRLeadClass, CRLead> 
			Mapping;

        [PXHidden]
		public PXSelect<CRSetup> 
			Setup;

		protected virtual void _(Events.RowDeleted<CRLeadClass> e)
		{
			var row = e.Row;
			if (row == null) return;
			
            CRSetup s = Setup.Select();

			if (s != null && (s.DefaultLeadClassID == row.ClassID))
			{
				s.DefaultLeadClassID = null;
				Setup.Update(s);
			}
		}

		protected virtual void _(Events.FieldUpdated<CRLeadClass, CRLeadClass.defaultOwner> e)
		{
			var row = e.Row;
			if (row == null || e.NewValue == e.OldValue)
				return;

			row.DefaultAssignmentMapID = null;
		}

		protected virtual void _(Events.RowSelected<CRLeadClass> e)
		{
			var row = e.Row;
			if (row == null) return;

			Delete.SetEnabled(CanDelete(row));
		}

		protected virtual void _(Events.RowDeleting<CRLeadClass> e)
		{
			var row = e.Row;
			if (row == null) return;

			if (!CanDelete(row))
			{
				throw new PXException(Messages.RecordIsReferenced);
			}
		}

		private bool CanDelete(CRLeadClass row)
		{
			if (row != null)
			{
				Contact c = PXSelect<Contact,
					Where<Contact.classID, Equal<Required<CRLeadClass.classID>>>>.
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
