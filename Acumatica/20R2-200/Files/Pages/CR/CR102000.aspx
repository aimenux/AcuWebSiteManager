<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CR102000.aspx.cs" Inherits="Page_CR102000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="ActivityTypes" TypeName="PX.Objects.CR.CRActivitySetupMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary" TabIndex="1900">
		<Levels>
			<px:PXGridLevel DataKeyNames="Type" DataMember="ActivityTypes">
                <RowTemplate>
                    <px:PXDropDown ID="edImageUrl" runat="server" DataField="ImageUrl" AllowEdit="true" /> 
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="ClassID" />
                    <px:PXGridColumn DataField="Type" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="Active" Type="CheckBox" TextAlign="Center" />
                    <px:PXGridColumn DataField="IsDefault" Type="CheckBox" TextAlign="Center" />
                    <px:PXGridColumn DataField="Application" CommitChanges="true" />
                    <px:PXGridColumn DataField="ImageUrl" />
                    <px:PXGridColumn DataField="PrivateByDefault" Type="CheckBox" TextAlign="Center" CommitChanges="true" />
                    <px:PXGridColumn DataField="RequireTimeByDefault" Type="CheckBox" TextAlign="Center" CommitChanges="true" />
                    <px:PXGridColumn DataField="Incoming" Type="CheckBox" TextAlign="Center" />
                    <px:PXGridColumn DataField="Outgoing" Type="CheckBox" TextAlign="Center" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
