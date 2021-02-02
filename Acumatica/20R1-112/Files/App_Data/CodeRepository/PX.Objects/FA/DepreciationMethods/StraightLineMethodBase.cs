namespace PX.Objects.FA.DepreciationMethods
{
	/// <exclude/>
	public abstract class StraightLineMethodBase : DepreciationMethodBase
	{
		protected override string CalculationMethod => FADepreciationMethod.depreciationMethod.StraightLine;
	}
}
