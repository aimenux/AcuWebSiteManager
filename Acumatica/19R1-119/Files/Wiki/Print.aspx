<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Print.aspx.cs" Inherits="Wiki_Print"
	EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>FormView Page</title>
</head>
<body style="background-color: White" onload="window.print();">
	<form id="form1" runat="server">
	<px:PXDataSource ID="ds" runat="server" Visible="False" Width="100%" TypeName="PX.SM.WikiShowReader"
		PrimaryView="Pages">
	</px:PXDataSource>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100;"
		Width="100%" DataMember="Pages" DataKeyNames="PageID" SkinID="Transparent" CheckChanges="False" OverflowX="Visible" OverflowY="Visible">
		<Searches>
			<px:PXQueryStringParam Name="PageID" QueryStringField="PageID" Type="String" />
			<px:PXQueryStringParam Name="Language" QueryStringField="Language" Type="String" />
			<px:PXQueryStringParam Name="PageRevisionID" QueryStringField="PageRevisionID" Type="Int32" />
			<px:PXQueryStringParam Name="Wiki" QueryStringField="Wiki" Type="String" />
			<px:PXQueryStringParam Name="Art" QueryStringField="Art" Type="String" />
			<px:PXQueryStringParam Name="Parent" QueryStringField="From" Type="String" />
			<px:PXControlParam ControlID="form" Name="PageID" PropertyName="NewDataKey[&quot;PageID&quot;]"
				Type="String" />
		</Searches>
	</px:PXFormView>
	</form>
</body>
</html>
