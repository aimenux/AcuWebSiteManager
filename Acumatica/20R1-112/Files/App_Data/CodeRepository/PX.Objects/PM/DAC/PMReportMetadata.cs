using PX.Data;
using System;

namespace PX.Objects.PM
{
	/// <summary>
	/// This is a virtual DAC. The fields/selector of this dac is used in reports.
	/// </summary>
	[PXHidden]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMReportMetadata : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		//Note: This field is used by Reports (Selector in parameters).
		[PXDefault]
		[Project(typeof(Where<PMProject.baseType, Equal<CT.CTPRType.project>, And<PMProject.nonProject, NotEqual<True>>>), WarnIfCompleted = false)]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		protected Int32? _ProjectTaskID;
		//Note: This field is used by Reports (Selector in parameters).
		[BaseProjectTask(typeof(PMReportMetadata.projectID))]
		public virtual Int32? ProjectTaskID
		{
			get
			{
				return this._ProjectTaskID;
			}
			set
			{
				this._ProjectTaskID = value;
			}
		}
		#endregion
	}
}
