using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Data;

public partial class Frames_MobileAuth : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var request = HttpContext.Current.Request;
        var locale = request.QueryString["locale"];
        if(!string.IsNullOrWhiteSpace(locale))
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(locale);
        var result = request.QueryString["result"];
        string login = "temp";
        if (PXDatabase.Companies.Length > 0)
        {
            string company =  PXDatabase.Companies[0];
            login += "@" + company;
        }
        using (new PXLoginScope(login, PXAccess.GetAdministratorRoles()))
        {
            if (string.Equals(result, "success", StringComparison.OrdinalIgnoreCase))
            {
                lblSuccess.Visible = true;
                imgSuccess.Visible = true;
                lblSuccess.Text =
                    PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.MobileAuthScreen
                        .AuthenticationSuccessfull);
            }

            if (string.Equals(result, "fail", StringComparison.OrdinalIgnoreCase))
            {
                lblFail.Visible = true;
                imgFail.Visible = true;
                lblFail.Text =
                    PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.MobileAuthScreen.AuthenticationFailed);
            }
        }
    }
}