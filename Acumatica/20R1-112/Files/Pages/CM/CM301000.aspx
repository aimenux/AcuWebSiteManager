<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CM301000.aspx.cs" Inherits="Page_CM301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource  ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.CM.CuryRateMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand PostData="Self" Name="First" />
			<px:PXDSCallbackCommand PostData="Self" Name="Prev" />
			<px:PXDSCallbackCommand PostData="Self" Name="Next" />
			<px:PXDSCallbackCommand PostData="Self" Name="Last" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server"  Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Currency Selection" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXSelector CommitChanges="True" ID="edToCurrency" runat="server" DataField="ToCurrency" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edEffDate" runat="server" DataField="EffDate" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab1" runat="server" Width="100%" DataMember="Filter"  Height="320px">
		<Items>
			<px:PXTabItem Text="Currency Rate Entry">
				<Template>
					<px:PXGrid ID="grid" runat="server"  Style="z-index: 100;" Width="100%" Height="220px" AllowPaging="True" Caption="Rate Details" ActionsPosition="top" AllowSearch="True" SkinID="DetailsInTab"
						AdjustPageSize="Auto">
						<Levels>
							<px:PXGridLevel DataMember="CuryRateRecordsEntry">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSelector ID="edFromCuryID" runat="server" DataField="FromCuryID" AutoRefresh="true" />
									<px:PXSelector ID="edCuryRateType" runat="server" DataField="CuryRateType" />
									<px:PXDateTimeEdit ID="edCuryEffDate" runat="server" DataField="CuryEffDate" />
									<px:PXNumberEdit ID="edCuryRate" runat="server" DataField="CuryRate" />
									<px:PXNumberEdit ID="edRateReciprocal" runat="server" DataField="RateReciprocal" />
									<px:PXNumberEdit ID="edCuryRateID" runat="server" DataField="CuryRateID" Visible="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="FromCuryID" TextCase="Upper" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryRateType" TextCase="Upper" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryEffDate" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryRate" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryMultDiv" Type="DropDownList" />
									<px:PXGridColumn DataField="RateReciprocal" TextAlign="Right" AllowUpdate="False" />
									<px:PXGridColumn AllowShowHide="False" DataField="CuryRateID" TextAlign="Right" Visible="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode InitNewRow="False" AllowUpload="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Effective Currency Rates">
				<Template>
					<px:PXGrid ID="gridEffDate" runat="server"  Style="z-index: 100;" Width="100%" Height="220px" AllowPaging="True" Caption="Rate Details" ActionsPosition="top" AllowSearch="true" SkinID="DetailsInTab"
						AdjustPageSize="Auto">
						<Levels>
							<px:PXGridLevel DataMember="CuryRateRecordsEffDate">
								<Columns>
									<px:PXGridColumn DataField="FromCuryID" TextCase="Upper" />
									<px:PXGridColumn DataField="CuryRateType" TextCase="Upper" />
									<px:PXGridColumn DataField="CuryEffDate" />
									<px:PXGridColumn DataField="CuryRate" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryMultDiv" Type="DropDownList" />
									<px:PXGridColumn DataField="RateReciprocal" TextAlign="Right" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="true" Container="Window" MinHeight="320" />
	</px:PXTab>
</asp:Content>
