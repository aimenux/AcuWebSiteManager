<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS404000.aspx.cs" Inherits="Page_FS404000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.FS.RoutePendingInq" 
        PrimaryView="Filter" SuspendUnloading="False">
        <CallbackCommands>              
            <px:PXDSCallbackCommand Name="OpenRouteClosing" Visible="False">
            </px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="75px" DataMember="Filter" TabIndex="3700" DefaultControlID="edDate">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" StartRow="True" ControlSize="SM" LabelsWidth="XS">
            </px:PXLayoutRule>
            <px:PXDateTimeEdit ID="edDate" runat="server" CommitChanges="True"
                DataField="Date">
            </px:PXDateTimeEdit>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="Inquire" TabIndex="3000" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="Routes" DataKeyNames="RefNbr">
			    <RowTemplate>
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date">
                    </px:PXDateTimeEdit>
                    <px:PXSelector ID="edDriverID" runat="server" DataField="DriverID" 
                        AllowEdit="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edAddtionalDriverID" runat="server" DataField="AdditionalDriverID" 
                        AllowEdit="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edVehicleID" runat="server" DataField="VehicleID" 
                        AllowEdit="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edRouteID" runat="server" 
                        DataField="RouteID" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXTextEdit ID="edRouteShort" runat="server" 
                        DataField="FSRoute__RouteShort" AllowEdit="True">
                    </px:PXTextEdit>
                    <px:PXNumberEdit ID="edTripNbr" runat="server" DataField="TripNbr">
                    </px:PXNumberEdit>
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status">
                    </px:PXDropDown>
                    <px:PXDateTimeEdit ID="edTimeBegin_Time" runat="server" 
                        DataField="TimeBegin_Time">
                    </px:PXDateTimeEdit>
                    <px:PXDateTimeEdit ID="edTimeEnd_Time" runat="server" DataField="TimeEnd_Time">
                    </px:PXDateTimeEdit>
                    <px:PXNumberEdit ID="edTotalNumAppointments" runat="server" 
                        DataField="TotalNumAppointments">
                    </px:PXNumberEdit>
                    <px:PXMaskEdit ID="edTotalDuration" runat="server" DataField="TotalDuration">
                    </px:PXMaskEdit>
                    <px:PXMaskEdit ID="edTotalTravelTime" runat="server" 
                        DataField="TotalTravelTime">
                    </px:PXMaskEdit>
                    <px:PXDateTimeEdit ID="edTimeBegin_Date" runat="server" 
                        DataField="TimeBegin_Date">
                    </px:PXDateTimeEdit>
                    <px:PXNumberEdit ID="edTotalServices" runat="server" DataField="TotalServices">
                    </px:PXNumberEdit>
                    <px:PXMaskEdit ID="edTotalServicesDuration" runat="server" 
                        DataField="TotalServicesDuration">
                    </px:PXMaskEdit>
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="OpenRouteClosing">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="RouteID">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="FSRoute__RouteShort">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TripNbr">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Status">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Date">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TimeBegin_Time" TimeMode="True">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TimeEnd_Time" TimeMode="True">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="DriverID" CommitChanges="true" DisplayMode="Hint">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="AdditionalDriverID" CommitChanges="true" DisplayMode="Hint">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="VehicleID" CommitChanges="True">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TotalNumAppointments" TextAlign="Right">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TotalServices" TextAlign="Right">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TotalDuration">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TotalServicesDuration">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TotalTravelTime">
                    </px:PXGridColumn>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
