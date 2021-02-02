using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Common;
using PX.SM;
using PX.Objects.SO;

namespace PX.Objects.IN
{
	public class WarehouseProductivityEnq : PXGraph<WarehouseProductivityEnq>
	{
		public PXFilter<EfficiencyFilter> Filter;

		public SelectFrom<PickingEfficiency>.View Efficiency;
		public IEnumerable efficiency()
		{
			var filter = Filter.Current;

			var shipmentsByUser =
				SelectFrom<SOShipmentProcessedByUser>.
				InnerJoin<Users>.On<SOShipmentProcessedByUser.FK.User>.
				Where<SOShipmentProcessedByUser.endDateTime.IsNotNull.
				And<SOShipmentProcessedByUser.startDateTime.IsGreaterEqual<EfficiencyFilter.startDate.FromCurrent>>.
				And<SOShipmentProcessedByUser.startDateTime.IsLessEqual<EfficiencyFilter.endDate.FromCurrent>.
					Or<EfficiencyFilter.endDate.FromCurrent.IsNull>>.
				And<SOShipmentProcessedByUser.userID.IsEqual<EfficiencyFilter.userID.FromCurrent>.
					Or<EfficiencyFilter.userID.FromCurrent.IsNull>>.
				And<SOShipmentProcessedByUser.shipmentNbr.IsEqual<EfficiencyFilter.shipmentNbr.FromCurrent>.
					Or<EfficiencyFilter.shipmentNbr.FromCurrent.IsNull>>>.
				OrderBy<SOShipmentProcessedByUser.startDateTime.Desc, Users.username.Asc, SOShipmentProcessedByUser.shipmentNbr.Asc>.
				View.Select(this);

			var result = new List<PickingEfficiency>();
			PickingEfficiency prevRow = null;
			var distinctItems = new HashSet<int?>();
			foreach (SOShipmentProcessedByUser shipByUser in shipmentsByUser)
			{
				bool isNewLine = false;
				if (prevRow == null 
					|| filter.ExpandByUser == true && prevRow.UserID != shipByUser.UserID 
					|| filter.ExpandByShipment == true && prevRow.ShipmentNbr != shipByUser.ShipmentNbr 
					|| filter.ExpandByDay == true && prevRow.StartDate.Value.Date != shipByUser.StartDateTime.Value.Date)
				{
					prevRow = new PickingEfficiency
					{
						ShipmentNbr = shipByUser.ShipmentNbr,
						UserID = shipByUser.UserID,
						StartDate = shipByUser.StartDateTime.Value.Date,
						EndDate = shipByUser.EndDateTime.Value.Date,
						AmountOfShipments = 0,
						AmountOfLines = 0,
						AmountOfInventories = 0,
						AmountOfPackages = 0,
						TotalSeconds = 0,
						TotalQty = 0,
						QtyOfUsefulOperations = 0
					};
					distinctItems = new HashSet<int?>();
					isNewLine = true;
				}

				var shipmentLines =
					SelectFrom<SOShipLine>.
					Where<SOShipLine.shipmentNbr.IsEqual<@P.AsString>>.
					View.Select(this, shipByUser.ShipmentNbr).RowCast<SOShipLine>().ToArray();

				int packagesCount =
					SelectFrom<SOPackageDetailEx>.
					Where<SOPackageDetailEx.shipmentNbr.IsEqual<@P.AsString>>.
					View.Select(this, shipByUser.ShipmentNbr).Count;

				decimal totalQty = shipmentLines.Sum(line => line.Qty ?? 0);

				distinctItems.AddRange(shipmentLines.Where(line => line.InventoryID != null).Select(line => line.InventoryID));

				prevRow.StartDate = Tools.Min(prevRow.StartDate.Value.Date, shipByUser.StartDateTime.Value.Date);
				prevRow.EndDate = Tools.Max(prevRow.EndDate.Value.Date, shipByUser.EndDateTime.Value.Date);

				prevRow.AmountOfShipments++;
				prevRow.AmountOfLines += shipmentLines.Length;
				prevRow.AmountOfPackages += packagesCount;
				prevRow.AmountOfInventories = distinctItems.Count;
				prevRow.TotalQty += totalQty;
				prevRow.TotalSeconds += (decimal)(shipByUser.EndDateTime - shipByUser.StartDateTime).Value.TotalSeconds;
				prevRow.QtyOfUsefulOperations += 2 + 3 * packagesCount + totalQty;

				if (isNewLine)
					result.Add(prevRow);
			}

			return result.OrderByDescending(r => r.Day).ThenByDescending(r => r.Efficiency).ToList();
		}

		protected virtual void _(Events.RowSelected<EfficiencyFilter> e)
		{
			if (e.Row == null) return;
			Efficiency.Cache.Adjust<PXUIFieldAttribute>()
				.For<PickingEfficiency.day>(a => a.Visible = e.Row.ExpandByDay == true)
				.For<PickingEfficiency.startDate>(a => a.Visible = e.Row.ExpandByDay == false)
				.For<PickingEfficiency.endDate>(a => a.Visible = e.Row.ExpandByDay == false)
				.For<PickingEfficiency.userID>(a => a.Visible = e.Row.ExpandByUser == true)
				.For<PickingEfficiency.amountOfShipments>(a => a.Visible = e.Row.ExpandByShipment == false)
				.For<PickingEfficiency.shipmentNbr>(a => a.Visible = e.Row.ExpandByShipment == true);
		}
	}

	[PXHidden]
	public class EfficiencyFilter : IBqlTable
	{
		#region StartDate
		[PXDBDate]
		[PXUIField(DisplayName = "Period From")]
		public virtual DateTime? StartDate { get; set; }
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		#endregion
		#region EndDate
		[PXDBDate]
		[PXUIField(DisplayName = "Period To")]
		public virtual DateTime? EndDate { get; set; }
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		#endregion
		#region ExpandByUser
		[PXBool]
		[PXUIField(DisplayName = "Expand by User")]
		public bool? ExpandByUser { get; set; }
		public abstract class expandByUser : PX.Data.BQL.BqlBool.Field<expandByUser> { }
		#endregion
		#region UserID
		[PXGuid]
		[PXUIField(DisplayName = "User")]
		[PXUIVisible(typeof(expandByUser))]
		[PXFormula(typeof(Switch<Case<Where<expandByUser.IsEqual<False>>, Null>, userID>))]
		[PXSelector(typeof(Search<Users.pKID, Where<Users.isHidden, Equal<False>>>), SubstituteKey = typeof(Users.username))]
		public virtual Guid? UserID { get; set; }
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
		#endregion
		#region ExpandByShipment
		[PXBool]
		[PXUIField(DisplayName = "Expand by Shipment")]
		public bool? ExpandByShipment { get; set; }
		public abstract class expandByShipment : PX.Data.BQL.BqlBool.Field<expandByShipment> { }
		#endregion
		#region ShipmentNbr
		[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIVisible(typeof(expandByShipment))]
		[PXFormula(typeof(Switch<Case<Where<expandByShipment.IsEqual<False>>, Null>, shipmentNbr>))]
		[PXUIField(DisplayName = "Shipment Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(SearchFor<SOShipment.shipmentNbr>.In<SelectFrom<SOShipment>.Where<SOShipment.status.IsEqual<SOShipmentStatus.confirmed>>>))]
		public virtual String ShipmentNbr { get; set; }
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		#endregion
		#region ExpandByDay
		[PXBool]
		[PXUIField(DisplayName = "Expand by Day")]
		public bool? ExpandByDay { get; set; }
		public abstract class expandByDay : PX.Data.BQL.BqlBool.Field<expandByDay> { }
		#endregion
	}

	[PXHidden]
	public class PickingEfficiency : IBqlTable
	{
		#region StartDate
		[PXDate]
		[PXUIField(DisplayName = "Period From", Enabled = false)]
		public virtual DateTime? StartDate { get; set; }
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		#endregion
		#region EndDate
		[PXDate]
		[PXUIField(DisplayName = "Period To", Enabled = false)]
		public virtual DateTime? EndDate { get; set; }
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		#endregion
		#region Day
		[PXDate]
		[PXUIField(DisplayName = "Day", Enabled = false)]
		public virtual DateTime? Day { get => StartDate; set { } }
		public abstract class day : PX.Data.BQL.BqlDateTime.Field<day> { }
		#endregion
		#region ShipmentNbr
		[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Shipment Nbr.", Enabled = false)]
		public virtual String ShipmentNbr { get; set; }
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		#endregion
		#region UserID
		[PXGuid]
		[PXUIField(DisplayName = "User", Enabled = false)]
		[PXSelector(typeof(Search<Users.pKID, Where<Users.isHidden, Equal<False>>>), SubstituteKey = typeof(Users.username))]
		public virtual Guid? UserID { get; set; }
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
		#endregion
		#region QtyOfUsefulOperations
		[PXDecimal]
		[PXUIField(DisplayName = "Number of Effective Operations", Enabled = false)]
		public virtual Decimal? QtyOfUsefulOperations { get; set; }
		public abstract class qtyOfUsefulOperations : PX.Data.BQL.BqlDecimal.Field<qtyOfUsefulOperations> { }
		#endregion
		#region AmountOfShipments
		[PXDecimal]
		[PXUIField(DisplayName = "Number of Shipments", Enabled = false)]
		public virtual Decimal? AmountOfShipments { get; set; }
		public abstract class amountOfShipments : PX.Data.BQL.BqlDecimal.Field<amountOfShipments> { }
		#endregion
		#region AmountOfLines
		[PXDecimal]
		[PXUIField(DisplayName = "Number of Lines", Enabled = false)]
		public virtual Decimal? AmountOfLines { get; set; }
		public abstract class amountOfLines : PX.Data.BQL.BqlDecimal.Field<amountOfLines> { }
		#endregion
		#region AmountOfInventories
		[PXDecimal]
		[PXUIField(DisplayName = "Number of Unique Items", Enabled = false)]
		public virtual Decimal? AmountOfInventories { get; set; }
		public abstract class amountOfInventories : PX.Data.BQL.BqlDecimal.Field<amountOfInventories> { }
		#endregion
		#region AmountOfPackages
		[PXDecimal]
		[PXUIField(DisplayName = "Number of Packages", Enabled = false)]
		public virtual Decimal? AmountOfPackages { get; set; }
		public abstract class amountOfPackages : PX.Data.BQL.BqlDecimal.Field<amountOfPackages> { }
		#endregion
		#region TotalQty
		[PXDecimal]
		[PXUIField(DisplayName = "Total Qty.", Enabled = false)]
		public virtual Decimal? TotalQty { get; set; }
		public abstract class totalQty : PX.Data.BQL.BqlDecimal.Field<totalQty> { }
		#endregion
		#region TotalSeconds
		[PXDecimal]
		public virtual Decimal? TotalSeconds { get; set; }
		public abstract class totalSeconds : PX.Data.BQL.BqlDecimal.Field<totalSeconds> { }
		#endregion
		#region Efficiency
		[PXDecimal]
		[PXUIField(DisplayName = "Efficiency (Eff. Op. per Minute)", Enabled = false)]
		public virtual Decimal? Efficiency { get => QtyOfUsefulOperations / (TotalSeconds / 60); set { } }
		public abstract class efficiency : PX.Data.BQL.BqlDecimal.Field<efficiency> { }
		#endregion
		#region TotalTime
		[PXString]
		[PXUIField(DisplayName = "Total Time", Enabled = false)]
		public virtual String TotalTime { get => $"{(int)(TotalSeconds/3600)}:{TotalSeconds/60 % 60:00}:{TotalSeconds % 60:00}"; set { } }
		public abstract class totalTime : PX.Data.BQL.BqlDecimal.Field<totalTime> { }
		#endregion
	}
}