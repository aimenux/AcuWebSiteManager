<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM203000.aspx.cs" Inherits="Page_AM203000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="AMMPSTypeRecords" TypeName="PX.Objects.AM.MPSType" Visible="true">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Width="100%" 
		AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary" TabIndex="2100"> 
		<Levels>
			<px:PXGridLevel DataMember="AMMPSTypeRecords" DataKeyNames="MPSTypeID">
                <RowTemplate>
                    <px:PXMaskEdit ID="edMPSTypeID" runat="server" DataField="MPSTypeID"/>
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr"/>
                    <px:PXSelector ID="edMPSNumberingID" runat="server" DataField="MPSNumberingID" AllowEdit="True"/>
                    <px:PXCheckBox ID="edDependent" runat="server" DataField="Dependent"/>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="MPSTypeID" Width="100px"/>
                    <px:PXGridColumn DataField="Descr" Width="200px"/>
                    <px:PXGridColumn DataField="MPSNumberingID" Width="100px"/>
                    <px:PXGridColumn DataField="Dependent" TextAlign="Center" Type="CheckBox" Width="90px"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
        <AutoCallBack Command="save" Target="grid">
        </AutoCallBack>
        <Mode InitNewRow="True" />
	</px:PXGrid>
</asp:Content>
