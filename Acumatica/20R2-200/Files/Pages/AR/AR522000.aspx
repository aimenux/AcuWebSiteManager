<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR522000.aspx.cs" Inherits="Page_AR522000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AR.ARDunningLetterPrint" >
        <CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" Name="ViewDocument" CommitChanges="true" DependOnGrid="grid" />
		</CallbackCommands>
      </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" Caption="Selection" DataMember="Filter">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDropDown CommitChanges="True" ID="edAction" runat="server" AllowNull="False" DataField="Action" SelectedIndex="-1" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edBeginDate" runat="server" DataField="BeginDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXCheckBox CommitChanges="True" ID="chkShowAll" runat="server" DataField="ShowAll" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXCheckBox CommitChanges="True" ID="chkPrintWithDeviceHub" runat="server" DataField="PrintWithDeviceHub" AlignLeft="true" />
            <px:PXCheckBox CommitChanges="True" ID="chkDefinePrinterManually" runat="server" DataField="DefinePrinterManually" AlignLeft="true" />
            <px:PXSelector CommitChanges="True" ID="edPrinterID" runat="server" DataField="PrinterID" />
			<px:PXTextEdit CommitChanges="true" ID="edNumberOfCopies" runat="server" DataField="NumberOfCopies" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" Height="150px" SkinID="Details" Caption="Dunning Letters" FastFilterFields="CustomerID">
        <Levels>
            <px:PXGridLevel DataKeyNames="DunningLetterID" DataMember="Details">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="True" AllowCheckAll="True" />
                    <px:PXGridColumn AllowNull="False" DataField="BranchID" TextAlign="Left" />
                    <px:PXGridColumn DataField="CustomerID" DisplayMode="Hint"/>
                    <px:PXGridColumn AllowNull="False" DataField="DunningLetterDate" />
                    <px:PXGridColumn DataField="DunningLetterLevel" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="DocBal" TextAlign="Right" />
                    <px:PXGridColumn DataField="LastLevel" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowNull="False" DataField="DontPrint" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowNull="False" DataField="Printed" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowNull="False" DataField="DontEmail" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowNull="False" DataField="Emailed" TextAlign="Center" Type="CheckBox" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <ActionBar DefaultAction="ViewDocument" >
			<CustomItems>
                   <px:PXToolBarButton Text="View Dunning Letter" Key="cmdViewDocument">
					    <AutoCallBack Command="ViewDocument" Target="ds" />
				    </px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
    </px:PXGrid>
</asp:Content>
