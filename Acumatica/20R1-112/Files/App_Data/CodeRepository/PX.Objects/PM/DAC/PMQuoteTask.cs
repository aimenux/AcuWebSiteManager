namespace PX.Objects.PM
{
    using PX.Data;
    using PX.Data.BQL;
    using PX.Data.EP;
    using PX.Objects.CN.ProjectAccounting.Descriptor;
    using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
    using PX.Objects.CS;
    using PX.Objects.TX;
    using System;

    [System.SerializableAttribute()]
	[PXCacheName(Messages.PMTaskRate)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMQuoteTask : PX.Data.IBqlTable
	{
		#region QuoteID
		public abstract class quoteID : PX.Data.BQL.BqlGuid.Field<quoteID> { }
		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(PMQuote.quoteID))]
		[PXParent(typeof(Select<PMQuote, Where<PMQuote.quoteID, Equal<Current<quoteID>>>>))]
		public virtual Guid? QuoteID { get; set; }
		#endregion

		#region TaskCD
		public abstract class taskCD : PX.Data.BQL.BqlString.Field<taskCD> { }
		protected String _TaskCD;
		[PXDimension(ProjectTaskAttribute.DimensionName)]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Project Task", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TaskCD
		{
			get
			{
				return this._TaskCD;
			}
			set
			{
				this._TaskCD = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		/// <summary>
		/// Gets or sets description
		/// </summary>
		[PXDBLocalizableString(250, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region PlannedStartDate
		public abstract class plannedStartDate : PX.Data.BQL.BqlDateTime.Field<plannedStartDate> { }
		protected DateTime? _PlannedStartDate;
		/// <summary>
		/// Gets or sets the date, when the task is suppose to start
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Planned Start Date")]
		public virtual DateTime? PlannedStartDate
		{
			get
			{
				return this._PlannedStartDate;
			}
			set
			{
				this._PlannedStartDate = value;
			}
		}
		#endregion
		#region PlannedEndDate
		public abstract class plannedEndDate : PX.Data.BQL.BqlDateTime.Field<plannedEndDate> { }
		protected DateTime? _PlannedEndDate;
		/// <summary>
		///  Gets or sets the date, when the task is suppose to finish
		/// </summary>
		[PXDBDate()]
		[PXVerifyEndDate(typeof(plannedStartDate), AutoChangeWarning = true)]
		[PXUIField(DisplayName = "Planned End Date")]
		public virtual DateTime? PlannedEndDate
		{
			get
			{
				return this._PlannedEndDate;
			}
			set
			{
				this._PlannedEndDate = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		public virtual String TaxCategoryID
		{
			get;
			set;
		}
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }

		/// <summary>
		/// Gets or sets whether the task is deafult or not.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default")]
		public virtual Boolean? IsDefault
		{
			get;
			set;
		}
        #endregion
        #region Type
        public abstract class type : BqlString.Field<type> { }
        /// <summary>
        /// Gets or sets task type. Relevant fot Construction feature.
        /// </summary>
        [PXDBString(10)]
        [PXDefault(ProjectTaskType.CostRevenue)]
        [PXUIField(DisplayName = ProjectAccountingLabels.Type, Required = true, FieldClass = nameof(FeaturesSet.Construction))]
        [ProjectTaskType.List]
        public string Type
        {
            get;
            set;
        }
        #endregion

        #region System Columns
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(PMQuoteTask.taskCD))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion
	}
}
