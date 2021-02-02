using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.TX;
using System;

namespace PX.Objects.FS
{
    [PXProjection(typeof(Select<FSTaxDetail,
                            Where<
                                FSTaxDetail.entityType, Equal<FSTaxDetail.entityType.Appointment>,
                                And<FSTaxDetail.lineNbr, Equal<intMax>>>>),
                    Persistent = true)]
    [Serializable]
    [PXBreakInheritance]
    public class FSAppointmentTaxTran : FSAppointmentTax
    {
        #region EntityType
        public new abstract class entityType : ListField_PostDoc_EntityType
        {
        }
        [PXDBString(2, IsKey = true, IsFixed = true)]
        [PXDefault(ID.PostDoc_EntityType.APPOINTMENT)]
        [entityType.ListAtrribute]
        [PXUIField(DisplayName = "EntityType", Visibility = PXUIVisibility.Visible, Visible = true)]
        public override String EntityType { get; set; }
        #endregion

        #region EntityID
        public new abstract class entityID : PX.Data.BQL.BqlInt.Field<entityID> { }

        [PXDBInt(IsKey = true)]
        [PXDBLiteDefault(typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Entity ID", Enabled = false, Visible = true)]
        public override int? EntityID { get; set; }
        #endregion

        #region LineNbr
        public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		[PXDBInt(IsKey = true)]
		[PXDefault(int.MaxValue)]
        [PXUIField(DisplayName = "LineNbr", Visibility = PXUIVisibility.Visible, Visible = false)]
        [PXParent(typeof(Select<FSAppointment,
                            Where<
                                FSAppointment.appointmentID, Equal<Current<FSAppointmentTaxTran.entityID>>,
                                And<FSAppointmentTaxTran.entityType.Appointment, Equal<Current<FSAppointmentTaxTran.entityType>>>>>))]
		public override Int32? LineNbr { get; set; }
        #endregion

        #region TaxID
        public new abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr))]
		public override String TaxID
		{
			get
			{
				return this._TaxID;
			}
			set
			{
				this._TaxID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong()]
		[CurrencyInfo(typeof(FSAppointment.curyInfoID))]
		public override Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region CuryTaxableAmt
		public new abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		[PXDBCurrency(typeof(FSAppointmentTaxTran.curyInfoID), typeof(FSAppointmentTaxTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
        [PXUnboundFormula(typeof(Switch<Case<WhereExempt<FSAppointmentTaxTran.taxID>, FSAppointmentTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<FSAppointment.curyVatExemptTotal>))]
        [PXUnboundFormula(typeof(Switch<Case<WhereTaxable<FSAppointmentTaxTran.taxID>, FSAppointmentTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<FSAppointment.curyVatTaxableTotal>))]        
		public override Decimal? CuryTaxableAmt
		{
			get
			{
				return this._CuryTaxableAmt;
			}
			set
			{
				this._CuryTaxableAmt = value;
			}
		}
		#endregion
		#region TaxableAmt
		public new abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }
		#endregion
		#region CuryTaxAmt
		public new abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		[PXDBCurrency(typeof(FSAppointmentTaxTran.curyInfoID), typeof(FSAppointmentTaxTran.taxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryTaxAmt
		{
			get
			{
				return this._CuryTaxAmt;
			}
			set
			{
				this._CuryTaxAmt = value;
			}
		}
		#endregion
		#region TaxAmt
		public new abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
        #endregion
    }
}
