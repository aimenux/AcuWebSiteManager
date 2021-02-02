<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS501400.aspx.cs" Inherits="Page_FS501400" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%"  Visible="True" runat="server" TypeName="PX.Objects.FS.RoutesOptimizationProcess"  PrimaryView="Filter" PageLoadBehavior="InsertRecord">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
   <px:PXFormView ID="PXFormView1" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter" NoteField="" AllowCollapse="True" TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXDropDown ID="edType" runat="server" CommitChanges="True" DataField="Type"/>
            <px:PXSelector ID="edBranchID" runat="server" CommitChanges="True" DataField="BranchID"/>
            <px:PXSelector ID="edBranchLocationID" runat="server" CommitChanges="True" DataField="BranchLocationID" AutoRefresh="True"/>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXDateTimeEdit runat="server" DataSourceID="ds" ID="edStartDate" DataField="StartDate" CommitChanges="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXSplitContainer runat="server" ID="splitConditions" Orientation="Vertical" Height="100%" PositionInPercent="true" SplitterPosition="70">
        <AutoSize Enabled="true" Container="Window" />
        <Template1>
            <px:PXGrid ID="grid" runat="server" Width="100%" Style="z-index: 100" AllowPaging="True" 
                AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" 
                FastFilterFields="SrvOrdType,SORefNbr,RefNbr,DocDesc,CustomerID,LocationID" SyncPosition="True" SkinID="Inquire">
                <Levels>
                    <px:PXGridLevel DataMember="AppointmentList">
                        <RowTemplate>
                            <px:PXSelector runat="server" ID="edSrvOrdType" DataField="SrvOrdType" />
                            <px:PXSelector runat="server" ID="edRefNbr" DataField="RefNbr" AllowEdit="True" />
                            <px:PXSelector runat="server" ID="edFSAppointmentEmployee_EmployeeID" DataField="FSAppointmentEmployee__EmployeeID" />
                            <px:PXSegmentMask runat="server" ID="edCustomerID" DataField="CustomerID" AllowEdit="True" />
                            <px:PXDateTimeEdit runat="server" ID="edScheduledDateTimeBeginTime" DataField="ScheduledDateTimeBegin_Time" TimeMode="True"/>
                            <px:PXCheckBox runat="server" ID="edConfirmed" DataField="Confirmed" />
                            <px:PXMaskEdit runat="server" ID="edScheduledDuration" DataField="ScheduledDuration" />
                            <px:PXMaskEdit runat="server" ID="edEstimatedDurationTotal" DataField="EstimatedDurationTotal" />
                            <px:PXSelector runat="server" ID="edProjectID" DataField="ProjectID" />
                            <px:PXTextEdit runat="server" ID="edAddressLine1" DataField="AddressLine1" />
                            <px:PXTextEdit runat="server" ID="edState" DataField="State" />
                            <px:PXTextEdit runat="server" ID="edCity" DataField="City" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox"/>
                            <px:PXGridColumn DataField="SrvOrdType"/>
                            <px:PXGridColumn DataField="RefNbr"/>
                            <px:PXGridColumn DataField="PrimaryDriver"/>
                            <px:PXGridColumn DataField="CustomerID" DisplayMode="Hint" />
                            <px:PXGridColumn DataField="ScheduledDateTimeBegin_Time"/>
                            <px:PXGridColumn DataField="Confirmed" TextAlign="Center" Type="CheckBox"/>
                            <px:PXGridColumn DataField="ScheduledDuration"/>
                            <px:PXGridColumn DataField="EstimatedDurationTotal"/>
                            <px:PXGridColumn DataField="ProjectID" DisplayMode="Hint" />
                            <px:PXGridColumn DataField="AddressLine1" />
                            <px:PXGridColumn DataField="State" />
                            <px:PXGridColumn DataField="City" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Container="Window" Enabled="True" MinHeight="200" MinWidth="600"></AutoSize>
            </px:PXGrid>
        </Template1>
        <Template2>
            <px:PXGrid ID="PXGrid1" runat="server" Width="100%" Style="z-index: 100" AllowPaging="True" 
                AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SyncPosition="True" SkinID="Inquire">
                <Levels>
                    <px:PXGridLevel DataMember="StaffMemberFilter" DataKeyNames="AcctCD">
                        <Columns>
                            <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox"/>
                            <px:PXGridColumn DataField="Type"/>
                            <px:PXGridColumn DataField="AcctCD"/>
                            <px:PXGridColumn DataField="AcctName"/>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Container="Window" Enabled="True" MinHeight="200" ></AutoSize>
            </px:PXGrid>
        </Template2>
    </px:PXSplitContainer>
</asp:Content>
