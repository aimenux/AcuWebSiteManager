using System;
using PX.Data;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    [PXCacheName(TX.TableName.MANUFACTURER_MODEL)]
    [PXPrimaryGraph(typeof(ManufacturerModelMaint))]
    public class FSManufacturerModel : PX.Data.IBqlTable
    {
        #region ManufacturerID
        public abstract class manufacturerID : PX.Data.BQL.BqlInt.Field<manufacturerID> { }

		[PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Manufacturer ID")]
        [PXSelector(typeof(FSManufacturer.manufacturerID),
            SubstituteKey = typeof(FSManufacturer.manufacturerCD))]
        public virtual int? ManufacturerID { get; set; }
        #endregion
        #region ManufacturerModelID
        public abstract class manufacturerModelID : PX.Data.BQL.BqlInt.Field<manufacturerModelID> { }

		[PXDBIdentity]
        [PXUIField(Enabled = false)]
        public virtual int? ManufacturerModelID { get; set; }
        #endregion
        #region ManufacturerModelCD
        public abstract class manufacturerModelCD : PX.Data.BQL.BqlString.Field<manufacturerModelCD> { }

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault]
        [PXUIField(DisplayName = "Manufacturer Model", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FSManufacturerModel.manufacturerModelCD,
                            Where<
                                FSManufacturerModel.manufacturerID, Equal<Current<FSManufacturerModel.manufacturerID>>>>),
                           SubstituteKey = typeof(FSManufacturerModel.manufacturerModelCD))]
        public virtual string ManufacturerModelCD { get; set; }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        [PXDBLocalizableString(60, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Descr { get; set; }
        #endregion
        #region EquipmentTypeID
        public abstract class equipmentTypeID : PX.Data.BQL.BqlInt.Field<equipmentTypeID> { }

		[PXDBInt]
        [PXUIField(DisplayName = "Equipment Type")]
        [PXSelector(typeof(FSEquipmentType.equipmentTypeID),
            SubstituteKey = typeof(FSEquipmentType.equipmentTypeCD),
            DescriptionField = typeof(FSEquipmentType.descr))]
        public virtual int? EquipmentTypeID { get; set; }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote(new Type[0])]
        public virtual Guid? NoteID { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created On")]
        public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modified On")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
		#endregion
	}
}