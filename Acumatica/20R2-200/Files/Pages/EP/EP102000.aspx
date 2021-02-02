<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP102000.aspx.cs" Inherits="Page_EP102000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="EarningTypes" TypeName="PX.Objects.EP.EPEarningTypesSetup" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary" TabIndex="5100">
		<Levels>
			<px:PXGridLevel DataKeyNames="Type" DataMember="EarningTypes">
                <RowTemplate>
                    <px:PXNumberEdit ID="edOvertimeMultiplier" runat="server" DataField="OvertimeMultiplier" />
                </RowTemplate>
			    <Columns>
			        <px:PXGridColumn DataField="TypeCD" AutoCallBack="True"/>
			        <px:PXGridColumn DataField="Description"/>
                    <px:PXGridColumn DataField="isActive" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
			        <px:PXGridColumn DataField="isOvertime" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
			        <px:PXGridColumn DataField="OvertimeMultiplier" TextAlign="Right" CommitChanges="True"/>
			        <px:PXGridColumn DataField="isBillable" TextAlign="Center" Type="CheckBox"/>
			        <px:PXGridColumn DataField="ProjectID" AutoCallBack="True"/>
			        <px:PXGridColumn DataField="TaskID"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
