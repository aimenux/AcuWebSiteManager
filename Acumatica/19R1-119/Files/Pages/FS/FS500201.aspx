<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS500201.aspx.cs" Inherits="Page_FS500201" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        BorderStyle="NotSet" PrimaryView="Filter"
        TypeName="PX.Objects.FS.CloneAppointmentProcess">
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="Clone" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="True">
            </px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="OpenAppointment" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="false">
            </px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100; margin-bottom: 0px;"
		Width="100%" DataMember="AppointmentSelected" NoteIndicator="True" TabIndex="-14736">
        <Template>
            <px:PXLayoutRule runat="server" 
                StartColumn="True" GroupCaption="Original Appointment Info" 
                StartRow="True">
            </px:PXLayoutRule>
            <px:PXPanel ID="PXPanel1" runat="server" DataMember="" RenderSimple="True" RenderStyle="Simple">
                <px:PXLayoutRule runat="server" ColumnWidth="M" StartColumn="True" StartRow="True">
                </px:PXLayoutRule>
                <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr">
                </px:PXSelector>
                <px:PXSelector ID="edSORefNbr" runat="server" DataField="SORefNbr">
                </px:PXSelector>
                <px:PXSelector ID="edSrvOrdType" runat="server" DataField="SrvOrdType">
                </px:PXSelector>
                <px:PXLabel ID="PXLabel1" runat="server"></px:PXLabel>
                <px:PXLayoutRule runat="server" StartColumn="True">
                </px:PXLayoutRule>
                <px:PXSegmentMask ID="edFSServiceOrder__CustomerID" runat="server" AllowEdit="True" DataField="FSServiceOrder__CustomerID" DataSourceID="ds" edit="1" LabelWidth="125px" Width="150px">
                </px:PXSegmentMask>
                <px:PXSelector ID="edFSServiceOrder__BranchLocationID" runat="server" DataField="FSServiceOrder__BranchLocationID" LabelWidth="125px">
                </px:PXSelector>
                <px:PXSelector ID="edFSServiceOrder__RoomID" runat="server" DataField="FSServiceOrder__RoomID" LabelWidth="125px">
                </px:PXSelector>
                <px:PXLayoutRule runat="server" StartColumn="True">
                </px:PXLayoutRule>
                <px:PXDropDown ID="Status" runat="server" DataField="Status" Enabled="False" LabelWidth="60px" Width="90px">
                </px:PXDropDown>
                <px:PXCheckBox ID="edConfirmed" runat="server" AlignLeft="True" DataField="Confirmed">
                </px:PXCheckBox>
            </px:PXPanel>
            <px:PXLayoutRule runat="server" StartColumn="True" 
                GroupCaption="New Date and Time" StartRow="True">
            </px:PXLayoutRule>
            <px:PXFormView ID="PXFormView1" runat="server" DataMember="Filter" DataSourceID="ds" DefaultControlID="edScheduledDate_Date" RenderStyle="Simple" TabIndex="10600" Width="100%">
                <Template>
                    <px:PXPanel ID="PXPanel2" runat="server" RenderSimple="True" RenderStyle="Simple">
                        <px:PXLayoutRule runat="server" ColumnWidth="S" LabelsWidth="S" StartColumn="True" StartRow="True">
                        </px:PXLayoutRule>
                        <px:PXDropDown ID="edCloningType" runat="server" CommitChanges="True" DataField="CloningType" SuppressLabel="False">
                        </px:PXDropDown>
                        <px:PXLabel runat="server"></px:PXLabel>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS">
                        </px:PXLayoutRule>
                        <px:PXDateTimeEdit ID="edScheduledStartTime_Time" runat="server" CommitChanges="True" DataField="ScheduledStartTime_Time" TimeMode="True">
                        </px:PXDateTimeEdit>
                        <px:PXDateTimeEdit ID="edScheduledEndTime_Time" runat="server" CommitChanges="True" DataField="ScheduledEndTime_Time" TimeMode="True">
                        </px:PXDateTimeEdit>
                        <px:PXLabel runat="server"></px:PXLabel>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS">
                        </px:PXLayoutRule>
                        <px:PXDateTimeEdit ID="edScheduledDate_Date" runat="server" CommitChanges="True" DataField="ScheduledDate_Date" SuppressLabel="False">
                        </px:PXDateTimeEdit>
                        <px:PXDateTimeEdit ID="edScheduledFromDate_Date" runat="server" CommitChanges="True" DataField="ScheduledFromDate_Date" SuppressLabel="False">
                        </px:PXDateTimeEdit>
                        <px:PXDateTimeEdit ID="edScheduledToDate_Date" runat="server" CommitChanges="True" DataField="ScheduledToDate_Date" SuppressLabel="False">
                        </px:PXDateTimeEdit>
                        <px:PXLabel runat="server"></px:PXLabel>
                        <px:PXLayoutRule runat="server" StartColumn="True"  ColumnWidth="XS"  ControlSize="XS" LabelsWidth="XS">
                        </px:PXLayoutRule>
                        <px:PXCheckBox ID="edActiveOnSunday" runat="server" DataField="ActiveOnSunday" Text="Sunday" CommitChanges="True">
                        </px:PXCheckBox>
                        <px:PXCheckBox ID="edActiveOnMonday" runat="server" DataField="ActiveOnMonday" Text="Monday" CommitChanges="True">
                        </px:PXCheckBox>
                        <px:PXCheckBox ID="edActiveOnTuesday" runat="server" DataField="ActiveOnTuesday" Text="Tuesday" CommitChanges="True">
                        </px:PXCheckBox>
                        <px:PXCheckBox ID="edActiveOnWednesday" runat="server" DataField="ActiveOnWednesday" Text="Wednesday" CommitChanges="True">
                        </px:PXCheckBox>
                        <px:PXLabel runat="server"></px:PXLabel>
                        <px:PXLayoutRule runat="server" StartColumn="True"  ColumnWidth="XS"  ControlSize="XS" LabelsWidth="XS">
                        </px:PXLayoutRule>
                        <px:PXCheckBox ID="edActiveOnThursday" runat="server" DataField="ActiveOnThursday" Text="Thursday " CommitChanges="True">
                        </px:PXCheckBox>
                        <px:PXCheckBox ID="edActiveOnFriday" runat="server" DataField="ActiveOnFriday" Text="Friday" CommitChanges="True">
                        </px:PXCheckBox>
                        <px:PXCheckBox ID="edActiveOnSaturday" runat="server" DataField="ActiveOnSaturday" Text="Saturday" CommitChanges="True">
                        </px:PXCheckBox>
                    </px:PXPanel>
                </Template>
            </px:PXFormView>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont4" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="ClonesTab" runat="server" Height="150px" Style="z-index: 100" Width="100%" DataMember="AppointmentClones" DataSourceID="ds">
        <AutoSize Enabled="True" Container="Window" MinWidth="300" MinHeight="250"></AutoSize>        
        <Items>
            <px:PXTabItem Text="Cloned Appointments">
                <Template>                        
                    <px:PXGrid ID="PXRouteEmployees" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" KeepPosition="True" SyncPosition="True"
                        AllowPaging="True" AdjustPageSize="Auto" Height="200px" TabIndex="11300" FilesIndicator="False" NoteIndicator="False">
                        <Levels>
                            <px:PXGridLevel DataMember="AppointmentClones">
                                <RowTemplate>
                                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edSrvOrdType" runat="server" DataField="SrvOrdType">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edSORefNbr" runat="server" DataField="SORefNbr">
                                    </px:PXSelector>
                                    <px:PXCheckBox ID="edConfirmed" runat="server" DataField="Confirmed" 
                                        Text="Confirmed">
                                    </px:PXCheckBox>
                                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status">
                                    </px:PXDropDown>
                                    <px:PXSelector ID="edFSServiceOrder__RoomID" runat="server" 
                                        DataField="FSServiceOrder__RoomID">
                                    </px:PXSelector>
                                    <px:PXDateTimeEdit ID="edScheduledDateTimeBegin" runat="server" 
                                        DataField="ScheduledDateTimeBegin">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="edScheduledDateTimeBegin_Time" runat="server" DataField="ScheduledDateTimeBegin_Time">
                                    </px:PXDateTimeEdit>
                                    <px:PXDateTimeEdit ID="edScheduledDateTimeEnd_Time" runat="server" DataField="ScheduledDateTimeEnd_Time">
                                    </px:PXDateTimeEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="RefNbr" Width="80px" LinkCommand="OpenAppointment">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SrvOrdType">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SORefNbr">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Confirmed" TextAlign="Center" Type="CheckBox" 
                                        Width="60px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Status">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSServiceOrder__RoomID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ScheduledDateTimeBegin" Width="90px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ScheduledDateTimeBegin_Time" Width="90px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ScheduledDateTimeEnd_Time" Width="90px">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    <AutoSize Enabled="True" MinHeight="200" ></AutoSize>                    
                    </px:PXGrid>                                            
                </Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>