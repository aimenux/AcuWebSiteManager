using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;

namespace PX.Objects.IN.GraphExtensions.KitAssemblyEntryExt
{
	public class ShiftedPeriodsExt : ShiftedPeriodsExt<KitAssemblyEntry, INKitRegister, INKitRegister.tranDate, INKitRegister.tranPeriodID, INComponentTran>
	{
		public PXSelectExtension<DocumentLine> Overheads;

		public override void Initialize()
		{
			base.Initialize();
			Documents = new PXSelectExtension<Document>(Base.Document);
			Lines = new PXSelectExtension<DocumentLine>(Base.Components);
			Overheads = new PXSelectExtension<DocumentLine>(Base.Overhead);
		}

		protected virtual DocumentLineMapping GetDocumentOverheadMapping()
		{
			return new DocumentLineMapping(typeof(INOverheadTran));
		}

		protected override void _(Events.RowUpdated<Document> e)
		{
			base._(e);

			if (ShouldUpdateOverheadsOnDocumentUpdated(e))
			{
				foreach (DocumentLine overhead in Overheads.Select())
				{
					Overheads.Cache.SetDefaultExt<DocumentLine.finPeriodID>(overhead);

					Overheads.Cache.MarkUpdated(overhead);
				}
			}
		}

		protected virtual bool ShouldUpdateOverheadsOnDocumentUpdated(Events.RowUpdated<Document> e)
		{
			return ShouldUpdateDetailsOnDocumentUpdated(e);
		}
	}
}
