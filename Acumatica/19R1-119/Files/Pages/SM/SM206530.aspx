<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM206530.aspx.cs" Inherits="Page_SM206530"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Scale"
		TypeName="PX.SM.ScaleMaint">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:content id="cont2" contentplaceholderid="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Height="63px" Width="100%" Visible="true" DataMember="Scale">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />
            <px:PXSelector ID="edScaleID" runat="server" DataField="ScaleID"/>
            <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="XL" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Descr"/>
            <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="S" />
            <px:PXNumberEdit ID="edLastWeight" runat="server" DataField="LastWeight"/>
            <px:PXDateTimeEdit ID="edLastModifiedDateTime" runat="server" DataField="LastModifiedDateTime" DisplayFormat="g" Enabled="False" Size="XL"/>
        </Template>
        <AutoSize Container="Window" Enabled="True" />
    </px:PXFormView>
</asp:content>