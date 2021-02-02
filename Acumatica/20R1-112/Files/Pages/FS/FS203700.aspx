<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="FS203700.aspx.cs" Inherits="Page_FS203700" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.FS.RouteMaint" 
        PrimaryView="RouteRecords" SuspendUnloading="False">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />                  
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" FilesIndicator="true" 
        Style="z-index: 100" Width="100%" TabIndex="800" DataMember="RouteRecords" DefaultControlID="edRouteCD" AllowCollapse="True">
		<Template>
            <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" ControlSize="SM" LabelsWidth="S">
            </px:PXLayoutRule>
            <px:PXSelector ID="edRouteCD" runat="server" DataField="RouteCD">
            </px:PXSelector>
            <px:PXSelector ID="edOriginRouteID" runat="server" AutoRefresh="True" CommitChanges="True" DataField="OriginRouteID">
            </px:PXSelector>
            <px:PXMaskEdit ID="edRouteShort" runat="server" DataField="RouteShort">
            </px:PXMaskEdit>
            <px:PXLayoutRule runat="server" ColumnSpan="2">
            </px:PXLayoutRule>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr">
            </px:PXTextEdit>
            <px:PXLayoutRule runat="server" ControlSize="SM" StartColumn="True" LabelsWidth="S">
            </px:PXLayoutRule>
            <px:PXTextEdit ID="edWeekCode" runat="server" DataField="WeekCode" CommitChanges="True">
            </px:PXTextEdit>
            <px:PXLayoutRule runat="server" Merge="True">
            </px:PXLayoutRule>
            <px:PXNumberEdit ID="edMaxAppointmentQty" runat="server" DataField="MaxAppointmentQty" CommitChanges="True" Width="68px">
            </px:PXNumberEdit>
            <px:PXCheckBox ID="edNoAppointmentLimit" runat="server" CommitChanges="True" DataField="NoAppointmentLimit" Text="No Limit">
            </px:PXCheckBox>
            <px:PXLayoutRule runat="server" StartRow="True">
            </px:PXLayoutRule>
            <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Start Location" ControlSize="SM" LabelsWidth="S">
            </px:PXLayoutRule>
            <px:PXSelector ID="edBeginBranchID" runat="server" CommitChanges="True" DataField="BeginBranchID" AllowEdit="True" AutoRefresh="True">
            </px:PXSelector>
            <px:PXSelector ID="edBeginBranchLocationID" runat="server" AutoRefresh="True" CommitChanges="True" 
                DataSourceID="ds" DataField="BeginBranchLocationID" AllowEdit="True">
            </px:PXSelector>
            <px:PXLayoutRule  runat="server" StartColumn="True" GroupCaption="End Location" ControlSize="SM" LabelsWidth="S">
            </px:PXLayoutRule>
            <px:PXSelector ID="edEndBranchID" runat="server" CommitChanges="True" DataField="EndBranchID" AllowEdit="True" AutoRefresh="True">
            </px:PXSelector>
            <px:PXSelector ID="edEndBranchLocationID" runat="server" AutoRefresh="True" CommitChanges="True" 
                DataSourceID="ds" DataField="EndBranchLocationID" AllowEdit="True">
            </px:PXSelector>                
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab runat="server" Style="z-index: 100" Width="100%" DataMember="RouteSelected" DataSourceID="ds">
        <AutoSize Enabled="True" Container="Window" MinWidth="300" MinHeight="250"></AutoSize>        
        <Items>
            <px:PXTabItem Text="Execution Days">
                <Template>                        
                <px:PXLayoutRule runat="server" StartColumn="True" StartRow="True" SuppressLabel="True">
                </px:PXLayoutRule>
                <px:PXLabel runat="server">Day of Week</px:PXLabel>
                <px:PXCheckBox ID="edActiveOnSunday" runat="server" DataField="ActiveOnSunday" Text="Sunday" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edActiveOnMonday" runat="server" DataField="ActiveOnMonday" Text="Monday" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edActiveOnTuesday" runat="server" DataField="ActiveOnTuesday" Text="Tuesday" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edActiveOnWednesday" runat="server" DataField="ActiveOnWednesday" Text="Wednesday" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edActiveOnThursday" runat="server" DataField="ActiveOnThursday" Text="Thursday " CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edActiveOnFriday" runat="server" DataField="ActiveOnFriday" Text="Friday" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXCheckBox ID="edActiveOnSaturday" runat="server" DataField="ActiveOnSaturday" Text="Saturday" CommitChanges="True">
                </px:PXCheckBox>
                <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" LabelsWidth="80px">
                </px:PXLayoutRule>
                <px:PXLabel runat="server">Start Time</px:PXLabel>
                <px:PXDateTimeEdit ID="edBeginTimeOnSunday_Time" runat="server" DataField="BeginTimeOnSunday_Time" TimeMode="True" CommitChanges="True">
                </px:PXDateTimeEdit>
                <px:PXDateTimeEdit ID="edBeginTimeOnMonday_Time" runat="server" DataField="BeginTimeOnMonday_Time" TimeMode="True" CommitChanges="True">
                </px:PXDateTimeEdit>
                <px:PXDateTimeEdit ID="edBeginTimeOnTuesday_Time" runat="server" DataField="BeginTimeOnTuesday_Time" TimeMode="True" CommitChanges="True">
                </px:PXDateTimeEdit>
                <px:PXDateTimeEdit ID="edBeginTimeOnWednesday_Time" runat="server" DataField="BeginTimeOnWednesday_Time" TimeMode="True" CommitChanges="True">
                </px:PXDateTimeEdit>
                <px:PXDateTimeEdit ID="edBeginTimeOnThursday_Time" runat="server" DataField="BeginTimeOnThursday_Time" TimeMode="True" CommitChanges="True">
                </px:PXDateTimeEdit>
                <px:PXDateTimeEdit ID="edBeginTimeOnFriday_Time" runat="server" DataField="BeginTimeOnFriday_Time" TimeMode="True" CommitChanges="True">
                </px:PXDateTimeEdit>
                <px:PXDateTimeEdit ID="edBeginTimeOnSaturday_Time" runat="server" DataField="BeginTimeOnSaturday_Time" TimeMode="True" CommitChanges="True">
                </px:PXDateTimeEdit>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" SuppressLabel="True">
                </px:PXLayoutRule>
                <px:PXLabel runat="server">Nbr. Trips per Day</px:PXLabel>
                <px:PXNumberEdit ID="edNbrTripOnSunday" runat="server" DataField="NbrTripOnSunday">
                </px:PXNumberEdit>
                <px:PXNumberEdit ID="edNbrTripOnMonday" runat="server" DataField="NbrTripOnMonday">
                </px:PXNumberEdit>
                <px:PXNumberEdit ID="edNbrTripOnTuesday" runat="server" DataField="NbrTripOnTuesday">
                </px:PXNumberEdit>
                <px:PXNumberEdit ID="edNbrTripOnWednesday" runat="server" DataField="NbrTripOnWednesday">
                </px:PXNumberEdit>
                <px:PXNumberEdit ID="edNbrTripOnThursday" runat="server" DataField="NbrTripOnThursday">
                </px:PXNumberEdit>
                <px:PXNumberEdit ID="edNbrTripOnFriday" runat="server" DataField="NbrTripOnFriday">
                </px:PXNumberEdit>
                <px:PXNumberEdit ID="edNbrTripOnSaturday" runat="server" DataField="NbrTripOnSaturday">
                </px:PXNumberEdit>
                </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Route Employees">
                <Template>                        
                    <px:PXGrid ID="PXRouteEmployees" runat="server" DataSourceID="ds" SkinID="Details" Width="100%"  
                        AllowPaging="True" AdjustPageSize="Auto" TabIndex="11300" FilesIndicator="False" NoteIndicator="False">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="RouteID,EmployeeID" DataMember="RouteEmployeeRecords">
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edEmployeeID" runat="server" AllowEdit="True" CommitChanges="True" DataField="EmployeeID">
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edBAccount__AcctName" runat="server" 
                                        DataField="BAccount__AcctName" Enabled="False">
                                    </px:PXTextEdit>
                                    <px:PXDropDown ID="edBAccount__Status" runat="server" DataField="BAccount__Status">
                                    </px:PXDropDown>
                                    <px:PXNumberEdit ID="edPriorityPreference" runat="server" DataField="PriorityPreference">
                                    </px:PXNumberEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="EmployeeID" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="BAccount__AcctName">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="BAccount__Status">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="PriorityPreference">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    <AutoSize Enabled="True" MinHeight="200" ></AutoSize>                    
                    </px:PXGrid>                                            
                </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="AttributesGrid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;
                        border: 0px;" Width="100%" ActionsPosition="Top" SkinID="Details"  MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Mapping">
                                <RowTemplate>
                                    <px:PXSelector CommitChanges="True" ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True" FilterByAllFields="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="AttributeID" AutoCallBack="true" LinkCommand="CRAttribute_ViewDetails" />
                                    <px:PXGridColumn AllowNull="False" DataField="Description" />
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" SortDirection="Ascending" />
                                    <px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" />
                                    <px:PXGridColumn AllowNull="True" DataField="DefaultValue" RenderEditorText="False" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Days by Week Codes">
                <Template>                        
                    <px:PXFormView ID="WeekCodeFilterForm" runat="server" DataMember="WeekCodeFilter" DataSourceID="ds" SkinID="Transparent" Width="100%" TabIndex="5400">
                        <Template>
                            <px:PXLayoutRule runat="server" ColumnWidth="XS" LabelsWidth="XXS" StartColumn="True" StartRow="True">
                            </px:PXLayoutRule>
                            <px:PXDateTimeEdit ID="edDateBegin" runat="server" CommitChanges="True" DataField="DateBegin">
                            </px:PXDateTimeEdit>
                            <px:PXLayoutRule runat="server" ColumnWidth="XS" LabelsWidth="XXS" StartColumn="True">
                            </px:PXLayoutRule>
                            <px:PXDateTimeEdit ID="edDateEnd" runat="server" CommitChanges="True" DataField="DateEnd">
                            </px:PXDateTimeEdit>
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="PXRouteWeekCodeDates" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" 
                        AllowPaging="True" AdjustPageSize="Auto" TabIndex="11300" FilesIndicator="False" NoteIndicator="False">
                        <ActionBar>
                                <Actions>
                                    <AddNew Enabled="False" />
                                    <Delete Enabled="False" />
                                </Actions>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataKeyNames="WeekCodeDate" DataMember="WeekCodeDateRecords">
                               <RowTemplate>
                                    <px:PXDateTimeEdit ID="edWeekCodeDate" runat="server" DataField="WeekCodeDate" CommitChanges="True">
                                    </px:PXDateTimeEdit>
                                    <px:PXTextEdit ID="edWeekCode" runat="server" DataField="WeekCode">
                                    </px:PXTextEdit>
                                    <px:PXTextEdit ID="edMem_DayOfWeek" runat="server" DataField="Mem_DayOfWeek">
                                    </px:PXTextEdit>
                                    <px:PXTextEdit ID="edMem_WeekOfYear" runat="server" DataField="Mem_WeekOfYear">
                                    </px:PXTextEdit>
                                    <px:PXDateTimeEdit ID="edBeginDateOfWeek" runat="server" DataField="BeginDateOfWeek">
                                    </px:PXDateTimeEdit>
                                </RowTemplate>
			                    <Columns>
                                    <px:PXGridColumn DataField="WeekCodeDate" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="WeekCode" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Mem_DayOfWeek">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Mem_WeekOfYear">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="BeginDateOfWeek">
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
