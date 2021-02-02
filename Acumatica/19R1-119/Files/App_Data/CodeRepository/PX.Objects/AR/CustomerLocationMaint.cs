using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.Common.Discount;

namespace PX.Objects.AR
{
	[PXPrimaryGraph(typeof(CustomerLocationMaint))]
	[PXSubstitute(GraphType = typeof(CustomerLocationMaint))]
	[Serializable]
	public partial class SelectedCustomerLocation : SelectedLocation
	{
		#region BAccountID
        public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        [PXDefault(typeof(Location.bAccountID))]
        [Customer(typeof(Search<Customer.bAccountID,
            Where<Where<Customer.type, Equal<BAccountType.customerType>,
                    Or<Customer.type, Equal<BAccountType.prospectType>,
                     Or<Customer.type, Equal<BAccountType.combinedType>>>>>>), IsKey = true, TabOrder = 0)]
        [PXParent(typeof(Select<BAccount,
            Where<BAccount.bAccountID,
            Equal<Current<Location.bAccountID>>>>)
            )]
        public override Int32? BAccountID
        {
            get
            {
                return base._BAccountID;
            }
            set
            {
                base._BAccountID = value;
            }
        }
        #endregion

	}

	public class CustomerLocationMaint : LocationMaint
	{
		public CustomerLocationMaint()
		{
			Location.Join<LeftJoin<Customer, On<Customer.bAccountID, Equal<Location.bAccountID>>>>();

			Location.WhereAnd<Where<
				Customer.bAccountID, Equal<Location.bAccountID>,
				And<Customer.bAccountID, IsNotNull,
				And<Match<Customer, Current<AccessInfo.userName>>>>>>();
		}

		protected override void Location_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			base.Location_RowPersisted(sender, e);

			if (e.TranStatus == PXTranStatus.Completed)
				DiscountEngine.RemoveFromCachedCustomerPriceClasses(((Location)e.Row).BAccountID);
		}

		protected override void EstablishVTaxZoneRule(Action<Type, bool> setCheck)
		{
			setCheck(typeof(Location.vTaxZoneID), false);
		}
	}

}
