<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MobileAuth.aspx.cs" Inherits="Frames_MobileAuth" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        body {
            margin: 0px;
        }

        .background {
            width: 100%;
            height: 100%;
            margin: 0;
            padding: 0;
        }

        .content {
            width: 200px;
            height: 150px;
            position: absolute;
            left: 0;
            right: 0;
            top: 0;
            bottom: 0;
            margin: auto;
            max-width: 100%;
            max-height: 100%;
            overflow: auto;
            text-align: center;
            vertical-align: central;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server" class="background">
        <div class="content">
            <asp:Image id="imgFail" runat="server" ImageUrl="~/Icons/auth_fail.png" Visible="false"/>
            <asp:Image id="imgSuccess" runat="server" ImageUrl="~/Icons/auth_success.png" Visible="false"/>
            <br />
            <asp:Label ID="lblFail" runat="server" Font-Size="Medium" Font-Bold="True" Visible="false"></asp:Label>
            <asp:Label ID="lblSuccess" runat="server" Font-Size="Medium" Font-Bold="True" Visible="false"></asp:Label>
        </div>
    </form>
</body>
</html>
