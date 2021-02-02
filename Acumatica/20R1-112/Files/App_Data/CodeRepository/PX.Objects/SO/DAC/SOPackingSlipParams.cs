using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.IN;
using System;

namespace PX.Objects.SO
{
	[PXCacheName(Messages.SOPackingSlipParams)]
	public class SOPackingSlipParams : IBqlTable
	{
		#region BatchWorksheetNbr
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Worksheet Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(
			SearchFor<SOPickingWorksheet.worksheetNbr>.In<
				SelectFrom<SOPickingWorksheet>.
				InnerJoin<INSite>.On<SOPickingWorksheet.FK.Site>.
				Where<
					Match<INSite, AccessInfo.userName.FromCurrent>.
					And<SOPickingWorksheet.worksheetType.IsEqual<SOPickingWorksheet.worksheetType.batch>>>.
				OrderBy<SOPickingWorksheet.worksheetNbr.Desc>
			>))]
		public virtual String BatchWorksheetNbr { get; set; }
		public abstract class batchWorksheetNbr : PX.Data.BQL.BqlString.Field<batchWorksheetNbr> { }
		#endregion
		#region BatchShipmentNbr
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = SOShipment.shipmentNbr.DisplayName, Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(
			SearchFor<SOShipment.shipmentNbr>.In<
				SelectFrom<SOShipment>.
				InnerJoin<INSite>.On<SOShipment.FK.Site>.
				LeftJoin<Customer>.On<SOShipment.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly.
				Where<
					Match<INSite, AccessInfo.userName.FromCurrent>.
					And<batchWorksheetNbr.FromCurrent.NoDefault.IsNull.
						Or<SOShipment.currentWorksheetNbr.IsEqual<batchWorksheetNbr.FromCurrent.NoDefault>>>.
					And<Customer.bAccountID.IsNull.
						Or<Match<Customer, AccessInfo.userName.FromCurrent>>>>.
				OrderBy<SOShipment.shipmentNbr.Desc>>))]
		public virtual String BatchShipmentNbr { get; set; }
		public abstract class batchShipmentNbr : PX.Data.BQL.BqlString.Field<batchShipmentNbr> { }
		#endregion

		#region WaveWorksheetNbr
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Worksheet Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(
			SearchFor<SOPickingWorksheet.worksheetNbr>.In<
				SelectFrom<SOPickingWorksheet>.
				InnerJoin<INSite>.On<SOPickingWorksheet.FK.Site>.
				Where<
					Match<INSite, AccessInfo.userName.FromCurrent>.
					And<SOPickingWorksheet.worksheetType.IsEqual<SOPickingWorksheet.worksheetType.wave>>>.
				OrderBy<SOPickingWorksheet.worksheetNbr.Desc>
			>))]
		public virtual String WaveWorksheetNbr { get; set; }
		public abstract class waveWorksheetNbr : PX.Data.BQL.BqlString.Field<waveWorksheetNbr> { }
		#endregion
		#region WaveShipmentNbr
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = SOShipment.shipmentNbr.DisplayName, Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(
			SearchFor<SOShipment.shipmentNbr>.In<
				SelectFrom<SOShipment>.
				InnerJoin<INSite>.On<SOShipment.FK.Site>.
				LeftJoin<Customer>.On<SOShipment.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly.
				Where<
					Match<INSite, AccessInfo.userName.FromCurrent>.
					And<waveWorksheetNbr.FromCurrent.NoDefault.IsNull.
						Or<SOShipment.currentWorksheetNbr.IsEqual<waveWorksheetNbr.FromCurrent.NoDefault>>>.
					And<Customer.bAccountID.IsNull.
						Or<Match<Customer, AccessInfo.userName.FromCurrent>>>>.
				OrderBy<SOShipment.shipmentNbr.Desc>>))]
		public virtual String WaveShipmentNbr { get; set; }
		public abstract class waveShipmentNbr : PX.Data.BQL.BqlString.Field<waveShipmentNbr> { }
		#endregion
	}
}