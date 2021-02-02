namespace PX.Objects.PM
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.PMProjectRate)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMProjectRate : PX.Data.IBqlTable
	{
		#region RateDefinitionID
		public abstract class rateDefinitionID : PX.Data.BQL.BqlInt.Field<rateDefinitionID> { }
		protected int? _RateDefinitionID;
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PMRateSequence.rateDefinitionID))]
		[PXParent(typeof(Select<PMRateSequence, Where<PMRateSequence.rateDefinitionID, Equal<Current<PMProjectRate.rateDefinitionID>>, And<PMRateSequence.rateCodeID, Equal<Current<PMProjectRate.rateCodeID>>>>>))]
		public virtual int? RateDefinitionID
		{
			get
			{
				return this._RateDefinitionID;
			}
			set
			{
				this._RateDefinitionID = value;
			}
		}
		#endregion
		#region RateCodeID
		public abstract class rateCodeID : PX.Data.BQL.BqlString.Field<rateCodeID> { }
		protected String _RateCodeID;

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(PMRateSequence.rateCodeID))]
		public virtual String RateCodeID
		{
			get
			{
				return this._RateCodeID;
			}
			set
			{
				this._RateCodeID = value;
			}
		}
		#endregion
		#region ProjectCD
		public abstract class projectCD : PX.Data.BQL.BqlString.Field<projectCD> { }
		protected String _ProjectCD;
		[PXDimensionWildcardAttribute(ProjectAttribute.DimensionName, 
			typeof(Search<PMProject.contractCD, Where<PMProject.baseType, Equal<CT.CTPRType.project>>>),
			typeof(PMProject.contractCD), typeof(PMProject.description))]
		[PXDBString(IsKey = true, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName="Project")]
		public virtual String ProjectCD
		{
			get
			{
				return this._ProjectCD;
			}
			set
			{
				this._ProjectCD = value;
			}
		}
		#endregion
	}
}
