using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.CS;
using System;

namespace PX.Objects.FS
{
    [Serializable()]
    [PXCacheName(TX.TableName.FSADDRESS)]
    public partial class FSAddress : PX.Data.IBqlTable, IAddress, IAddressBase
    {
        #region AddressID
        public abstract class addressID : PX.Data.IBqlField
        {
        }
        protected Int32? _AddressID;
        [PXDBIdentity(IsKey = true)]
        [PXUIField(DisplayName = "Address ID", Visible = false)]
        public virtual Int32? AddressID
        {
            get
            {
                return this._AddressID;
            }
            set
            {
                this._AddressID = value;
            }
        }
        #endregion

        #region EntityType
        public abstract class entityType : ListField.ACEntityType
        {
        }

        [PXDBString(4, IsFixed = true)]
        [PXDefault(ID.ACEntityType.SERVICE_ORDER)]
        [PXUIField(DisplayName = "Entity Type", Visible = false, Enabled = false)]
        public virtual string EntityType { get; set; }
        #endregion

        #region BAccountID
        public abstract class bAccountID : PX.Data.IBqlField
        {
        }
        protected Int32? _BAccountID;
        [PXDBInt()]
        [PXDBDefault(typeof(FSServiceOrder.billCustomerID), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Int32? BAccountID
        {
            get
            {
                return this._BAccountID;
            }
            set
            {
                this._BAccountID = value;
            }
        }

        #endregion
        #region BAccountAddressID
        public abstract class bAccountAddressID : PX.Data.IBqlField
        {
        }
        protected Int32? _BAccountAddressID;
        [PXDBInt()]
        public virtual Int32? BAccountAddressID
        {
            get
            {
                return this._BAccountAddressID;
            }
            set
            {
                this._BAccountAddressID = value;
            }
        }
        
        #endregion
        #region IsDefaultAddress
        public abstract class isDefaultAddress : PX.Data.IBqlField
        {
        }
        protected Boolean? _IsDefaultAddress;
        [PXDBBool()]
        [PXUIField(DisplayName = "Default Customer Address", Visibility = PXUIVisibility.Visible)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? IsDefaultAddress
        {
            get
            {
                return this._IsDefaultAddress;
            }
            set
            {
                this._IsDefaultAddress = value;
            }
        }
        #endregion
        #region OverrideAddress
        public abstract class overrideAddress : PX.Data.IBqlField
        {
        }
        [PXBool()]
        [PXUIField(DisplayName = "Override Address", Visibility = PXUIVisibility.Visible)]
        public virtual Boolean? OverrideAddress
        {
            [PXDependsOnFields(typeof(isDefaultAddress))]
            get
            {
                return (bool?)(this._IsDefaultAddress == null ? this._IsDefaultAddress : this._IsDefaultAddress == false);
            }
            set
            {
                this._IsDefaultAddress = (bool?)(value == null ? value : value == false);
            }
        }
        #endregion
        #region RevisionID
        public abstract class revisionID : PX.Data.IBqlField
        {
        }
        protected Int32? _RevisionID;
        [PXDBInt()]        
        public virtual Int32? RevisionID
        {
            get
            {
                return this._RevisionID;
            }
            set
            {
                this._RevisionID = value;
            }
        }
        #endregion
        #region AddressLine1
        public abstract class addressLine1 : PX.Data.IBqlField
        {
        }
        protected String _AddressLine1;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Address Line 1", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		public virtual String AddressLine1
        {
            get
            {
                return this._AddressLine1;
            }
            set
            {
                this._AddressLine1 = value;
            }
        }
        #endregion
        #region AddressLine2
        public abstract class addressLine2 : PX.Data.IBqlField
        {
        }
        protected String _AddressLine2;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Address Line 2")]
		[PXPersonalDataField]
		public virtual String AddressLine2
        {
            get
            {
                return this._AddressLine2;
            }
            set
            {
                this._AddressLine2 = value;
            }
        }
        #endregion
        #region AddressLine3
        public abstract class addressLine3 : PX.Data.IBqlField
        {
        }
        protected String _AddressLine3;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Address Line 3")]
		[PXPersonalDataField]
		public virtual String AddressLine3
        {
            get
            {
                return this._AddressLine3;
            }
            set
            {
                this._AddressLine3 = value;
            }
        }
        #endregion
        #region City
        public abstract class city : PX.Data.IBqlField
        {
        }
        protected String _City;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "City", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		public virtual String City
        {
            get
            {
                return this._City;
            }
            set
            {
                this._City = value;
            }
        }
        #endregion
        #region CountryID
        public abstract class countryID : PX.Data.IBqlField
        {
        }
        protected String _CountryID;
        [PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(100)]
        [PXUIField(DisplayName = "Country")]
        [CR.Country]
		public virtual String CountryID
        {
            get
            {
                return this._CountryID;
            }
            set
            {
                this._CountryID = value;
            }
        }
        #endregion
        #region State
        public abstract class state : PX.Data.IBqlField
        {
        }
        protected String _State;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "State")]
        [CR.State(typeof(countryID))]
        public virtual String State
        {
            get
            {
                return this._State;
            }
            set
            {
                this._State = value;
            }
        }
        #endregion
        #region PostalCode
        public abstract class postalCode : PX.Data.IBqlField
        {
        }
        protected String _PostalCode;
        [PXDBString(20)]
        [PXUIField(DisplayName = "Postal Code")]
        [PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(countryID))]
		[PXPersonalDataField]
		public virtual String PostalCode
        {
            get
            {
                return this._PostalCode;
            }
            set
            {
                this._PostalCode = value;
            }
        }
        #endregion
		#region NoteID
		public abstract class noteID : IBqlField { }

		[PXDBGuidNotNull]
		public virtual Guid? NoteID { get; set; }
		#endregion
        
        #region IsValidated
        public abstract class isValidated : PX.Data.IBqlField
        {
        }
        protected Boolean? _IsValidated;

        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBBool()]
        [PXUIField(DisplayName = "Validated", FieldClass = CS.Messages.ValidateAddress)]
        public virtual Boolean? IsValidated
        {
            get
            {
                return this._IsValidated;
            }
            set
            {
                this._IsValidated = value;
            }
        }
        #endregion
        
        #region tstamp
        public abstract class Tstamp : PX.Data.IBqlField
        {
        }
        protected Byte[] _tstamp;
        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.IBqlField
        {
        }
        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.IBqlField
        {
        }
        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.IBqlField
        {
        }
        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.IBqlField
        {
        }
        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.IBqlField
        {
        }
        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.IBqlField
        {
        }
        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
    }

}
