using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    [PXCacheName(TX.TableName.MANUFACTURER)]
    [PXPrimaryGraph(typeof(ManufacturerMaint))]
    public class FSManufacturer : PX.Data.IBqlTable
    {
        #region ManufacturerID
        public abstract class manufacturerID : PX.Data.BQL.BqlInt.Field<manufacturerID> { }

        [PXDBIdentity]
        [PXUIField(Enabled = false)]
        public virtual int? ManufacturerID { get; set; }
        #endregion
        #region ManufacturerCD
        public abstract class manufacturerCD : PX.Data.BQL.BqlString.Field<manufacturerCD> { }

        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", IsFixed = true)]
        [PXDefault]
        [NormalizeWhiteSpace]
        [PXUIField(DisplayName = "Manufacturer ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(FSManufacturer.manufacturerCD))]
        public virtual string ManufacturerCD { get; set; }
        #endregion

        #region ManufacturerAddressID
        public abstract class manufacturerAddressID : PX.Data.IBqlField
        {
        }
        protected Int32? _ManufacturerAddressID;
        [PXDBInt]
        [FSDocumentAddress(typeof(Select<Address,
             Where<True, Equal<False>>>))]
        public virtual Int32? ManufacturerAddressID
        {
            get
            {
                return this._ManufacturerAddressID;
            }
            set
            {
                this._ManufacturerAddressID = value;
            }
        }
        #endregion
        #region ManufacturerContactID
        public abstract class manufacturerContactID : PX.Data.IBqlField
        {
        }
        protected Int32? _ManufacturerContactID;
        [PXDBInt]
        [FSDocumentContact(typeof(Select<Contact,
             Where<True, Equal<False>>>))]
        public virtual Int32? ManufacturerContactID
        {
            get
            {

                return this._ManufacturerContactID;
            }
            set
            {
                this._ManufacturerContactID = value;
            }
        }
        #endregion  
        #region AllowOverrideContactAddress
        public abstract class allowOverrideContactAddress : PX.Data.IBqlField
        {
        }
        protected Boolean? _AllowOverrideContactAddress;
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? AllowOverrideContactAddress
        {
            get
            {
                return this._AllowOverrideContactAddress;
            }
            set
            {
                this._AllowOverrideContactAddress = value;
            }
        }
        #endregion

        #region LocationID
        public abstract class locationID : PX.Data.IBqlField { }
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXInt]
        public virtual Int32? LocationID { get; set; }
        #endregion

        #region ContactID
        public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Contact")]
        [PXSelector(typeof(Search2<Contact.contactID,
                            InnerJoin<BAccount,
                                On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
                             Where<
                                Contact.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>,
                            And<
                                Where<
                                    BAccount.type, Equal<BAccountType.customerType>,
                                    Or<BAccount.type, Equal<BAccountType.prospectType>,
                                    Or<BAccount.type, Equal<BAccountType.combinedType>,
                                    Or<BAccount.type, Equal<BAccountType.vendorType>>>>>>>>), 
                            new Type[]
                            {
                                typeof(Contact.displayName),
                                typeof(Contact.salutation),
                                typeof(Contact.fullName),
                                typeof(Contact.eMail),
                                typeof(Contact.phone1),
                                typeof(BAccount.type)
                            },
                            DescriptionField = typeof(Contact.displayName))]
        public virtual int? ContactID { get; set; }
        #endregion

        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        [PXDBLocalizableString(60, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Descr { get; set; }
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

        #region Memory Helper
        #region ManufacturerGICD
        public abstract class manufacturerGICD : PX.Data.BQL.BqlString.Field<manufacturerGICD>
		{
        }

        [PXString]
        [PXUIField(DisplayName = "Manufacturer")]
        public virtual string ManufacturerGICD { get; set; }
        #endregion
        #endregion
    }
}