using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.SM;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.CS;

namespace PX.Objects.AR
{	
	/// <summary>
	/// A projection DAC which represents a pre-defined <see cref="NotificationSetup">
	/// record in the Accounts Receivable module. The records of this type provide default 
	/// values for <see cref="NotificationSource"> mailing source settings</see> of customer 
	/// classes, which in turn provide default values for mailings configured for particular 
	/// customers. Entities of this type are edited on the Mailing Settings tab of the Accounts 
	/// Receivable Preferences (AR101000) form, which corresponds to the <see cref="ARSetupMaint"/> graph.
	/// </summary>
	[PXProjection(typeof(Select<NotificationSetup,
		Where<NotificationSetup.module, Equal<PXModule.ar>>>), Persistent = true)]
    [Serializable]
	[PXCacheName(Messages.ARNotification)]
	public partial class ARNotification : NotificationSetup
	{
		#region SetupID
		public new abstract class setupID : PX.Data.BQL.BqlGuid.Field<setupID> { }
		#endregion
		#region Module
		public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		/// <summary>
		/// The module to which the mailing source settings belong.
		/// This field is a part of the compound key of the record.
		/// </summary>
		/// <value>
		/// The value of this field is always equal to <see cref="PXModule.AR"/>.
		/// </value>
		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault(PXModule.AR)]
		public override string Module
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
		public new abstract class sourceCD : PX.Data.BQL.BqlString.Field<sourceCD> { }		
		/// <summary>
		/// The name of the source object of email notifications.
		/// This field is a part of the compound key of the record.
		/// </summary>
		/// <value>
		/// Defaults to <see cref="ARNotificationSource.Customer"/>.
		/// </value>
		[PXDefault(ARNotificationSource.Customer)]
		[PXDBString(10, IsKey = true, InputMask = "")]
		[PXCheckUnique]
		public override string SourceCD
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
		public new abstract class nBranchID : PX.Data.BQL.BqlInt.Field<nBranchID> { }
		#endregion
		#region NotificationCD
		public new abstract class notificationCD : PX.Data.BQL.BqlString.Field<notificationCD> { }
		/// <summary>
		/// The human-readable identifier of the mailing settings record.
		/// </summary>
		[PXDBString(30, IsKey = true, InputMask = "", IsUnicode = true)]
		[PXUIField(DisplayName = "Mailing ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXCheckUnique(typeof(NotificationSetup.module), typeof(NotificationSetup.sourceCD), typeof(NotificationSetup.nBranchID))]
		[PXDefault]
		public override string NotificationCD
		{
			get
			{
				return _NotificationCD;
			}
			set
			{
				_NotificationCD = value;
			}
		}
		#endregion
		#region ReportID
		public new abstract class reportID : PX.Data.BQL.BqlString.Field<reportID> { }
		/// <summary>
		/// The unique identifier of the report that should be
		/// attached to the notification email.
		/// </summary>
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Report")]
		[PXSelector(typeof(Search<SiteMap.screenID,
			Where<SiteMap.screenID, Like<PXModule.ar_>, And<SiteMap.url, Like<Common.urlReports>>>,
			OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
			Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
			DescriptionField = typeof(SiteMap.title))]
		public override String ReportID
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
		public new abstract class defaultPrinterID : PX.Data.BQL.BqlGuid.Field<defaultPrinterID> { }
		#endregion
		#region TemplateID
		public abstract class templateID : PX.Data.IBqlField { }
		#endregion
		#region Active
		public new abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion
	}

	public class ARNotificationSource
	{
		public const string Customer = "Customer";
		public class customer : PX.Data.BQL.BqlString.Constant<customer> { public customer() : base(Customer) { } }
	}
}
