using PX.Data;

namespace PX.Objects.CR
{
	/// <exclude/>
	public class CRContactClassMaint : PXGraph<CRContactClassMaint, CRContactClass>
	{
		[PXViewName(Messages.LeadClass)]
		public PXSelect<CRContactClass> 
			Class;

		[PXHidden]
		public PXSelect<CRContactClass, 
			Where<CRContactClass.classID, Equal<Current<CRContactClass.classID>>>>
			ClassCurrent;

        [PXViewName(Messages.Attributes)]
        public CSAttributeGroupList<CRContactClass, Contact> 
			Mapping;

        [PXHidden]
		public PXSelect<CRSetup> 
			Setup;

		protected virtual void _(Events.RowDeleted<CRContactClass> e)
		{
			var row = e.Row;
			if (row == null) return;
			
            CRSetup s = Setup.Select();
			
            if (s != null && (s.DefaultContactClassID == row.ClassID))
			{
                s.DefaultContactClassID = null;
				Setup.Update(s);
			}
		}

		protected virtual void _(Events.FieldUpdated<CRContactClass, CRContactClass.defaultOwner> e)
		{
			var row = e.Row;
			if (row == null || e.NewValue == e.OldValue)
				return;

			row.DefaultAssignmentMapID = null;
		}

		protected virtual void _(Events.RowSelected<CRContactClass> e)
		{
			var row = e.Row;
			if (row == null) return;

			Delete.SetEnabled(CanDelete(row));
		}

		protected virtual void _(Events.RowDeleting<CRContactClass> e)
		{
			var row = e.Row;
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
