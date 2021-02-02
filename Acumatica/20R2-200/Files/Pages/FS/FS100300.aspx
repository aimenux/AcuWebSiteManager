<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS100300.aspx.cs" Inherits="Page_FS100300" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="EquipmentSetupRecord" SuspendUnloading="False" 
        TypeName="PX.Objects.FS.EquipmentSetupMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" ></px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="EquipmentSetupRecord">
	    <Items>
	      	<px:PXTabItem Text="General Settings">
	      	<Template>
			<px:PXFormView ID="edEquipmentSetup" runat="server" DataMember="EquipmentSetupRecord" DataSourceID="ds" Width="100%" DefaultControlID="edEquipmentNumberingID" Style="z-index: 100">
				<Template>
					<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="XM" ControlSize="XM">
					</px:PXLayoutRule>
					<px:PXLayoutRule runat="server" GroupCaption="Numbering Settings">
					</px:PXLayoutRule>   
					<px:PXSelector ID="edEquipmentNumberingID" runat="server" AllowEdit = "True"
						DataField="EquipmentNumberingID">
					</px:PXSelector>
					<px:PXSelector ID="edServiceContractNumberingID" runat="server" AllowEdit = "True"
						DataField="ServiceContractNumberingID">
					</px:PXSelector>
					<px:PXSelector ID="edScheduleNumberingID" runat="server" AllowEdit = "True"
						DataField="ScheduleNumberingID">
					</px:PXSelector>
					<px:PXLayoutRule runat="server" GroupCaption="General Settings" StartGroup="True">
		            </px:PXLayoutRule> 
		            <px:PXCheckBox ID="edEnableAllTargetEquipment" runat="server" AlignLeft="True" DataField="EnableAllTargetEquipment">
		            </px:PXCheckBox> 
					<px:PXLayoutRule runat="server" GroupCaption="Contract Settings">
					</px:PXLayoutRule>
					<px:PXCheckBox ID="edEnableSeasonScheduleContract" runat="server" AlignLeft="True" DataField="EnableSeasonScheduleContract">
					</px:PXCheckBox>
					<px:PXLayoutRule runat="server" GroupCaption="Equipment Settings">
					</px:PXLayoutRule>  
					<px:PXGroupBox ID="edEquipmentCalculateWarrantyFrom" runat="server" 
					Caption="Calculate Warranty From" CommitChanges="True" 
					DataField="EquipmentCalculateWarrantyFrom" 
					Width="100%">
						<Template>
							<px:PXRadioButton ID="edEquipmentCalculateWarrantyFrom_op0" runat="server" 
								GroupName="edEquipmentCalculateWarrantyFrom" Text="Sales Order Date" Value="SD" />
							<px:PXRadioButton ID="edEquipmentCalculateWarrantyFrom_op1" runat="server" 
								GroupName="edEquipmentCalculateWarrantyFrom" Text="Installation Date" Value="AD" />
							<px:PXRadioButton ID="edEquipmentCalculateWarrantyFrom_op2" runat="server" 
								GroupName="edEquipmentCalculateWarrantyFrom" Text="The Earliest of Both Dates" Value="ED" />
							<px:PXRadioButton ID="edEquipmentCalculateWarrantyFrom_op3" runat="server" 
								GroupName="edEquipmentCalculateWarrantyFrom" Text="The Latest of Both Dates" Value="LD" />
							<px:PXLayoutRule runat="server" EndGroup="True">
							</px:PXLayoutRule>
						</Template>
						<ContentLayout layout="Stack" />
					</px:PXGroupBox>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM">
					</px:PXLayoutRule>
					<px:PXLayoutRule 
						runat="server" 
						StartColumn="True" 
						GroupCaption="Billing Settings">
					</px:PXLayoutRule>
					<%-- Posting Settings Fields--%>
					<px:PXDropDown ID="edContractPostTo" runat="server" CommitChanges="True" DataField="ContractPostTo">
                    </px:PXDropDown>
					<px:PXSelector ID="edContractPostOrderType" runat="server" AllowEdit="True" AutoRefresh="True" CommitChanges="True" DataField="ContractPostOrderType" DataSourceID="ds" >
					</px:PXSelector>
					<px:PXSelector ID="edDfltTermIDARSO" runat="server" AllowEdit="True" AutoRefresh="True" DataField="DfltContractTermIDARSO" DataSourceID="ds">
					</px:PXSelector>
					<px:PXDropDown ID="edSalesAcctSource" runat="server" CommitChanges="True" DataField="ContractSalesAcctSource">
					</px:PXDropDown>
					<px:PXSegmentMask ID="edCombineSubFrom" runat="server" CommitChanges="True" DataField="ContractCombineSubFrom">
					</px:PXSegmentMask>
					<px:PXCheckBox ID="edEnableContractPeriodWhenInvoice" runat="server" DataField="EnableContractPeriodWhenInvoice" AlignLeft="True">
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
