<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Error.aspx.cs" Inherits="Frames_Error"
    ValidateRequest="false" EnableViewState="false" EnableEventValidation="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Error Has Occurred</title>
    <meta http-equiv="content-script-type" content="text/javascript" />
    <style type="text/css">
        .main
        {
            padding-left: 40px;
            padding-right: 20px;
            padding-top: 30px;
            font-family: Arial;
        }
        .errCode
        {
            padding-bottom: 20px;
            font-family: Arial;
            font-size: 15pt;
        }
        .errMsg
        {
            font-size: 12pt;
        }
        .img
        {
            float: left;
            margin-right: 10px;
        }
        .nxtSt
        {
            margin-top: 30px;
            font-family: Arial;
            font-size: 15pt;
        }
        .navTo
        {
            margin-top: 10px;
            margin-left: 20px;
        }
        .errPnl
        {
            padding: 10px;
            padding-top: 15px;
        }
        .grayBox
        {
            border: solid 1px #CCC;
            background-color: #F9F9F9;
            padding-top: 20px;
            padding-bottom: 25px;
            padding-left: 10px;
            padding-right: 20px;
        }
        .traceLnk
        {
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div class="main">
        <px:PXFormView ID="frmBottom" runat="server" SkinID="Transparent">
            <Template>
                <div class="errCode">
                    <px:PXLabel ID="lblErrCode" runat="server" Text="Error code" CssClass="errCode" Encode="False"></px:PXLabel>
                </div>
                <div class="grayBox">
                    <div class="img">
                        <asp:Image ID="imgMessage" runat="server" ImageUrl="~/App_Themes/Default/Images/Message/error2.gif" />
                    </div>
                    <div class="errMsg">
                        <px:PXLabel ID="lblMessage" CssClass="errMsg" runat="server" Encode="False" Text="We're sorry! An error has occurred while processing your request. A report has been generated for our technical staff to investigate the problem. Please try to repeat your request later. Thank you for understanding."></px:PXLabel>
                    </div>
                </div>
                <div class="traceLnk">
                    <asp:HyperLink ID="lnkTrace" runat="server" Font-Size="Medium" Font-Underline="True"
                        ForeColor="Blue" NavigateUrl="~/Frames/Trace.aspx" Text="Show Trace"></asp:HyperLink>
                </div>
                <div class="nxtSt">
                    <px:PXLabel ID="lblNxStep" runat="server" CssClass="nxtSt" Text="Next Step:"></px:PXLabel>
                </div>
                <div class="navTo">
                    <px:PXLabel ID="lblNavTo" runat="server" Font-Size="Medium" Text="Navigate to "></px:PXLabel>
                    <asp:HyperLink ID="hlNavTo" runat="server" NavigateUrl="~/Frames/Trace.aspx" Text="Show Trace"
                        Font-Size="Medium"></asp:HyperLink>
                    <px:PXLabel ID="lblNavToEnding" runat="server" Font-Size="Medium" Text=" screen and enter required configuration data."></px:PXLabel>
                </div>
				<asp:Button ID="btnLogout" runat="server" Text="Sign Out" OnClick="Logout" />
            </Template>
            <AutoSize Enabled="True" Container="Window" />
        </px:PXFormView>
    </div>
    </form>
</body>
</html>
