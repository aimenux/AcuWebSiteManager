<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS404100.aspx.cs" Inherits="Page_FS404100" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.FS.RouteDocumentInq" 
        PrimaryView="Filter" SuspendUnloading="False">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="OpenRouteDocument" Visible="False"></px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter" TabIndex="3700">
        <Template>
            <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="SM" StartColumn="True" StartRow="True">
            </px:PXLayoutRule>
            <px:PXSelector ID="edRouteID" runat="server" CommitChanges="True" DataField="RouteID">
            </px:PXSelector>
            <px:PXDateTimeEdit ID="edFromDate" runat="server" CommitChanges="True" DataField="FromDate">
            </px:PXDateTimeEdit>
            <px:PXDateTimeEdit ID="edToDate" runat="server" CommitChanges="True" DataField="ToDate">
            </px:PXDateTimeEdit>
            <px:PXLayoutRule runat="server" GroupCaption="Status" StartColumn="True" StartGroup="True" SuppressLabel="True" LabelsWidth="S" ControlSize="SM">
            </px:PXLayoutRule>
            <px:PXPanel ID="PXPanel1" runat="server" DataMember="" RenderSimple="True" RenderStyle="Simple">
                <px:PXLayoutRule runat="server" StartColumn="True" StartRow="True" SuppressLabel="True" LabelsWidth="S" ControlSize="SM">
                </px:PXLayoutRule>
                <px:PXCheckBox ID="edStatusOpen" runat="server" CommitChanges="True" DataField="StatusOpen" Text="Open">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edStatusInProcess" runat="server" CommitChanges="True" DataField="StatusInProcess" Text="In Process">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edStatusCanceled" runat="server" CommitChanges="True" DataField="StatusCanceled" Text="Canceled">
                </px:PXCheckBox>
                <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" LabelsWidth="S" ControlSize="SM">
                </px:PXLayoutRule>
                <px:PXCheckBox ID="edStatusCompleted" runat="server" CommitChanges="True" DataField="StatusCompleted" Text="Completed">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edStatusClosed" runat="server" CommitChanges="True" DataField="StatusClosed" Text="Closed">
                </px:PXCheckBox>
            </px:PXPanel>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" SkinID="Inquire" TabIndex="3000" SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" KeepPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="RouteDocuments" DataKeyNames="RefNbr">
			    <RowTemplate>
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edRouteID" runat="server" DataField="RouteID" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXTextEdit ID="edTripNbr" runat="server" DataField="TripNbr">
                    </px:PXTextEdit>
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status">
                    </px:PXDropDown>
                    <px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date">
                    </px:PXDateTimeEdit>
                    <px:PXDateTimeEdit ID="edTimeBegin_Time" runat="server" DataField="TimeBegin_Time">
                    </px:PXDateTimeEdit>
                    <px:PXSelector ID="edDriverID" runat="server" DataField="DriverID" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edAdditionalDriverID" runat="server" DataField="AdditionalDriverID" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edVehicleID" runat="server" DataField="VehicleID" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edAdditionalVehicleID1" runat="server" DataField="AdditionalVehicleID1" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edAdditionalVehicleID2" runat="server" DataField="AdditionalVehicleID2" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXNumberEdit ID="edTotalNumAppointments" runat="server" DataField="TotalNumAppointments">
                    </px:PXNumberEdit>
                    <px:PXNumberEdit ID="edTotalServices" runat="server" DataField="TotalServices">
                    </px:PXNumberEdit>
                    <px:PXMaskEdit ID="edTotalTravelTime" runat="server" DataField="TotalTravelTime">
                    </px:PXMaskEdit>
                    <px:PXMaskEdit ID="edTotalServicesDuration" runat="server" DataField="TotalServicesDuration">
                    </px:PXMaskEdit>
                    <px:PXMaskEdit ID="edTotalDuration" runat="server" DataField="TotalDuration">
                    </px:PXMaskEdit>
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="OpenRouteDocument">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="RouteID">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TripNbr">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Status">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Date">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TimeBegin_Time">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="DriverID" DisplayMode="Hint">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="AdditionalDriverID" DisplayMode="Hint">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="VehicleID">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="AdditionalVehicleID1">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="AdditionalVehicleID2">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TotalNumAppointments" TextAlign="Right">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TotalServices" TextAlign="Right">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TotalTravelTime">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TotalServicesDuration">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TotalDuration">
                    </px:PXGridColumn>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False"/>
	</px:PXGrid>
</asp:Content>
