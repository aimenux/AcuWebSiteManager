using System;
using PX.Data;

namespace PX.Objects.Common.Discount.Mappers
{
	public abstract class DiscountLineFields : DiscountedLineMapperBase
	{
		public virtual bool SkipDisc { get; set; }
		public virtual decimal? CuryDiscAmt { get; set; }
		public virtual decimal? DiscPct { get; set; }
		public virtual string DiscountID { get; set; }
		public virtual string DiscountSequenceID { get; set; }
		public virtual ushort[] DiscountsAppliedToLine { get; set; }
		public virtual bool ManualDisc { get; set; }
		public virtual bool ManualPrice { get; set; }
		public virtual string LineType { get; set; }
		public virtual bool? IsFree { get; set; }
		public virtual bool? CalculateDiscountsOnImport { get; set; }

		public abstract class skipDisc : PX.Data.BQL.BqlBool.Field<skipDisc> { }
		public abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt> { }
		public abstract class discPct : PX.Data.BQL.BqlDecimal.Field<discPct> { }
		public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
		public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }
		public abstract class discountsAppliedToLine : PX.Data.BQL.BqlByteArray.Field<discountsAppliedToLine> { }
		public abstract class manualDisc : PX.Data.BQL.BqlBool.Field<manualDisc> { }
		public abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		public abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree> { }
		public abstract class calculateDiscountsOnImport : PX.Data.BQL.BqlBool.Field<calculateDiscountsOnImport> { }

		protected DiscountLineFields(PXCache cache, object row)
			: base(cache, row) { }

		public static DiscountLineFields GetMapFor<TLine>(TLine line, PXCache cache)
			where TLine : class, IBqlTable
			=> new DiscountLineFields<skipDisc, curyDiscAmt, discPct, discountID, discountSequenceID, discountsAppliedToLine, manualDisc, manualPrice, lineType, isFree, calculateDiscountsOnImport>(cache, line);
	}

	public class DiscountLineFields<SkipDiscField, CuryDiscAmtField, DiscPctField, DiscountIDField, DiscountSequenceIDField, DiscountsAppliedToLineField, ManualDiscField, ManualPriceField, LineTypeField, IsFreeField, CalculateDiscountsOnImportField>
		: DiscountLineFields
		where SkipDiscField : IBqlField
		where CuryDiscAmtField : IBqlField
		where DiscPctField : IBqlField
		where DiscountIDField : IBqlField
		where DiscountSequenceIDField : IBqlField
		where DiscountsAppliedToLineField : IBqlField
		where ManualDiscField : IBqlField
		where ManualPriceField : IBqlField
		where LineTypeField : IBqlField
		where IsFreeField : IBqlField
		where CalculateDiscountsOnImportField : IBqlField
	{
		public DiscountLineFields(PXCache cache, object row)
			: base(cache, row) { }

		public override Type GetField<T>()
		{
			if (typeof(T) == typeof(skipDisc))
			{
				return typeof(SkipDiscField);
			}
			if (typeof(T) == typeof(curyDiscAmt))
			{
				return typeof(CuryDiscAmtField);
			}
			if (typeof(T) == typeof(discPct))
			{
				return typeof(DiscPctField);
			}
			if (typeof(T) == typeof(discountID))
			{
				return typeof(DiscountIDField);
			}
			if (typeof(T) == typeof(discountSequenceID))
			{
				return typeof(DiscountSequenceIDField);
			}
			if (typeof(T) == typeof(discountsAppliedToLine))
			{
				return typeof(DiscountsAppliedToLineField);
			}
			if (typeof(T) == typeof(manualDisc))
			{
				return typeof(ManualDiscField);
			}
			if (typeof(T) == typeof(manualPrice))
			{
				return typeof(ManualPriceField);
			}
			if (typeof(T) == typeof(lineType))
			{
				return typeof(LineTypeField);
			}
			if (typeof(T) == typeof(isFree))
			{
				return typeof(IsFreeField);
			}
			if (typeof(T) == typeof(calculateDiscountsOnImport))
			{
				return typeof(CalculateDiscountsOnImportField);
			}
			return null;
		}

		public override bool SkipDisc
		{
			get { return (bool?) Cache.GetValue<SkipDiscField>(MappedLine) == true; }
			set { Cache.SetValue<SkipDiscField>(MappedLine, value); }
		}

		public override decimal? CuryDiscAmt
		{
			get { return (decimal?) Cache.GetValue<CuryDiscAmtField>(MappedLine); }
			set { Cache.SetValue<CuryDiscAmtField>(MappedLine, value); }
		}

		public override decimal? DiscPct
		{
			get { return (decimal?) Cache.GetValue<DiscPctField>(MappedLine); }
			set { Cache.SetValue<DiscPctField>(MappedLine, value); }
		}

		public override string DiscountID
		{
			get { return (string) Cache.GetValue<DiscountIDField>(MappedLine); }
			set { Cache.SetValue<DiscountIDField>(MappedLine, value); }
		}

		public override string DiscountSequenceID
		{
			get { return (string) Cache.GetValue<DiscountSequenceIDField>(MappedLine); }
			set { Cache.SetValue<DiscountSequenceIDField>(MappedLine, value); }
		}

		public override ushort[] DiscountsAppliedToLine
		{
			get { return (ushort[])Cache.GetValue<DiscountsAppliedToLineField>(MappedLine); }
			set { Cache.SetValue<DiscountsAppliedToLineField>(MappedLine, value); }
		}

		public override bool ManualDisc
		{
			get { return (bool?) Cache.GetValue<ManualDiscField>(MappedLine) == true; }
			set { Cache.SetValue<ManualDiscField>(MappedLine, value); }
		}

		public override bool ManualPrice
		{
			get { return (bool?) Cache.GetValue<ManualPriceField>(MappedLine) == true; }
			set { Cache.SetValue<ManualPriceField>(MappedLine, value); }
		}

		public override string LineType
		{
			get { return (string) Cache.GetValue<LineTypeField>(MappedLine); }
			set { Cache.SetValue<LineTypeField>(MappedLine, value); }
		}

		public override bool? IsFree
		{
			get { return (bool?) Cache.GetValue<IsFreeField>(MappedLine) == true; }
			set { Cache.SetValue<IsFreeField>(MappedLine, value); }
		}

		public override bool? CalculateDiscountsOnImport
		{
			get { return (bool?)Cache.GetValue<CalculateDiscountsOnImportField>(MappedLine) == true; }
			set { Cache.SetValue<CalculateDiscountsOnImportField>(MappedLine, value); }
		}
	}
}