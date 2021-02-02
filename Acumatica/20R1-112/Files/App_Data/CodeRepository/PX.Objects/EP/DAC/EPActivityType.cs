using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.EP
{
	[PXCacheName(Messages.ActivityType)]
	[PXPrimaryGraph(typeof(CRActivitySetupMaint))]
	[Serializable]
	public partial class EPActivityType : IBqlTable, PX.Data.EP.ActivityService.IActivityType
	{
		#region Type

		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

		[PXDBString(5, IsFixed = true, IsKey = true, InputMask = ">AAAAA")]
		[PXDefault]
		[PXUIField(DisplayName = "Type ID")]
		public virtual String Type { get; set; }

		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXDBLocalizableString(64, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXDefault]
		public virtual string Description { get; set; }
		#endregion

		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active { get; set; }
		#endregion

		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "System Default")]
		public virtual bool? IsDefault { get; set; }
		#endregion

		#region ImageUrl

		public abstract class imageUrl : PX.Data.BQL.BqlString.Field<imageUrl> { }

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Image")]
		[PXIconsList]
		public virtual String ImageUrl { get; set; }

		#endregion

		#region Application
		public abstract class application : PX.Data.BQL.BqlInt.Field<application> { }
		[PXInt]
		[PXUIField(DisplayName = "Application")]
		[PXActivityApplication]
		[PXDefault(PXActivityApplicationAttribute.Backend)]
		[PXDBCalced(typeof(Switch<Case<Where<EPActivityType.isInternal, Equal<True>>, PXActivityApplicationAttribute.backend>, PXActivityApplicationAttribute.portal>), typeof(int))]
		public virtual int? Application { get; set; }
		#endregion

		#region IsInternal
		public abstract class isInternal : PX.Data.BQL.BqlBool.Field<isInternal> { }
		[PXDBBool]				
		[PXDefault(true)]
		[PXFormula(typeof(Switch<Case<Where<EPActivityType.application, Equal<PXActivityApplicationAttribute.backend>>, True>,False>)) ]
		public virtual bool? IsInternal { get; set; }
		#endregion

		#region PrivateByDefault
		public abstract class privateByDefault : PX.Data.BQL.BqlBool.Field<privateByDefault> { }
		[PXDBBool]		
		[PXFormula(typeof(Switch<Case<Where<EPActivityType.application, Equal<PXActivityApplicationAttribute.portal>>, False>, EPActivityType.privateByDefault>))]
		[PXUIEnabled(typeof(EPActivityType.isInternal))]
		[PXUIField(DisplayName = "Internal")]
		public virtual Boolean? PrivateByDefault { get; set; }
		#endregion

		#region RequireTimeByDefault


		public abstract class requireTimeByDefault : PX.Data.BQL.BqlBool.Field<requireTimeByDefault> { }
		[PXDefault(false,PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBBool]
		[PXUIField(DisplayName = "Track Time")]
		[PXFormula(typeof(Switch<Case<Where<EPActivityType.application, Equal<PXActivityApplicationAttribute.portal>>, False>, EPActivityType.requireTimeByDefault>))]
		[PXUIEnabled(typeof(EPActivityType.isInternal))]
		public virtual Boolean? RequireTimeByDefault { get; set; }

		#endregion

		#region Incoming
		public abstract class incoming : PX.Data.BQL.BqlBool.Field<incoming> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Incoming")]
		public virtual bool? Incoming { get; set; }
		#endregion

		#region Outgoing
		public abstract class outgoing : PX.Data.BQL.BqlBool.Field<outgoing> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Outgoing")]
		public virtual bool? Outgoing { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(new Type[0])]
		public virtual Guid? NoteID { get; set; }
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
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = "Last Activity", Enabled = false)]
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

		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }

		[PXDBInt]
		[CRActivityClass]
		[PXDefault(typeof(CRActivityClass.activity))]
		[PXUIField(DisplayName = "Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual int? ClassID { get; set; }
		#endregion

		#region IsSystem
		public abstract class isSystem : PX.Data.BQL.BqlBool.Field<isSystem> { }
		[PXBool]
		[PXUIField(DisplayName = "Is System")]
		[PXDBCalced(typeof(IIf<Where<EPActivityType.classID, Equal<CRActivityClass.task>, Or<EPActivityType.classID, Equal<CRActivityClass.email>, Or<EPActivityType.classID, Equal<CRActivityClass.events>>>>, True, False>), typeof(bool))]
		public virtual bool? IsSystem { get; set; }
		#endregion
	}
}
