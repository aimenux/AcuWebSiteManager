using System;
using System.Collections;
using System.Collections.Generic;

using PX.Data;

using PX.Objects.AP;
using PX.Objects.AP.MigrationMode;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.IN;

namespace PX.Objects.PO
{
	[PX.Objects.GL.TableAndChartDashboardType]
	[Serializable]
	public class POLandedCostProcess : PXGraph<POLandedCostProcess>
	{

		public PXCancel<POLandedCostDoc> Cancel;
		public PXAction<POLandedCostDoc> ViewDocument;

		[PXFilterable]
		public PXProcessingJoin<POLandedCostDoc, InnerJoin<Vendor, On<Vendor.bAccountID, Equal<POLandedCostDoc.vendorID>>>,
			Where<POLandedCostDoc.released, Equal<False>, And<POLandedCostDoc.hold, Equal<False>,
			And<Match<Vendor, Current<AccessInfo.userName>>>>>> landedCostDocsList;


		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton()]
		public virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			if (this.landedCostDocsList.Current != null)
			{
				var graph = PXGraph.CreateInstance<POLandedCostDocEntry>();
				var poDoc = graph.Document.Search<POLandedCostDoc.refNbr>(landedCostDocsList.Current.RefNbr, landedCostDocsList.Current.DocType);
				if (poDoc != null)
				{
					graph.Document.Current = poDoc;
					throw new PXRedirectRequiredException(graph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXSetup<POSetup> POSetup;

		public POLandedCostProcess()
		{
			APSetupNoMigrationMode.EnsureMigrationModeDisabled(this);
			POSetup setup = POSetup.Current;

			landedCostDocsList.SetSelected<POLandedCostDoc.selected>();
			landedCostDocsList.SetProcessCaption(Messages.Process);
			landedCostDocsList.SetProcessAllCaption(Messages.ProcessAll);
			landedCostDocsList.SetProcessDelegate(delegate (List<POLandedCostDoc> list)
			{
				ReleaseDoc(list, true);
			});
		}

		public static void ReleaseDoc(List<POLandedCostDoc> list, bool aIsMassProcess)
		{
			var docgraph = PXGraph.CreateInstance<POLandedCostDocEntry>();
			int iRow = 0;
			bool failed = false;
			foreach (POLandedCostDoc doc in list)
			{
				try
				{
					docgraph.ReleaseDoc(doc);
					PXProcessing<POLandedCostDoc>.SetInfo(iRow, ActionsMessages.RecordProcessed);
				}
				catch (Exception e)
				{
					if (aIsMassProcess)
					{
						PXProcessing<POLandedCostDoc>.SetError(iRow, e);
						failed = true;
					}
					else
						throw;
				}
				iRow++;
			}
			if (failed)
			{
				throw new PXException(Messages.LandedCostProcessingForOneOrMorePOReceiptsFailed);
			}
		}
	}
}