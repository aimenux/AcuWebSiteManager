<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS100000.aspx.cs" Inherits="Page_CS100000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
		PrimaryView="Features" TypeName="PX.Objects.CS.FeaturesMaint" PageLoadBehavior="GoFirstRecord">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />			
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Features" 
		EmailingGraph="" Caption="General Settings" TemplateContainer="" TabIndex="500">
        <Activity Width="" Height=""></Activity>
		<Template>
		    <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M"  />            		    
		    <px:PXDropDown ID="Status" runat="server" DataField="Status" Enabled="False" />                        
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit runat="server" ID="edLicnseID" DataField="LicenseID" Enabled="False" ></px:PXTextEdit>
            <px:PXLayoutRule runat="server" StartColumn="True"/>
            <px:PXDateTimeEdit runat="server" ID="edValidUntill" DataField="ValidUntill" Enabled="False"></px:PXDateTimeEdit>
            <px:PXLayoutRule runat="server" StartRow="True"/>
		    <px:PXLayoutRule GroupCaption=" " runat="server" StartRow="True" StartColumn="True" StartGroup="True" ControlSize="L" LabelsWidth="xxs"/>            		    
		</Template>
	    <AutoSize Container="Window" Enabled="true"/>
	</px:PXFormView>
</asp:Content>
