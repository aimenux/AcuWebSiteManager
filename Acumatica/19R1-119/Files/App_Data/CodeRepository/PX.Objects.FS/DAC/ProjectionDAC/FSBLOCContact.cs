using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXBreakInheritance]
    [PXProjection(typeof(
        Select<FSContact,
                Where<FSContact.entityType, Equal<FSContact.entityType.BranchLocation>>>))]
    public partial class FSBLOCContact : FSContact
    {
        #region ContactID
        public new abstract class contactID : PX.Data.IBqlField
        {
        }
        #endregion

        #region EntityType
        public new abstract class entityType : ListField.ACEntityType
        {
        }

        [PXDBString(4, IsFixed = true)]
        [PXDefault(ID.ACEntityType.BRANCH_LOCATION)]
        [PXUIField(DisplayName = "Entity Type", Visible = false, Enabled = false)]
        public override string EntityType { get; set; }
        #endregion

        #region RevisionID
        public new abstract class revisionID : PX.Data.IBqlField
        {
        }
        #endregion
    }
}