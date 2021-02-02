<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL202500.aspx.cs" Inherits="Page_GL202500"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="AccountRecords" TypeName="PX.Objects.GL.AccountMaint"  Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewRestrictionGroups" />
		    <px:PXDSCallbackCommand Name="AccountByPeriodEnq" DependOnGrid="grid" Visible="false"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="350px" Width="100%" AdjustPageSize="Auto"
		SkinID="Primary" AllowPaging="True" AllowSearch="True" FastFilterFields="AccountCD,Description" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="AccountRecords">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXSegmentMask ID="edAccountCD" runat="server" DataField="AccountCD" />
					<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
					<px:PXSelector ID="edAccountClassID" runat="server" DataField="AccountClassID" AllowEdit="True" AutoCallback="True"/>
				    <px:PXDropDown ID="Type" runat="server" DataField="Type"/>
					<px:PXDropDown ID="PostOption" runat="server" DataField="PostOption">
					</px:PXDropDown>
					<px:PXDropDown ID="ControlAccountModule" runat="server" DataField="ControlAccountModule" CommitChanges="true" />
					<px:PXCheckBox ID="chkAllowManualEntry" runat="server" DataField="AllowManualEntry" CommitChanges="true" />
					<px:PXCheckBox ID="chkRequireUnits" runat="server" DataField="RequireUnits" />
					<px:PXCheckBox ID="chkIsCashAccount" runat="server" DataField="IsCashAccount" />
					<px:PXCheckBox ID="chkSecured" runat="server" DataField="Secured" Enabled="False" />
					<px:PXLayoutRule runat="server" ColumnSpan="2" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" AllowEdit="True" CommitChanges="true" />
					<px:PXSelector ID="edRevalCuryRateTypeId" runat="server" DataField="RevalCuryRateTypeId"
						AllowEdit="True" CommitChanges="true" />
					<px:PXSelector ID="edGLConsolAccountCD" runat="server" DataField="GLConsolAccountCD" />
					<px:PXSegmentMask ID="edAccountGroupID" runat="server" DataField="AccountGroupID"
						AutoRefresh="True" AllowEdit="True" CommitChanges="true" />
                    <px:PXSelector ID="edTaxCategory" runat="server" DataField="TaxCategoryID" />
					<px:PXNumberEdit ID="edAccountID" runat="server" DataField="AccountID" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="AccountID" TextAlign="Right" />
					<px:PXGridColumn DataField="AccountCD" />
					<px:PXGridColumn DataField="AccountClassID" AutoCallBack="True"/>
					<px:PXGridColumn DataField="Type" Type="DropDownList" />
					<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="Description" />
					<px:PXGridColumn DataField="ControlAccountModule" CommitChanges="true" />
					<px:PXGridColumn DataField="AllowManualEntry" Type="CheckBox" TextAlign="Center" CommitChanges="true" />
					<px:PXGridColumn DataField="PostOption" Type="DropDownList" />
					<px:PXGridColumn DataField="COAOrder" Width="54px" TextAlign="Right" />
					<px:PXGridColumn DataField="IsCashAccount" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="CuryID" AutoCallBack="True" />
					<px:PXGridColumn DataField="RevalCuryRateTypeId" />
					<px:PXGridColumn DataField="GLConsolAccountCD" />
					<px:PXGridColumn DataField="AccountGroupID" CommitChanges="true" />
					<px:PXGridColumn DataField="TaxCategoryID" />
					<px:PXGridColumn DataField="NoSubDetail" Type="CheckBox" TextAlign="Center" />
					<px:PXGridColumn DataField="RequireUnits" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="Secured" TextAlign="Center" Type="CheckBox" />
				</Columns>
				<Styles>
					<RowForm Height="250px">
					</RowForm>
				</Styles>
			</px:PXGridLevel>
		</Levels>
		<Layout FormViewHeight="250px" />
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowFormEdit="True" AllowUpload="True" />
		<CallbackCommands>
			<Save PostData="Content" />
		</CallbackCommands>
	</px:PXGrid>
	<px:PXSmartPanel ID="smpCashAccountED" runat="server" Key="CashAccountEditor" InnerPageUrl="~/Pages/CA/CA202000.aspx?PopupPanel=On"
		CaptionVisible="true" Caption="Cash Account" RenderIFrame="True">
	</px:PXSmartPanel>
</asp:Content>
