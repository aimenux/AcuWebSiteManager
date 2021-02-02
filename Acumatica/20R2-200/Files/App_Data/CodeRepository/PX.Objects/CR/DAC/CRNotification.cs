using PX.Data;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR.DAC
{
    /// <summary>
	/// A projection DAC which represents a pre-defined <see cref="NotificationSetup">
	/// record in the CRM module. The records of this type provide default 
	/// values for <see cref="NotificationSource"> mailing source settings</see> of bussness account 
	/// classes, which in turn provide default values for mailings configured for particular 
	/// bussness account. Entities of this type are edited on the Mailing Settings tab of the Customer 
	/// Managment Preferences (CR101000) form, which corresponds to the <see cref="CRSetupMaint"/> graph.
	/// </summary>
	[PXProjection(typeof(Select<NotificationSetup,
        Where<NotificationSetup.module, Equal<PXModule.cr>>>), Persistent = true)]
    [Serializable]
    [PXCacheName(Messages.CRNotification)]
    public partial class CRNotification : NotificationSetup
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
        /// The value of this field is always equal to <see cref="PXModule.CR"/>.
        /// </value>
        [PXDBString(2, IsFixed = true, IsKey = true)]
        [PXDefault(PXModule.CR)]
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
        /// Defaults to <see cref="CRNotificationSource.BAccount"/>.
        /// </value>
        [PXDefault(CRNotificationSource.BAccount)]
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
        #region NotificationCD
        public new abstract class notificationCD : PX.Data.BQL.BqlString.Field<notificationCD> { }
        /// <summary>
        /// The human-readable identifier of the mailing settings record.
        /// </summary>
        [PXDBString(30, IsKey = true, InputMask = "", IsUnicode = true)]
        [PXUIField(DisplayName = "Mailing ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXCheckUnique(typeof(NotificationSetup.module), typeof(NotificationSetup.sourceCD))]
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
            Where<SiteMap.screenID, Like<PXModule.cr_>, And<SiteMap.url, Like<urlReports>>>,
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
        #region TemplateID
        public abstract class templateID : PX.Data.IBqlField { }
        #endregion
        #region Active
        public new abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
        #endregion
    }

    public class CRNotificationSource
    {
        public const string BAccount = "BAccount";
        public class bAccount : PX.Data.BQL.BqlString.Constant<bAccount> { public bAccount() : base(BAccount) { } }
    }
}
