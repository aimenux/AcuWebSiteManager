using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    public class RoomMaint : PXGraph<RoomMaint, FSRoom>
    {
        #region Selects
        public PXSelect<FSRoom,
               Where<
                   FSRoom.branchLocationID, Equal<Optional<FSRoom.branchLocationID>>>> RoomRecords;
        #endregion

        #region Cache Attached
        #region FSRoom_BranchLocationID
        [PXDefault]
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Branch Location ID")]
        [PXSelector(typeof(FSBranchLocation.branchLocationID), SubstituteKey = typeof(FSBranchLocation.branchLocationCD), DescriptionField = typeof(FSBranchLocation.descr))]
        protected virtual void FSRoom_BranchLocationID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSRoom_RoomID
        [PXDefault]
        [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">AAAAAAAAAA")]
        [PXUIField(DisplayName = "Room ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FSRoom.roomID, Where<FSRoom.branchLocationID, Equal<Current<FSRoom.branchLocationID>>>>))]
        protected virtual void FSRoom_RoomID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
    }
}