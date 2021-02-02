namespace PX.Objects.IN
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;

	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(INLotSerClassMaint))]
	[PXCacheName(Messages.LotSerClass, PXDacType.Catalogue, CacheGlobal = true)]
	public partial class INLotSerClass : PX.Data.IBqlTable
	{
		private const string DfltLotSerialClass = "DEFAULT";
		public class dfltLotSerialClass : PX.Data.BQL.BqlString.Constant<dfltLotSerialClass>
		{
			public dfltLotSerialClass()
				: base(DfltLotSerialClass)
			{
			}
		}

		public static string GetDefaultLotSerClass(PXGraph graph)
		{
			INLotSerClass lotSerClass = PXSelect<INLotSerClass, Where<INLotSerClass.lotSerTrack, Equal<INLotSerTrack.notNumbered>>>.Select(graph);
			if (lotSerClass == null)
			{
				PXCache cache = graph.Caches<INLotSerClass>();
				INLotSerClass lotser = (INLotSerClass) cache.CreateInstance();
				lotser.LotSerClassID = DfltLotSerialClass;
				lotser.LotSerTrack = INLotSerTrack.NotNumbered;
				cache.Insert(lotser);
				return lotser.LotSerClassID;
			}
			else
			{
				return lotSerClass.LotSerClassID;
			}
		}

		#region Keys
		public class PK : PrimaryKeyOf<INLotSerClass>.By<lotSerClassID>
		{
			public static INLotSerClass Find(PXGraph graph, string lotSerClassID) => FindBy(graph, lotSerClassID);

			internal static INLotSerClass FindDirty(PXGraph graph, string lotSerClassID)
				=> (INLotSerClass)PXSelect<INLotSerClass, 
					Where<INLotSerClass.lotSerClassID, Equal<Required<INLotSerClass.lotSerClassID>>>>.SelectWindowed(graph, 0, 1, lotSerClassID);
		}
		#endregion
		#region LotSerClassID
		public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }
		protected String _LotSerClassID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName="Class ID", Visibility=PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INLotSerClass.lotSerClassID>))]
		[PX.Data.EP.PXFieldDescription]
		public virtual String LotSerClassID
		{
			get
			{
				return this._LotSerClassID;
			}
			set
			{
				this._LotSerClassID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region LotSerTrack
		public abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
		protected String _LotSerTrack;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INLotSerTrack.NotNumbered)]
		[PXUIField(DisplayName = "Tracking Method", Visibility = PXUIVisibility.SelectorVisible)]
		[INLotSerTrack.List()]
		public virtual String LotSerTrack
		{
			get
			{
				return this._LotSerTrack;
			}
			set
			{
				this._LotSerTrack = value;
			}
		}
		#endregion
		#region LotSerAssign
		public abstract class lotSerAssign : PX.Data.BQL.BqlString.Field<lotSerAssign> { }
		protected String _LotSerAssign;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INLotSerAssign.WhenReceived)]
		[PXUIField(DisplayName = "Assignment Method", Visibility = PXUIVisibility.SelectorVisible)]
		[INLotSerAssign.List()]
		public virtual String LotSerAssign
		{
			get
			{
				return this._LotSerAssign;
			}
			set
			{
				this._LotSerAssign = value;
			}
		}
		#endregion
		#region LotSerIssueMethod
		public abstract class lotSerIssueMethod : PX.Data.BQL.BqlString.Field<lotSerIssueMethod> { }
		protected String _LotSerIssueMethod;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INLotSerIssueMethod.FIFO)]
		[PXUIField(DisplayName = "Issue Method", Visibility = PXUIVisibility.SelectorVisible)]
		[INLotSerIssueMethod.List()]
		public virtual String LotSerIssueMethod
		{
			get
			{
				return this._LotSerIssueMethod;
			}
			set
			{
				this._LotSerIssueMethod = value;
			}
		}
		#endregion
		#region LotSerNumShared
		public abstract class lotSerNumShared : PX.Data.BQL.BqlBool.Field<lotSerNumShared> { }
		protected Boolean? _LotSerNumShared;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Share Auto-Incremental Value Between All Class Items")]
		public virtual Boolean? LotSerNumShared
		{
			get
			{
				return this._LotSerNumShared;
			}
			set
			{
				this._LotSerNumShared = value;
			}
		}
		#endregion
		#region LotSerFormatStr
		public abstract class lotSerFormatStr : PX.Data.BQL.BqlString.Field<lotSerFormatStr> { }
		protected String _LotSerFormatStr;
		[PXDBString(60)]
		public virtual String LotSerFormatStr
		{
			get
			{
				return this._LotSerFormatStr;
			}
			set
			{
				this._LotSerFormatStr = value;
			}
		}
		#endregion
		#region LotSerTrackExpiration
		public abstract class lotSerTrackExpiration : PX.Data.BQL.BqlBool.Field<lotSerTrackExpiration> { }
		protected Boolean? _LotSerTrackExpiration;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Track Expiration Date")]
		public virtual Boolean? LotSerTrackExpiration
		{
			get
			{
				return this._LotSerTrackExpiration;
			}
			set
			{
				this._LotSerTrackExpiration = value;
			}
		}
		#endregion
		#region AutoNextNbr
		public abstract class autoNextNbr : PX.Data.BQL.BqlBool.Field<autoNextNbr> { }
		protected Boolean? _AutoNextNbr;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Auto-Generate Next Number")]
		public virtual Boolean? AutoNextNbr
		{
			get
			{
				return _AutoNextNbr;
			}
			set
			{
				_AutoNextNbr = value;
			}
		}
		#endregion
		#region AutoSerialMaxCount
		public abstract class autoSerialMaxCount : PX.Data.BQL.BqlInt.Field<autoSerialMaxCount> { }
		protected int? _AutoSerialMaxCount;
		[PXDBInt()]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Max. Auto-Generate Numbers")]
		public virtual int? AutoSerialMaxCount
		{
			get
			{
				return this._AutoSerialMaxCount;
			}
			set
			{
				this._AutoSerialMaxCount = value;
			}
		}
		#endregion
		#region RequiredForDropship
		public abstract class requiredForDropship : PX.Data.BQL.BqlBool.Field<requiredForDropship> { }
		protected Boolean? _RequiredForDropship;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Required for Drop-ship")]
		public virtual Boolean? RequiredForDropship
		{
			get
			{
				return _RequiredForDropship;
			}
			set
			{
				_RequiredForDropship = value;
			}
		}
		#endregion
		#region IsManualAssignRequired
		public abstract class isManualAssignRequired : IBqlField
		{
		}
		
		/// <summary>
		/// Lot/Serial number is not assigned automatically and requires user interaction.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Where<lotSerTrack, IsNotNull, And<lotSerTrack, NotEqual<INLotSerTrack.notNumbered>,
			And<Where<lotSerAssign, Equal<INLotSerAssign.whenReceived>, And<lotSerIssueMethod, Equal<INLotSerIssueMethod.userEnterable>,
				Or<lotSerAssign, Equal<INLotSerAssign.whenUsed>, And<autoNextNbr, NotEqual<True>>>>>>>>))]
		public virtual bool? IsManualAssignRequired
		{
			get;
			set;
		}
		#endregion

		/// <summary>
		/// Lot/Serial number is not assigned automatically and requires user interaction.
		/// </summary>
		[Obsolete(Common.Messages.FieldIsObsoleteAndWillBeRemoved2020R1 + " Please use IsManualAssignRequired instead.")]
		public bool IsUnassigned
		{
			get
			{
				return IsManualAssignRequired ?? false;
			}
		}

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(INLotSerClass.lotSerClassID),
			Selector = typeof(INLotSerClass.lotSerClassID))]
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
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
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
		[PXDBCreatedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXDBLastModifiedByID()]
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
		[PXDBLastModifiedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
	}

	public class INLotSerAssign
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(WhenReceived, Messages.WhenReceived),
					Pair(WhenUsed, Messages.WhenUsed),
				}) {}
		}

		public const string WhenReceived = "R";
		public const string WhenUsed = "U";

		public class whenReceived : PX.Data.BQL.BqlString.Constant<whenReceived>
		{
			public whenReceived() : base(WhenReceived) { ;}
		}

		public class whenUsed : PX.Data.BQL.BqlString.Constant<whenUsed>
		{
			public whenUsed() : base(WhenUsed) { ;}
		}
	}

	public class INLotSerTrack
	{
		[Flags]
		public enum Mode
		{
			None   = 0,
			Create = 1,
			Issue  = 2,
			Manual = 4
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(NotNumbered, Messages.NotNumbered),
					Pair(LotNumbered, Messages.LotNumbered),
					Pair(SerialNumbered, Messages.SerialNumbered),
				}) {}
		}

		public const string NotNumbered = "N";
		public const string LotNumbered = "L";
		public const string SerialNumbered = "S";

		public class notNumbered : PX.Data.BQL.BqlString.Constant<notNumbered>
		{
			public notNumbered() : base(NotNumbered) { ;}
		}

		public class lotNumbered : PX.Data.BQL.BqlString.Constant<lotNumbered>
		{
			public lotNumbered() : base(LotNumbered) { ;}
		}

		public class serialNumbered : PX.Data.BQL.BqlString.Constant<serialNumbered>
		{
			public serialNumbered() : base(SerialNumbered) { ;}
		}				
	}
	
	public class INLotSerIssueMethod
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(FIFO, Messages.FIFO),
					Pair(LIFO, Messages.LIFO),
					Pair(Sequential, Messages.Sequential),
					Pair(Expiration, Messages.Expiration),
					Pair(UserEnterable, Messages.UserEnterable),
				}) {}
		}

		public const string FIFO = "F";
		public const string LIFO = "L";
		public const string Sequential = "S";
		public const string Expiration = "E";
		public const string UserEnterable = "U";

		public class fIFO : PX.Data.BQL.BqlString.Constant<fIFO>
		{
			public fIFO() : base(FIFO) { ;}
		}

		public class lIFO : PX.Data.BQL.BqlString.Constant<lIFO>
		{
			public lIFO() : base(LIFO) { ;}
		}

		public class sequential : PX.Data.BQL.BqlString.Constant<sequential>
		{
			public sequential() : base(Sequential) { ;}
		}

		public class expiration : PX.Data.BQL.BqlString.Constant<expiration>
		{
			public expiration() : base(Expiration) { ;}
		}

		public class userEnterable : PX.Data.BQL.BqlString.Constant<userEnterable>
		{
			public userEnterable() : base(UserEnterable) { ;}
		}
	}
}
