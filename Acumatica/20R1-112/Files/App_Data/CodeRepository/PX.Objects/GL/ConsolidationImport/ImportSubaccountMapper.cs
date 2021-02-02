using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.Common.Logging;
using PX.Objects.CS;

namespace PX.Objects.GL.ConsolidationImport
{
	public class ImportSubaccountMapper : IImportSubaccountMapper
	{
		protected readonly GLSetup GLSetup;
		protected readonly GLConsolSetup GLConsolSetup;
		private readonly Func<string, int?> _findSubIdbyCD;
		protected readonly IAppLogger Logger;


		private readonly int _subaccountCDKeyLength;
		private int _segmentStartIndex;

		public ImportSubaccountMapper(
			IReadOnlyCollection<Segment> segments,
			GLSetup glSetup,
			GLConsolSetup glConsolSetup,
			Func<string, int?> findSubIDByCD,
			IAppLogger logger)
		{
			GLSetup = glSetup;
			GLConsolSetup = glConsolSetup;
			_findSubIdbyCD = findSubIDByCD;
			Logger = logger;

			CalcSegmentStartIndex(segments);
			_subaccountCDKeyLength = segments.Sum(segment => segment.Length.Value);
		}

		private void CalcSegmentStartIndex(IEnumerable<Segment> segments)
		{
			if (GLConsolSetup.PasteFlag == true)
			{
				_segmentStartIndex = segments.OrderBy(segment => segment.SegmentID)
												.TakeWhile(segment => segment.SegmentID != GLSetup.ConsolSegmentId.Value)
												.Sum(segment => segment.Length.Value);
			}
			else
			{
				_segmentStartIndex = -1;
			}
		}

		/// <summary>
		/// Validate subaccountCD length
		/// and insert "Consolidation Segment Value" if it is need
		/// </summary>
		public Sub.Keys GetMappedSubaccountKeys(string subaccountCD)
		{
			if (subaccountCD == null)
				throw new ArgumentNullException("subaccountCD");

			var pasteFlag = GLConsolSetup.PasteFlag ?? false;
			var pastingSegmentLength = pasteFlag ? GLConsolSetup.SegmentValue.Length : 0;

			var extrapolatedSubaccountLength = subaccountCD.Length + pastingSegmentLength;

			if (extrapolatedSubaccountLength != _subaccountCDKeyLength)
			{
				Logger.WriteWarning(PXMessages.LocalizeFormatNoPrefix(Messages.ConsolidationFailedToAssembleDestinationSub, subaccountCD));
			}

			if (extrapolatedSubaccountLength < _subaccountCDKeyLength)
			{
				subaccountCD = subaccountCD.PadRight(_subaccountCDKeyLength - pastingSegmentLength);
			}

			if (pasteFlag)
			{
				subaccountCD = subaccountCD.Insert(_segmentStartIndex, GLConsolSetup.SegmentValue);
			}

			if (subaccountCD.Length > _subaccountCDKeyLength)
			{
				subaccountCD = subaccountCD.Substring(0, _subaccountCDKeyLength);
			}

			return new Sub.Keys()
			{
				SubCD = subaccountCD,
				SubID = _findSubIdbyCD(subaccountCD)
			};
		}
	}
}
