using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXPrimaryGraph(typeof(AppointmentEntry))]
    [PXProjection(typeof(
        Select2<FSAppointment,
                InnerJoin<FSServiceOrder,
                    On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                InnerJoin<FSAddress,
                    On<FSAddress.addressID, Equal<FSServiceOrder.serviceOrderAddressID>>,
                LeftJoin<FSServiceContract, 
                    On<FSAppointment.serviceContractID, Equal<FSServiceContract.serviceContractID>>,
                CrossJoinSingleTable<FSSetup>>>>>))]
    public partial class FSAppointmentInRoute : FSAppointment
    {
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.customerID))]
        [PXUIField(DisplayName = "Customer ID")]
        [FSSelectorCustomer]
        public virtual int? CustomerID { get; set; }
        #endregion

        #region CustomerContractNbr
        public abstract class customerContractNbr : PX.Data.IBqlField
        {
        }

        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(FSServiceContract.customerContractNbr))]
        [PXUIField(DisplayName = "Customer Contract Nbr.")]
        public virtual string CustomerContractNbr { get; set; }
        #endregion

        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        [LocationID(BqlField = typeof(FSServiceOrder.locationID), DisplayName = "Location ID", DescriptionField = typeof(Location.descr))]
        public virtual int? LocationID { get; set; }
        #endregion

        #region State
        public abstract class state : PX.Data.BQL.BqlString.Field<state> { }

        [PXDBString(50, IsUnicode = true, BqlField = typeof(FSAddress.state))]
        [PXUIField(DisplayName = "State")]
        [State(typeof(FSAddress.countryID), DescriptionField = typeof(State.name))]
        public virtual string State { get; set; }
        #endregion

        #region AddressLine1
        public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }

        [PXDBString(50, IsUnicode = true, BqlField = typeof(FSAddress.addressLine1))]
        [PXUIField(DisplayName = "Address Line 1")]
        public virtual string AddressLine1 { get; set; }
        #endregion

        #region AddressLine2
        public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }

        [PXDBString(50, IsUnicode = true, BqlField = typeof(FSAddress.addressLine2))]
        [PXUIField(DisplayName = "Address Line 2")]
        public virtual string AddressLine2 { get; set; }
        #endregion

        #region PostalCode
        public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }

        [PXDBString(20, BqlField = typeof(FSAddress.postalCode))]
        [PXUIField(DisplayName = "Postal code")]
        [PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), typeof(FSAddress.countryID))]
        [PXDynamicMask(typeof(Search<Country.zipCodeMask, Where<Country.countryID, Equal<Current<FSAddress.countryID>>>>))]
        [PXFormula(typeof(Default<FSAddress.countryID>))]
        public virtual string PostalCode { get; set; }
        #endregion

        #region City
        public abstract class city : PX.Data.BQL.BqlString.Field<city> { }

        [PXDBString(50, IsUnicode = true, BqlField = typeof(FSAddress.city))]
        [PXUIField(DisplayName = "City")]
        public virtual string City { get; set; }
        #endregion

        #region MapApiKey
        public abstract class mapApiKey : PX.Data.BQL.BqlString.Field<mapApiKey> { }

        [PXDBString(255, IsUnicode = true, BqlField = typeof(FSSetup.mapApiKey))]
        [PXUIField(DisplayName = "Map API Key")]
        public virtual string MapApiKey { get; set; }
        #endregion

        #region ServiceContractID
        public new abstract class serviceContractID : PX.Data.IBqlField
        {
        }

        [PXDBInt(BqlField = typeof(FSServiceOrder.serviceContractID))]
        [PXSelector(typeof(Search<FSServiceContract.serviceContractID,
                           Where<
                                FSServiceContract.customerID, Equal<Current<FSAppointmentInRoute.customerID>>>>),
                           SubstituteKey = typeof(FSServiceContract.refNbr))]
        [PXUIField(DisplayName = "Source Service Contract ID", Enabled = false, FieldClass = "FSCONTRACT")]
        public override int? ServiceContractID { get; set; }
        #endregion
    }
}