using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CR;

namespace PX.Objects.FS
{	
    [System.SerializableAttribute]
    [PXCacheName(TX.TableName.LICENSE_TYPE)]
    [PXPrimaryGraph(typeof(LicenseTypeMaint))]
	public class FSLicenseType : PX.Data.IBqlTable
	{
		#region LicenseTypeID
		public abstract class licenseTypeID : PX.Data.BQL.BqlInt.Field<licenseTypeID> { }

		[PXDBIdentity]
		[PXUIField(Enabled = false)]
		public virtual int? LicenseTypeID { get; set; }
		#endregion
		#region LicenseTypeCD
		public abstract class licenseTypeCD : PX.Data.BQL.BqlString.Field<licenseTypeCD> { }

        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", IsFixed = true)]
        [PXUIField(DisplayName = "License Type ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FSLicenseType.licenseTypeCD>))]
        [PXDefault]
        [NormalizeWhiteSpace]
		public virtual string LicenseTypeCD { get; set; }
		#endregion
        #region City
        public abstract class city : PX.Data.BQL.BqlString.Field<city> { }

        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "City")]
        public virtual string City { get; set; }
        #endregion
        #region CountryID
        public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }

        [PXDBString(2, IsUnicode = true)]
        [PXUIField(DisplayName = "Country")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [Country]
        public virtual string CountryID { get; set; }
        #endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        [PXDBString(60, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Descr { get; set; }
		#endregion
        #region GeoZoneID
        public abstract class geoZoneID : PX.Data.BQL.BqlInt.Field<geoZoneID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Service Area")]
        [PXSelector(typeof(FSGeoZone.geoZoneID), SubstituteKey = typeof(FSGeoZone.geoZoneCD), DescriptionField = typeof(FSGeoZone.descr))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? GeoZoneID { get; set; }
        #endregion
        #region OwnerType
        public abstract class ownerType : ListField_OwnerType
        {
        }

        [PXDBString(1, IsFixed = true, IsUnicode = true)]
        [ownerType.ListAtrribute]
        [PXDefault(ID.OwnerType.BUSINESS)]
        [PXUIField(DisplayName = "Owner Type", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string OwnerType { get; set; }
        #endregion
        #region State
        public abstract class state : PX.Data.BQL.BqlString.Field<state> { }

        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "State")]
        [FSSelectorState]        
        public virtual string State { get; set; }
        #endregion
        #region ValidIn
        public abstract class validIn : PX.Data.BQL.BqlString.Field<validIn> { }

        [PXDBString(3, IsFixed = true)]
        [PXDefault("ALL")]
        [PXUIField(DisplayName = "Validation Settings")]
        public virtual string ValidIn { get; set; }
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
		[PXUIField(DisplayName = "Created By")]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		[PXUIField(DisplayName = "CreatedByScreenID")]
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
		[PXUIField(DisplayName = "Last Modified By")]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		[PXUIField(DisplayName = "LastModifiedByScreenID")]
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