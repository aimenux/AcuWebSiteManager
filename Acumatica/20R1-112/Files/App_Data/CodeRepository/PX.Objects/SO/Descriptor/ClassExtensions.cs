namespace PX.Objects.SO
{
	public static class ClassExtensions
	{
		internal static void ClearPOFlags(this SOLineSplit split)
		{
			split.POCompleted = false;
			split.POCancelled = false;
			
			split.POCreate = false;
			split.POSource = null;
		}

		internal static void ClearPOReferences(this SOLineSplit split)
		{
			split.POType = null;
			split.PONbr = null;
			split.POLineNbr = null;
			
			split.POReceiptType = null;
			split.POReceiptNbr = null;
		}

		internal static void ClearSOReferences(this SOLineSplit split)
		{
			split.SOOrderType = null;
			split.SOOrderNbr = null;
			split.SOLineNbr = null;
			split.SOSplitLineNbr = null;
		}
	}
}
