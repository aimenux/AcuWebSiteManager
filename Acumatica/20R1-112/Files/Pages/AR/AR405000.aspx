<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR405000.aspx.cs" Inherits="Page_AR405000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues" TypeName="PX.Objects.AR.ARExpiringCardsEnq">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="ViewCustomer" DependOnGrid="grid" Visible="False"
				CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ViewPaymentMethod" DependOnGrid="grid" Visible="False"
				CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection">
		<Template>
			 
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />

			 
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Expiration Period" />

			<px:PXDateTimeEdit CommitChanges="True" ID="edBeginDate" runat="server" DataField="BeginDate"  />
			<px:PXNumberEdit CommitChanges="True" ID="edExpireXDays" runat="server" AllowNull="False" DataField="ExpireXDays" />
			 
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXLabel runat="server"></px:PXLabel>
			<px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID"  />			
			<px:PXCheckBox CommitChanges="True" ID="chkActiveOnly" runat="server" DataField="ActiveOnly" /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Card List" AllowSearch="True" AllowPaging="True" SkinID="Inquire" RestrictFields="True">
		<Levels>
			<px:PXGridLevel  DataMember="Cards">
				<Columns>
					<px:PXGridColumn DataField="BAccountID" DisplayFormat="&gt;AAAAAAAAAA" />
					<px:PXGridColumn AllowUpdate="False" DataField="Customer__AcctName" />
					<px:PXGridColumn AllowUpdate="False" DataField="Customer__CustomerClassID" DisplayFormat="&gt;aaaaaaaaaa"  />
					<px:PXGridColumn DataField="PaymentMethodID" DisplayFormat="&gt;aaaaaaaaaa"  />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Descr" />
					<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn AllowUpdate="False" DataField="ExpirationDate" />
					<px:PXGridColumn AllowUpdate="False" DataField="Contact__EMail" />
					<px:PXGridColumn AllowUpdate="False" DataField="Contact__Phone1" DisplayFormat="CCCCCCCCCCCCCCCCCCCC" />
					<px:PXGridColumn AllowUpdate="False" DataField="Contact__Fax" DisplayFormat="CCCCCCCCCCCCCCCCCCCC" />
					<px:PXGridColumn AllowUpdate="False" DataField="Contact__WebSite" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<Actions>
				<Save Enabled="False" />
			</Actions>
			<CustomItems>
				<px:PXToolBarButton Text="View Customer" Key="cmdViewCustomer">
				    <AutoCallBack Command="ViewCustomer" Target="ds" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="View Payment Method" Key="cmdViewPaymentMethod">
				    <AutoCallBack Command="ViewPaymentMethod" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
