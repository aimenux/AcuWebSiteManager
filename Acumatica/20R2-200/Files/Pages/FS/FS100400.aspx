<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS100400.aspx.cs" Inherits="Page_FS100400" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="RouteSetupRecord" SuspendUnloading="False" 
        TypeName="PX.Objects.FS.RouteSetupMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" ></px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="SetupRecord">
	    <Items>
	      	<px:PXTabItem Text="General Settings">
	      	<Template>
			<px:PXFormView ID="edRouteSetup" runat="server" DataMember="RouteSetupRecord" DataSourceID="ds" Width="100%">
				<Template>
					<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="XM" ControlSize="XM">
					</px:PXLayoutRule>
					<px:PXLayoutRule runat="server" GroupCaption="Numbering Settings" >
					</px:PXLayoutRule>
					<px:PXFormView ID="edEquipmentNumbering" runat="server" DataMember="SetupRecord" DataSourceID="ds" Width="100%" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="XM" ControlSize="XM">
							</px:PXLayoutRule>
							<px:PXSelector ID="edEquipmentNumberingID" runat="server" AllowEdit = "True"
								DataField="EquipmentNumberingID">
							</px:PXSelector>
						</Template>
					</px:PXFormView>
					<px:PXSelector ID="edRouteNumberingID" runat="server" 
						DataField="RouteNumberingID" AllowEdit = "True" >
					</px:PXSelector>
					<px:PXFormView ID="edSetupContractNumbering" runat="server" DataMember="SetupRecord" DataSourceID="ds" Width="100%" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="XM" ControlSize="XM">
							</px:PXLayoutRule>
							<px:PXSelector ID="edServiceContractNumberingID" runat="server" AllowEdit = "True"
								DataField="ServiceContractNumberingID">
							</px:PXSelector>
							<px:PXSelector ID="edScheduleNumberingID" runat="server" AllowEdit = "True"
								DataField="ScheduleNumberingID">
							</px:PXSelector>
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" GroupCaption="Contract Settings" >
					</px:PXLayoutRule>
					<px:PXCheckBox ID="edEnableSeasonScheduleContractRoute" runat="server" AlignLeft="True" DataField="EnableSeasonScheduleContract">
					</px:PXCheckBox>
					<px:PXLayoutRule runat="server" GroupCaption="Route Settings">
					</px:PXLayoutRule>   
					<px:PXSelector ID="edDfltSrvOrdType" runat="server" 
						DataField="DfltSrvOrdType" >                        
					</px:PXSelector>                      
					<px:PXCheckBox ID="edAutoCalculateRouteStats" runat="server" DataField="AutoCalculateRouteStats" AlignLeft="True">
					</px:PXCheckBox>
					<px:PXCheckBox ID="edGroupINDocumentsByPostingProcess" runat="server" DataField="GroupINDocumentsByPostingProcess" Text="Group IN documents by Posting process" AlignLeft="True" CommitChanges="True">
					</px:PXCheckBox> 
					<px:PXCheckBox ID="edSetFirstManualAppointment" runat="server" DataField="SetFirstManualAppointment" AlignLeft="True">
					</px:PXCheckBox> 
					<px:PXCheckBox ID="edTrackRouteLocation" runat="server" DataField="TrackRouteLocation" AlignLeft="True">
					</px:PXCheckBox>
					<px:PXLayoutRule 
						runat="server" 
						StartColumn="True" 
						GroupCaption="Billing Settings"
						LabelsWidth="XM" ControlSize="XM">
					</px:PXLayoutRule>
					<%-- Posting Settings Fields--%>
					<px:PXDropDown ID="edContractPostTo" runat="server" CommitChanges="True" DataField="SetupRecord.ContractPostTo">
			        </px:PXDropDown>
					<px:PXSelector ID="edContractPostOrderType" runat="server" AllowEdit="True" AutoRefresh="True" CommitChanges="True" DataField="SetupRecord.ContractPostOrderType" DataSourceID="ds">
					</px:PXSelector>
					<px:PXSelector ID="edDfltTermIDARSO" runat="server" AllowEdit="True" AutoRefresh="True" DataField="SetupRecord.DfltContractTermIDARSO" DataSourceID="ds">
					</px:PXSelector>
					<px:PXDropDown ID="edSalesAcctSource" runat="server" CommitChanges="True" DataField="SetupRecord.ContractSalesAcctSource">
					</px:PXDropDown>
					<px:PXSegmentMask ID="edCombineSubFrom" runat="server" CommitChanges="True" DataField="SetupRecord.ContractCombineSubFrom">
					</px:PXSegmentMask>
					<px:PXCheckBox ID="edEnableContractPeriodWhenInvoice" runat="server" DataField="SetupRecord.EnableContractPeriodWhenInvoice" AlignLeft="True">
					</px:PXCheckBox>
					 <%-- Posting Settingss Fields--%>
			</Template>
			<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		</px:PXFormView>
		</Template>
      	</px:PXTabItem>
    </Items>
    <AutoSize Container="Window" Enabled="True" MinHeight="150" />
  	</px:PXTab>
</asp:Content>
