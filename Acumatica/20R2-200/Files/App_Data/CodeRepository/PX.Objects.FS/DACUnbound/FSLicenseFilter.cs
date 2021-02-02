using PX.Data;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class FSLicenseFilter : PX.Data.IBqlTable
    {
		#region OwnerType
        public abstract class ownerType : ListField_OwnerType
		{            
		}

        [PXString(1, IsFixed = true, IsUnicode = true)]
        [ownerType.ListAtrribute]
        [PXDefault(ID.OwnerType.BUSINESS)]
        [PXUIField(DisplayName = "Owner Type")]
        public virtual string OwnerType { get; set; }
		#endregion
        #region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXInt]
        [PXUIField(DisplayName = "Staff Member Name")]
        [FSSelector_StaffMember_All]
        public virtual int? EmployeeID { get; set; }
        #endregion
    }
}
