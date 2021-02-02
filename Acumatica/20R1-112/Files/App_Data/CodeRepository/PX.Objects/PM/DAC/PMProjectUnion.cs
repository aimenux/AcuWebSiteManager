using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.PM
{
	[PXCacheName(Messages.ProjectUnionLocals)]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMProjectUnion : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		/// <summary>
		/// Gets or sets the parent Project.
		/// </summary>
		protected Int32? _ProjectID;
		[Project(DisplayName = "Project ID", IsKey = true, DirtyRead = true)]
		[PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<PMProjectUnion.projectID>>>>))]
		[PXDBLiteDefault(typeof(PMProject.contractID))]
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
		#region UnionID
		public abstract class unionID : PX.Data.BQL.BqlString.Field<unionID> { }
		[PXForeignReference(typeof(Field<unionID>.IsRelatedTo<PMUnion.unionID>))]
		[PXRestrictor(typeof(Where<PMUnion.isActive, Equal<True>>), Messages.InactiveUnion, typeof(PMUnion.unionID))]
		[PXSelector(typeof(Search<PMUnion.unionID>), DescriptionField=typeof(PMUnion.description))]
		[PXDBString(PMUnion.unionID.Length, IsKey = true, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Union Local")]
		public virtual String UnionID
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
