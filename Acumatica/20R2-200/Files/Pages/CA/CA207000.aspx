<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA207000.aspx.cs" Inherits="Page_CA507000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="filter" TypeName="PX.Objects.CA.PaymentMethodConverter">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" RepaintControls="All" />
			<px:PXDSCallbackCommand Name="Process" StartNewGroup="true" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="filter" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" LabelsWidth="M" StartColumn="True">
			</px:PXLayoutRule>
			<px:PXSelector ID="edOldPaymentMethodID" runat="server" DataField="OldPaymentMethodID" CommitChanges="True">
			</px:PXSelector>
			<px:PXSelector ID="edOldCCProcessingCenterID" runat="server" DataField="OldCCProcessingCenterID" CommitChanges="True" AutoRefresh="True">
			</px:PXSelector>
			<px:PXSelector ID="edNewCCProcessingCenterID" runat="server" DataField="NewCCProcessingCenterID" CommitChanges="True" AutoRefresh="True">
			</px:PXSelector>
			<px:PXLayoutRule runat="server" StartRow="true" ColumnWidth="160" LabelsWidth="M"  />
			<px:PXCheckBox ID="edDeleteExpiredCards" runat="server" AlreadyLocalized="False" DataField="ProcessExpiredCards" CommitChanges="True" AlignLeft="true"></px:PXCheckBox>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="Inquire" TabIndex="1000">
		<Levels>
			<px:PXGridLevel DataKeyNames="BAccountID,PMInstanceID" DataMember="CustomerPaymentMethodList">
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" >
						<ValueItems MultiSelect="False">
						</ValueItems>
					</px:PXGridColumn>
					<px:PXGridColumn DataField="PaymentMethodID" >
						<ValueItems MultiSelect="False">
						</ValueItems>
					</px:PXGridColumn>
					<px:PXGridColumn DataField="BAccountID" Width="200px">
						<ValueItems MultiSelect="False">
						</ValueItems>
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Descr" Width="200px">
						<ValueItems MultiSelect="False">
						</ValueItems>
					</px:PXGridColumn>
					<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px">
						<ValueItems MultiSelect="False">
						</ValueItems>
					</px:PXGridColumn>
					<px:PXGridColumn DataField="CCProcessingCenterID">
						<ValueItems MultiSelect="False">
						</ValueItems>
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar ActionsText="False">
		</ActionBar>
		<Mode AllowAddNew="False" />
	</px:PXGrid>
</asp:Content>
