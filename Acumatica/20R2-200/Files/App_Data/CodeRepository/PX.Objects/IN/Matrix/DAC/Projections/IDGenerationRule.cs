using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN.Matrix.DAC.Projections
{
	[PXCacheName(Messages.IDGenerationRuleDAC)]
	[PXBreakInheritance]
	[PXProjection(typeof(Select<INMatrixGenerationRule,
		Where<IDGenerationRule.type, Equal<INMatrixGenerationRule.type.id>>>), Persistent = true)]
	public class IDGenerationRule : INMatrixGenerationRule
	{
		#region Keys
		public new class PK : PrimaryKeyOf<INCostStatus>.By<templateID, type, lineNbr>
		{
			public static INCostStatus Find(PXGraph graph, int? templateID, string type, int? lineNbr)
				=> FindBy(graph, templateID, type, lineNbr);
		}
		public static new class FK
		{
			public class TemplateItem : InventoryItem.PK.ForeignKeyOf<INCostStatus>.By<templateID> { }
		}
		#endregion

		#region TemplateID
		public abstract new class templateID : PX.Data.BQL.BqlInt.Field<templateID> { }

		/// <summary>
		/// Template Inventory Item identifier.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		[PXParent(typeof(FK.TemplateItem))]
		public override int? TemplateID
		{
			get => base.TemplateID;
			set => base.TemplateID = value;
		}
		#endregion
		#region Type
		public abstract new class type : PX.Data.BQL.BqlString.Field<type> { }

		[PXDBString(1, IsKey = true, IsFixed = true, IsUnicode = false)]
		[INMatrixGenerationRule.type.List]
		[PXDefault(INMatrixGenerationRule.type.ID)]
		public override string Type
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract new class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(InventoryItem.generationRuleCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public override int? LineNbr
		{
			get => base.LineNbr;
			set => base.LineNbr = value;
		}
		#endregion
	}
}
