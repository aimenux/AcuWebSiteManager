using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommonServiceLocator;
using PX.Common;
using PX.Common.Extensions;
using PX.Data;
using PX.Data.ReferentialIntegrity;
using PX.Data.ReferentialIntegrity.Inspecting;

namespace PX.Objects.SM
{
	public class DatabaseSchemaInquiry : PXGraph<DatabaseSchemaInquiry>
	{
		public PXPrevious<InspectingTable> Previous;
		public PXNext<InspectingTable> Next;
		public PXFirst<InspectingTable> First;
		public PXLast<InspectingTable> Last;
		public PXCancel<InspectingTable> Cancel;


		[InjectDependency]
		public ITableReferenceInspector TableReferencesInspector { get; set; }


		public DatabaseSchemaInquiry()
		{
			Cancel.SetVisible(false);
		}


		private static IEnumerable<InspectingTable> GetInspectingTables(ITableReferenceInspector inspector)
			=> inspector == null
				? Enumerable.Empty<InspectingTable>()
				: inspector.GetReferencesOfAllDacs().Select(t => new InspectingTable(t.Value));

		private static IEnumerable<InspectingTable> GetInspectingTables()
			=> GetInspectingTables(ServiceLocator.IsLocationProviderSet
				? ServiceLocator.Current.GetInstance<ITableReferenceInspector>()
				: null);


		public PXSelect<InspectingTable> inspectingTables;
		protected IEnumerable InspectingTables() => GetInspectingTables();


		public PXSelect<TableReference> tableOutgoingReferences;

		protected IEnumerable TableOutgoingReferences()
			=> inspectingTables.Current?
								.InspectionResult?
								.OutgoingReferences?
								.Select(r => new TableReference(r)) ?? Enumerable.Empty<TableReference>();


		public PXSelect<TableReference> tableIncomingReferences;

		protected IEnumerable TableIncomingReferences()
			=> inspectingTables.Current?
								.InspectionResult?
								.IncomingReferences?
								.Select(r => new TableReference(r)) ?? Enumerable.Empty<TableReference>();


		public PXAction<InspectingTable> viewParent;
		[PXButton, PXUIField(DisplayName = "")]
		protected virtual void ViewParent()
		{
			TableReference reference = tableOutgoingReferences.Current;
			inspectingTables.Current = inspectingTables.Search<InspectingTable.fullName>(reference.ParentFullName);
			throw new PXRedirectRequiredException(this, false, null);
		}

		public PXAction<InspectingTable> viewChild;
		[PXButton, PXUIField(DisplayName = "")]
		protected virtual void ViewChild()
		{
			TableReference reference = tableIncomingReferences.Current;
			inspectingTables.Current = inspectingTables.Search<InspectingTable.fullName>(reference.ChildFullName);
			throw new PXRedirectRequiredException(this, false, null);
		}


		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelectorByMethod(typeof(DatabaseSchemaInquiry), nameof(GetInspectingTables), typeof(Search<InspectingTable.fullName>))]
		protected virtual void TableReference_ParentFullName_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelectorByMethod(typeof(DatabaseSchemaInquiry), nameof(GetInspectingTables), typeof(Search<InspectingTable.fullName>))]
		protected virtual void TableReference_ChildFullName_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelectorByMethod(typeof(DatabaseSchemaInquiry), nameof(GetInspectingTables), typeof(Search<InspectingTable.fullName>))]
		protected virtual void InspectingTable_BaseClassName_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelectorByMethod(typeof(DatabaseSchemaInquiry), nameof(GetInspectingTables),
			typeof(Search<InspectingTable.fullName>),
			typeof(InspectingTable.className),
			typeof(InspectingTable.fullName),
			typeof(InspectingTable.hasIncoming),
			typeof(InspectingTable.hasOutgoing))]
		protected virtual void InspectingTable_FullName_CacheAttached(PXCache sender) { }
	}

	[PXHidden]
	[Serializable]
	[PXPrimaryGraph(typeof(DatabaseSchemaInquiry))]
	public partial class InspectingTable : IBqlTable
	{
		[PXString(IsKey = true)]
		[PXUIField(DisplayName = "DAC Full Name")]
		public string FullName { get; set; }
		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }

		[PXString]
		[PXUIField(DisplayName = "Class Name", Enabled = false)]
		public string ClassName { get; set; }
		public abstract class className : PX.Data.BQL.BqlString.Field<className> { }

		[PXString]
		[PXUIField(DisplayName = "Namespace", Enabled = false)]
		public string NamespaceName { get; set; }
		public abstract class namespaceName : PX.Data.BQL.BqlString.Field<namespaceName> { }

		[PXString]
		[PXUIField(DisplayName = "Short Name", Enabled = false)]
		public string ShortName { get; set; }
		public abstract class shortName : PX.Data.BQL.BqlString.Field<shortName> { }

		[PXString]
		[PXUIField(DisplayName = "Base Class", Enabled = false)]
		public string BaseClassName { get; set; }
		public abstract class baseClassName : PX.Data.BQL.BqlString.Field<baseClassName> { }

		[PXBool]
		[PXUIField(DisplayName = "Has Outgoing", Enabled = false)]
		public bool? HasOutgoing { get; set; }
		public abstract class hasOutgoing : PX.Data.BQL.BqlBool.Field<hasOutgoing> { }

		[PXBool]
		[PXUIField(DisplayName = "Has Incoming", Enabled = false)]
		public bool? HasIncoming { get; set; }
		public abstract class hasIncoming : PX.Data.BQL.BqlBool.Field<hasIncoming> { }


		public InspectingTable() {}

		public InspectingTable(ReferencesInspectionResult inspectionResult)
		{
			InspectionResult = inspectionResult;
			ClassName = inspectionResult.InspectingTable.FullName.LastSegment('.');
			FullName = inspectionResult.InspectingTable.FullName.Replace("+", "..");
			ShortName = inspectionResult.InspectingTable.Name;
			NamespaceName = inspectionResult.InspectingTable.Namespace;
			BaseClassName = (inspectionResult.InspectingTable.BaseType != typeof(object)
				? inspectionResult.InspectingTable.BaseType
				: inspectionResult.InspectingTable)?.FullName ?? "Unknown";
			HasIncoming = inspectionResult.IncomingReferences.Any();
			HasOutgoing = inspectionResult.OutgoingReferences.Any();
		}

		public virtual ReferencesInspectionResult InspectionResult { get; }
	}

	[PXHidden]
	[Serializable]
	public partial class TableReference : IBqlTable
	{
		[PXString(IsKey = true)]
		[PXUIField(DisplayName = "Parent Table", Enabled = false)]
		public string ParentFullName { get; set; }
		public abstract class parentFullName : PX.Data.BQL.BqlString.Field<parentFullName> { }

		[PXString(IsKey = true)]
		[PXUIField(DisplayName = "Child Table", Enabled = false)]
		public string ChildFullName { get; set; }
		public abstract class childFullName : PX.Data.BQL.BqlString.Field<childFullName> { }


		[PXUIField(DisplayName = "Parent Table", Enabled = false)]
		public string ParentDac { get; set; }
		public abstract class parentDac : PX.Data.BQL.BqlString.Field<parentDac> { }
		
		[PXUIField(DisplayName = "Child Table", Enabled = false)]
		public string ChildDac { get; set; }
		public abstract class childDac : PX.Data.BQL.BqlString.Field<childDac> { }
		
		[PXString(IsKey = true)]
		[PXUIField(DisplayName = "Parent Key Fields", Enabled = false)]
		public string ParentFields { get; set; }
		public abstract class parentFields : PX.Data.BQL.BqlString.Field<parentFields> { }
		
		[PXString(IsKey = true)]
		[PXUIField(DisplayName = "Child Key Fields", Enabled = false)]
		public string ChildFields { get; set; }
		public abstract class childFields : PX.Data.BQL.BqlString.Field<childFields> { }

		[PXString(IsKey = true)]
		[PXUIField(DisplayName = "Achieved By", Enabled = false)]
		[PXStringList(
			new[] { nameof(ReferenceOrigin.ForeignKeyApi), nameof(ReferenceOrigin.ParentAttribute), nameof(ReferenceOrigin.DeclareReferenceAttribute), nameof(ReferenceOrigin.SelectorAttribute), nameof(ReferenceOrigin.WhereInCustomSelect), nameof(ReferenceOrigin.JoinInCustomSelect) },
			new[] { "Foreign Key API", "Parent Attribute", "Declared Reference Attribute", "Selector Attribute", "Where In Custom Select", "Join In Custom Select" })]
		public string AchievedBy { get; set; }
		public abstract class achievedBy : PX.Data.BQL.BqlString.Field<achievedBy> { }
		
		[PXString(IsKey = true)]
		[PXUIField(DisplayName = "Behavior", Enabled = false)]
		[PXStringList(
			new[] { nameof(ReferenceBehavior.NoAction), nameof(ReferenceBehavior.Restrict), nameof(ReferenceBehavior.SetNull), nameof(ReferenceBehavior.SetDefault), nameof(ReferenceBehavior.Cascade) },
			new[] { "No Action", "Restrict", "Set Null", "Set Default", "Cascade" })]
		public string Behavior { get; set; }
		public abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }

		[PXString]
		[PXUIField(DisplayName = "Parent Select", Enabled = false)]
		public string ParentSelect { get; set; }
		public abstract class parentSelect : PX.Data.BQL.BqlString.Field<parentSelect> { }
		
		[PXString]
		[PXUIField(DisplayName = "Child Select", Enabled = false)]
		public string ChildSelect { get; set; }
		public abstract class childSelect : PX.Data.BQL.BqlString.Field<childSelect> { }

		[PXString]
		[PXUIField(DisplayName = "Original Select", Enabled = false)]
		public string OriginalSelect { get; set; }
		public abstract class originalSelect : PX.Data.BQL.BqlString.Field<originalSelect> { }


		public TableReference() {}

		public TableReference(Reference reference)
		{
			Reference = reference;
			ChildFullName = reference.Child.Table.FullName.Replace("+", "..");
			ChildDac = reference.Child.Table.Name;
			ChildFields = reference.Child.KeyFieldsToString;
			ParentFullName = reference.Parent.Table.FullName.Replace("+", "..");
			ParentDac = reference.Parent.Table.Name;
			ParentFields = reference.Parent.KeyFieldsToString;
			AchievedBy = reference.ReferenceOrigin.ToString();
			Behavior = reference.ReferenceBehavior.ToString();
			ParentSelect = ChopBql(reference.ParentSelect?.ToCodeString() ?? "");
			ChildSelect = ChopBql(reference.ChildrenSelect?.ToCodeString() ?? "");
			OriginalSelect = ChopBql(reference.OriginalSelect?.ToCodeString() ?? "");
		}

		public virtual Reference Reference { get; }

		protected string ChopBql(string bql)
		{
			string[] keywords =
			{
				"Where<",
				"Where2<",
				"InnerJoin<",
				"LeftJoin<",
				"RightJoin<",
				"CrossJoin<",
				"FullJoin<",
				"And<",
				"And2<",
				"Or<",
				"Or2<",
				"OrderBy<",
				"GroupBy<"
			};

			string result = bql.Replace(Environment.NewLine, "");
			foreach (var keyword in keywords)
				result = result.Replace(keyword, Environment.NewLine + keyword);
			return result;
		}
	}
}