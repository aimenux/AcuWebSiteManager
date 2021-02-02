<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM206500.aspx.cs" Inherits="Page_SM206500"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter"
		TypeName="PX.SM.SMPrintJobMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Reprint" Visible="True" CommitChanges="true" DependOnGrid="jobsGrid" />
            <px:PXDSCallbackCommand Name="ShowReport" Visible="True" CommitChanges="true" DependOnGrid="jobsGrid" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" Visible="true" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" Visible="false" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" Visible="false" />
            			<px:PXDSCallbackCommand Name="Next" PostData="Self" Visible="false" />
            			<px:PXDSCallbackCommand Name="Previous" PostData="Self" Visible="false" />
            			<px:PXDSCallbackCommand Name="CopyPaste" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="Delete" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:content id="cont2" contentplaceholderid="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" Visible="true" DataMember="Filter" AllowCollapse="false">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" CommitChanges="true" />
            <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" CommitChanges="true" />
            <px:PXCheckBox ID="chkHideProcessed" runat="server" DataField="HideProcessed" CommitChanges="true" />
        </Template>
    </px:PXFormView>
</asp:content>
<asp:content id="cont3" contentplaceholderid="phG" runat="Server">
        <px:PXGrid ID="jobsGrid" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" SyncPosition="True" >
        <Levels>
            <px:PXGridLevel DataMember="Job">
                <Columns>
                    <px:PXGridColumn DataField="Selected" AllowCheckAll="true" Type="CheckBox" TextAlign="Center" />
                    <px:PXGridColumn DataField="JobID" TextAlign="Left" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="ReportID" />
					<px:PXGridColumn DataField="DeviceHubID" TextAlign="Left" />
                    <px:PXGridColumn DataField="PrinterName" />
					<px:PXGridColumn DataField="NumberOfCopies" />
                    <px:PXGridColumn DataField="CreatedByID" />
                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" />
                    <px:PXGridColumn DataField="Status" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
		</px:PXGrid>

    	<px:PXSmartPanel ID="paramsPanel" runat="server" Height="396px" Width="850px" Caption="Print Job Parameters" Key="Parameters" >
			<px:PXGrid ID="paramsGrid" runat="server" Height="240px" Width="100%" DataSourceID="ds" SkinID="Details" SyncPosition="true">
				<AutoSize Enabled="true" />
				<Parameters>
					<px:PXSyncGridParam ControlID="jobsGrid" />
				</Parameters>
				<Levels>
					<px:PXGridLevel DataMember="Parameters" DataKeyNames="JobID,ParameterName">
						<Columns>
							<px:PXGridColumn DataField="ParameterName" />
							<px:PXGridColumn DataField="Parametervalue" />
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
			</px:PXGrid>
		</px:PXSmartPanel>
</asp:content>