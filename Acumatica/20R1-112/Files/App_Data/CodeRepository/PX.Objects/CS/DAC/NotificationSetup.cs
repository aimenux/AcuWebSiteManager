using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using PX.Objects.SM;
using PX.SM;

namespace PX.Objects.CS
{
    [Serializable]
	[PXCacheName(Messages.NotificationSetup)]
	public partial class NotificationSetup : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<NotificationSetup>.By<setupID>
		{
			public static NotificationSetup Find(PXGraph graph, Guid? setupID) => FindBy(graph, setupID);
		}
		#endregion

		#region SetupID
		public abstract class setupID : PX.Data.BQL.BqlGuid.Field<setupID> { }
		protected Guid? _SetupID;
		[PXDBGuid(true, IsKey = true)]
		public virtual Guid? SetupID
		{
			get
			{
				return this._SetupID;
			}
			set
			{
				this._SetupID = value;
			}
		}
		#endregion
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		protected string _Module;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Module", Enabled = false)]
		public virtual string Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region SourceCD
		public abstract class sourceCD : PX.Data.BQL.BqlString.Field<sourceCD> { }
		protected string _SourceCD;
		[PXDBString(10, IsFixed = true, InputMask = "")]
		[PXUIField(DisplayName = "Source", Enabled = false)]		
		public virtual string SourceCD
		{
			get
			{
				return this._SourceCD;
			}
			set
			{
				this._SourceCD = value;
			}
		}
		#endregion
		#region NBranchID
		public abstract class nBranchID : PX.Data.BQL.BqlInt.Field<nBranchID> { }
		protected Int32? _NBranchID;
		//[PXExtraKey()]
		[GL.Branch(useDefaulting: false, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing)]
		//[PXCheckUnique(typeof(NotificationSetup.setupID), IgnoreNulls = false,
		//	Where = typeof(Where<NotificationSetup.notificationCD, Equal<Current<NotificationSetup.notificationCD>>>))]
		public virtual Int32? NBranchID
		{
			get
			{
				return this._NBranchID;
			}
			set
			{
				this._NBranchID = value;
			}
		}
		#endregion
		#region NotificationCD
		public abstract class notificationCD : PX.Data.BQL.BqlString.Field<notificationCD> { }
		protected string _NotificationCD;
		[PXDBString(30, InputMask="", IsUnicode = true)]
		[PXUIField(DisplayName = "Mailing ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXCheckUnique(typeof(NotificationSetup.module), typeof(NotificationSetup.sourceCD), typeof(NotificationSetup.nBranchID))]
		[PXDefault]
		public virtual string NotificationCD
		{
			get
			{
				return this._NotificationCD;
			}
			set
			{
				this._NotificationCD = value;
			}
		}
		#endregion
		#region EMailAccount
		public abstract class eMailAccountID : PX.Data.BQL.BqlInt.Field<eMailAccountID> { }
		protected Int32? _EMailAccountID;
		[PXDBInt]
		[PXUIField(DisplayName = "Default Email Account")]
		[PXSelector(typeof(EMailAccount.emailAccountID), DescriptionField = typeof(EMailAccount.address))]
		public virtual Int32? EMailAccountID { get; set; }
		#endregion
		#region ReportID
		public abstract class reportID : PX.Data.BQL.BqlString.Field<reportID> { }
		protected String _ReportID;
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Report ID", Visibility=PXUIVisibility.SelectorVisible )]
		public virtual String ReportID
		{
			get
			{
				return this._ReportID;
			}
			set
			{
				this._ReportID = value;
			}
		}
		#endregion
		#region DefaultPrinterID
		public abstract class defaultPrinterID : PX.Data.BQL.BqlGuid.Field<defaultPrinterID> { }
		protected Guid? _DefaultPrinterID;
		[PXPrinterSelector(DisplayName = "Default Printer", Visibility = PXUIVisibility.SelectorVisible)]
		[PXForeignReference(typeof(Field<defaultPrinterID>.IsRelatedTo<SMPrinter.printerID>))]
		public virtual Guid? DefaultPrinterID
		{
			get
			{
				return this._DefaultPrinterID;
			}
			set
			{
				this._DefaultPrinterID = value;
			}
		}
		#endregion

		#region NotificationID
		public abstract class notificationID : PX.Data.BQL.BqlInt.Field<notificationID> { }
		protected Int32? _NotificationID;
		[PXDBInt]
		[PXUIField(DisplayName = "Notification Template", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Notification.notificationID>), 
			SubstituteKey = typeof(Notification.name), 
			DescriptionField = typeof(Notification.name))]
		public virtual Int32? NotificationID
		{
			get 
			{ 
				return this._NotificationID;
			}
			set
			{
				this._NotificationID = value;
			}
		}
		#endregion

		#region Format
		public abstract class format : PX.Data.BQL.BqlString.Field<format> { }
		protected string _Format;
		[PXDefault(NotificationFormat.Html)]
		[PXDBString(255)]
		[PXUIField(DisplayName = "Format")]
		[NotificationFormat.List]
		[PXNotificationFormat(typeof(NotificationSetup.reportID), typeof(NotificationSetup.notificationID))]
		public virtual string Format
		{
			get
			{
				return this._Format;
			}
			set
			{
				this._Format = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected bool? _Active;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion

		#region ShipVia
		[PXDBString(15, IsUnicode = true)]
		public virtual String ShipVia { get; set; }
		public abstract class shipVia : PX.Data.BQL.BqlString.Field<shipVia> { }
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
	}

	public class NotificationFormat
	{
		public const string Html = "H";
		public const string Excel = "E";
		public const string PDF = "P";

		public class ListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues
			{
				get
				{
					return _AllowedValues;
				}
			}
			public string[] AllowedLabels
			{
				get
				{
					return _AllowedLabels;
				}
			}
			public ListAttribute()
				: base(new string[] { Html, Excel, PDF },
							new string[] { "Html", "Excel", "PDF" })
			{
			}
			protected ListAttribute(string[] values, string[] labels)
				: base(values, labels)
			{
			}
		}
		public class ReportListAttribute : ListAttribute
		{
			public ReportListAttribute()
				: base(new string[] { Html, Excel, PDF },
							new string[] { "Html", "Excel", "PDF" })
			{
			}
		}
		public class TemplateListAttribute : ListAttribute
		{
			public TemplateListAttribute()
				: base(new string[] {  Html },
							new string[] { "Html" })
			{
			}
		}

		public static ListAttribute List = new ListAttribute();
		public static ListAttribute ReportList = new ReportListAttribute();
		public static ListAttribute TemplateList = new TemplateListAttribute();
	}			
}
