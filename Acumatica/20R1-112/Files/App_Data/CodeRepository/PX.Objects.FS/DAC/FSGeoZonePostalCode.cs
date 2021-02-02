using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using System;
using static PX.Data.PXAccess;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.GEOGRAPHIC_POSTAL_CODE)]
    public class FSGeoZonePostalCode : PX.Data.IBqlTable
    {
        #region GeoZoneID
        public abstract class geoZoneID : PX.Data.BQL.BqlInt.Field<geoZoneID> { }

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Service Area", Enabled = false, Visible = false)]
        [PXSelector(typeof(Search<FSGeoZone.geoZoneID>), SubstituteKey = typeof(FSGeoZone.geoZoneCD), DescriptionField = typeof(FSGeoZone.descr))]
        [PXDBLiteDefault(typeof(FSGeoZone.geoZoneID))]
        [PXParent(typeof(Select<FSGeoZone, Where<FSGeoZone.geoZoneID, Equal<Current<FSGeoZonePostalCode.geoZoneID>>>>))]
        public virtual int? GeoZoneID { get; set; }
        #endregion
        #region PostalCode
        public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }

        [PXDBString(25, IsKey = true)]
        [PXDefault]
        [NormalizeWhiteSpace]
        [PXUIField(DisplayName = "Postal Code")]
        [PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), typeof(Address.countryID))]
        [PXDynamicMask(typeof(Search<Country.zipCodeMask, Where<Country.countryID, Equal<Current<FSGeoZone.countryID>>>>))]
        public virtual string PostalCode { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "Created By Screen ID")]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
        [PXUIField(DisplayName = "Last Modified By Screen ID")]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion
    }
}
