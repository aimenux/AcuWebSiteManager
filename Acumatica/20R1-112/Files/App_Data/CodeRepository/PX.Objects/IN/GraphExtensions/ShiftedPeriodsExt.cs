using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;

namespace PX.Objects.IN.GraphExtensions
{
	public class INRegisterShiftedPeriodsExt : ShiftedPeriodsExt<INRegisterEntryBase, INRegister, INRegister.tranDate, INRegister.tranPeriodID, INTran>
	{
		public override void Initialize()
		{
			base.Initialize();

			Documents = new PXSelectExtension<Document>(Base.INRegisterDataMember);
			Lines = new PXSelectExtension<DocumentLine>(Base.INTranDataMember);
		}
	}

	public abstract class ShiftedPeriodsExt<TGraph, TDocument, THeaderDocDate, THeaderTranPeriodID, TDocumentLine> : DocumentWithLinesGraphExtension<TGraph>
		where TGraph : PXGraph
		where TDocument : IBqlTable
		where TDocumentLine : IBqlTable
		where THeaderTranPeriodID : IBqlField
		where THeaderDocDate : IBqlField
	{
		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(TDocument))
			{
				HeaderTranPeriodID = typeof(THeaderTranPeriodID),
				HeaderDocDate = typeof(THeaderDocDate)
			};
		}

		protected override DocumentLineMapping GetDocumentLineMapping()
		{
			return new DocumentLineMapping(typeof(TDocumentLine));
		}
	}
}
