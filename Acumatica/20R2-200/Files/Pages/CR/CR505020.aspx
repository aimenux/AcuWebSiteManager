<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR505020.aspx.cs" Inherits="Page_CR505020" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.MergePrepareProcess"
		PrimaryView="Items">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveClose" Visible="false" PopupVisible="true"
				ClosePopup="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Process" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="viewDetails" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grdItems" runat="server" DataSourceID="ds" SyncPosition="True" SkinID="Inquire"
		Width="100%">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>
					<px:PXGridColumn DataField="Selected" AllowCheckAll="True" Type="CheckBox" TextAlign="Center"
						Width="30px" AllowShowHide="False">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="MergeCD" LinkCommand="viewDetails">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Description">
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="cmdViewDetails" PagerVisible="False">
			<CustomItems>
				<px:PXToolBarButton Text="View Details" Tooltip="Shows Time Card Details" Key="cmdViewDetails"
					Visible="false">
				    <AutoCallBack Command="viewDetails" Target="ds" Enabled="True">
						<Behavior CommitChanges="True" />
					</AutoCallBack>
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<AutoSize Enabled="True" Container="Window"></AutoSize>
	</px:PXGrid>
</asp:Content>
