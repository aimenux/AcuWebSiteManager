using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommonServiceLocator;
using PX.Data;
using PX.Data.ReferentialIntegrity;
using PX.Data.ReferentialIntegrity.Inspecting;
using PX.Data.ReferentialIntegrity.Merging;

namespace PX.Objects.SM
{
	public class MergedDatabaseSchemaInquiry : PXGraph<MergedDatabaseSchemaInquiry>
	{
		public PXPrevious<MergedInspectingTable> Previous;
		public PXNext<MergedInspectingTable> Next;
		public PXFirst<MergedInspectingTable> First;
		public PXLast<MergedInspectingTable> Last;
		public PXCancel<MergedInspectingTable> Cancel;


		[InjectDependency]
		public ITableMergedReferencesInspector TableMergedReferencesInspector { get; set; }


		public MergedDatabaseSchemaInquiry()
		{
			Cancel.SetVisible(false);
		}

		private static IEnumerable<MergedInspectingTable> GetMergedInspectingTables(ITableMergedReferencesInspector inspector)
			=> inspector == null
				? Enumerable.Empty<MergedInspectingTable>()
				: inspector.GetMergedReferencesOfAllTables().Select(t => new MergedInspectingTable(t.Value));

		private static IEnumerable<MergedInspectingTable> GetMergedInspectingTables()
			=> GetMergedInspectingTables(ServiceLocator.IsLocationProviderSet
				? ServiceLocator.Current.GetInstance<ITableMergedReferencesInspector>()
				: null);


		public PXSelect<MergedInspectingTable> inspectingTables;
		protected IEnumerable InspectingTables() => GetMergedInspectingTables(TableMergedReferencesInspector);

		public PXSelect<MergedTableReference> tableOutgoingReferences;
		protected IEnumerable TableOutgoingReferences()
			=> inspectingTables.Current?
				.MergedInspectionResult?
				.OutgoingMergedReferences?
				.Select(r => new MergedTableReference(r)) ?? Enumerable.Empty<MergedTableReference>();
		
		public PXSelect<MergedTableReference> tableIncomingReferences;
		protected IEnumerable TableIncomingReferences()
			=> inspectingTables.Current?
				.MergedInspectionResult?
				.IncomingMergedReferences?
				.Select(r => new MergedTableReference(r)) ?? Enumerable.Empty<MergedTableReference>();

		public PXAction<MergedInspectingTable> viewParent;
		[PXButton, PXUIField(DisplayName = "")]
		protected virtual void ViewParent()
		{
			TableReference reference = tableOutgoingReferences.Current;
			inspectingTables.Current = inspectingTables.Search<InspectingTable.fullName>(reference.ParentFullName);
			throw new PXRedirectRequiredException(this, false, null);
		}

		public PXAction<MergedInspectingTable> viewChild;
		[PXButton, PXUIField(DisplayName = "")]
		protected virtual void ViewChild()
		{
			TableReference reference = tableIncomingReferences.Current;
			inspectingTables.Current = inspectingTables.Search<InspectingTable.fullName>(reference.ChildFullName);
			throw new PXRedirectRequiredException(this, false, null);
		}


		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelectorByMethod(typeof(MergedDatabaseSchemaInquiry), nameof(GetMergedInspectingTables), typeof(Search<InspectingTable.fullName>))]
		protected void MergedTableReference_ParentFullName_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelectorByMethod(typeof(MergedDatabaseSchemaInquiry), nameof(GetMergedInspectingTables), typeof(Search<InspectingTable.fullName>))]
		protected void MergedTableReference_ChildFullName_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelectorByMethod(typeof(MergedDatabaseSchemaInquiry), nameof(GetMergedInspectingTables), typeof(Search<InspectingTable.fullName>))]
		protected virtual void MergedInspectingTable_BaseClassName_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelectorByMethod(typeof(MergedDatabaseSchemaInquiry), nameof(GetMergedInspectingTables),
			typeof(Search<InspectingTable.fullName>),
			typeof(InspectingTable.className),
			typeof(InspectingTable.fullName),
			typeof(InspectingTable.hasIncoming),
			typeof(InspectingTable.hasOutgoing))]
		protected void MergedInspectingTable_FullName_CacheAttached(PXCache sender) { }
	}

	[PXHidden]
	[Serializable]
	public partial class MergedInspectingTable : InspectingTable
	{
		public MergedInspectingTable() {}

		public MergedInspectingTable(MergedReferencesInspectionResult mergedInspectionResult)
			:base(mergedInspectionResult.ReferencesInspectionResult)
		{
			MergedInspectionResult = mergedInspectionResult;
		}

		public MergedReferencesInspectionResult MergedInspectionResult { get; }

		public override ReferencesInspectionResult InspectionResult => MergedInspectionResult?.ReferencesInspectionResult;
	}

	[PXHidden]
	[Serializable]
	public partial class MergedTableReference : TableReference
	{
		[PXString]
		[PXUIField(DisplayName = "Substitutable Parents", Enabled = false)]
		public String SubstitutableParents { get; set; }
		public abstract class substitutableParents { }

		[PXString]
		[PXUIField(DisplayName = "Substitutable Children", Enabled = false)]
		public String SubstitutableChildren { get; set; }
		public abstract class substitutableChildren { }

		public MergedTableReference() {}

		public MergedTableReference(MergedReference mergedReference)
			: base(mergedReference.Reference)
		{
			MergedReference = mergedReference;
			SubstitutableChildren = String.Join(Environment.NewLine, mergedReference.ApplicableChildren.Select(t => $"{t.Name} ({t.FullName})"));
			SubstitutableParents = String.Join(Environment.NewLine, mergedReference.ApplicableParents.Select(t => $"{t.Name} ({t.FullName})"));
		}

		public MergedReference MergedReference { get; }

		public override Reference Reference => MergedReference?.Reference;
	}
}