<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS404070.aspx.cs" Inherits="Page_FS404070" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="PX.Objects.FS.RouteAppointmentForecastingInq"
        PrimaryView="RouteAppointmentForecastingFilter"
        >
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="GenerateProjection" CommitChanges="True"/>
		    <px:PXDSCallbackCommand Name="OpenServiceContractScreen" Visible="False"/>
            <px:PXDSCallbackCommand Name="OpenScheduleScreen" Visible="False"/>
            <px:PXDSCallbackCommand Name="OpenLocationScreen" Visible="False"/>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="RouteAppointmentForecastingFilter" Width="100%" AllowAutoHide="false">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edsRouteID" runat="server" DataField="RouteID" CommitChanges="True">
			</px:PXSelector>
            <px:PXSelector ID="edServiceID" runat="server" DataField="ServiceID" CommitChanges="True">
            </px:PXSelector>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M">
			</px:PXLayoutRule>
            <px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID" CommitChanges="True">
            </px:PXSelector>
            <px:PXSelector ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" AutoRefresh="True" CommitChanges="True">
            </px:PXSelector>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M">
			</px:PXLayoutRule>
			<px:PXDateTimeEdit ID="edDateBegin" runat="server" CommitChanges="True" DataField="DateBegin">
            </px:PXDateTimeEdit>
            <px:PXDateTimeEdit ID="edDateEnd" runat="server" CommitChanges="True" DataField="DateEnd">
            </px:PXDateTimeEdit>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" AllowAutoHide="false" AllowPaging="True" AdjustPageSize="Auto" AutoAdjustColumns="True" SyncPosition="True" KeepPosition="True">
        <Levels>
            <px:PXGridLevel 
                DataMember="RouteAppointmentForecastingRecords" DataKeyNames="StartDate, ScheduleID, CustomerID, LocationID, RouteID">
                <RowTemplate>
                <px:PXLayoutRule runat="server" StartColumn="True">
                </px:PXLayoutRule> 
                    <px:PXDateTimeEdit ID="edStartDate" runat="server" CommitChanges="True" DataField="StartDate">
                    </px:PXDateTimeEdit>
                    <px:PXSelector ID="edRouteID" runat="server" DataField="RouteID" CommitChanges="True" AllowEdit="True">
                    </px:PXSelector>
					<px:PXSelector ID="edScheduleRefNbr" runat="server" DataField="FSSchedule__RefNbr" CommitChanges="True" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edServiceContractID" runat="server" DataField="ServiceContractID" CommitChanges="True" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXTextEdit ID="edFSServiceContract__CustomerContractNbr" runat="server" DataField="FSServiceContract__CustomerContractNbr">
                    </px:PXTextEdit>           
                    <px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID" CommitChanges="True" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edCustomerLocation" runat="server" DataField="CustomerLocationID" CommitChanges="True" AllowEdit="True">
                    </px:PXSelector>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="StartDate">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="RouteID" DisplayMode="Hint">
                    </px:PXGridColumn>                                    
					<px:PXGridColumn DataField="FSSchedule__RefNbr" LinkCommand="OpenScheduleScreen">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="ServiceContractID" LinkCommand="OpenServiceContractScreen"> 
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="FSServiceContract__CustomerContractNbr">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerID" DisplayMode="Hint">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerLocationID" DisplayMode="Hint" LinkCommand="OpenLocationScreen">
                    </px:PXGridColumn>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar >
            <Actions>
                <AddNew ToolBarVisible ="False"/>
                <Delete ToolBarVisible="False"/>    
            </Actions>  
        </ActionBar>
    </px:PXGrid>
</asp:Content>