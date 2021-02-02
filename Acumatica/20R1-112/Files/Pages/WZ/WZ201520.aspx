<%@ Page Language="C#" AutoEventWireup="true" CodeFile="WZ201520.aspx.cs" Inherits="Page_WZ201520"
    ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Scenario is not active</title>
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
        .activateBtn {
            margin-top: 10px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div class="main">
        
        <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.WZ.WZSetupMaint" PrimaryView="Setup"/>
        
        <px:PXFormView ID="frmBottom" runat="server" SkinID="Transparent" DataMember="Setup" DataSourceID="ds">
            <Template>
                <div class="errCode">
                    <px:PXLabel ID="lblErrCodeEnding" runat="server" Text=" Welcome to Acumatica ERP!" CssClass="errCode"></px:PXLabel>
                </div>
                
                <div>
	                <p style="width: 60%; line-height: 160%; text-indent: 0.5em;">You have installed a new blank instance of Acumatica ERP. You can activate the implementation scenarios, which will guide you through the process of implementing the Finance suite, or you can proceed with implementation on your own.</p>
                    <br/>
					<asp:Button ID="enableBtn" runat="server" OnClick="enableBtn_OnClick" Text="Enable Implementation scenarios" ></asp:Button>
                    <px:PXLabel ID="lbl1"  runat="server" Text="Yes, activate the implementation scenarios"></px:PXLabel>
	                <br/><br/>
					<asp:Button ID="disableBtn" runat="server" OnClick="disableBtn_OnClick" Text="Disable Implementation scenarios" ></asp:Button>
                    <px:PXLabel ID="lbl2" runat="server" Text="No thanks. I do not need to use the implementation scenarios"></px:PXLabel>
					
                </div>        
            </Template>
            <AutoSize Enabled="True" Container="Window" />
        </px:PXFormView>
    </div>
    </form>
</body>
</html>
