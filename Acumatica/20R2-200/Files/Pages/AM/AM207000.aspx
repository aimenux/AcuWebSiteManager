<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM207000.aspx.cs" Inherits="Page_AM207000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" BorderStyle="NotSet" PrimaryView="WCRecords" TypeName="PX.Objects.AM.WCMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		    <px:PXDSCallbackCommand Name="MassUpdate" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewBOM" Visible="False" DependOnGrid="gridWhereUsed" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="WCRecords" DataKeyNames="WcID" DefaultControlID="edWcID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
            <px:PXSelector ID="edWcID" runat="server" DataField="WcID" CommitChanges="True" MaxLength="10" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" MaxLength="60"  />
            <px:PXCheckBox ID="chkActiveFlg" runat="server" DataField="ActiveFlg" />
            <px:PXCheckBox ID="chkOutsideFlg" runat="server" DataField="OutsideFlg" />
            <px:PXSegmentMask CommitChanges="true" ID="edSiteId" runat="server" DataField="SiteID" />
            <px:PXSelector ID="edLocationID" runat="server" DataField="LocationID" />
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="M" />
            <px:PXNumberEdit ID="edStdCost" runat="server" DataField="StdCost" />
            <px:PXDropDown ID="edWcBasis" runat="server" AllowNull="False" DataField="WcBasis"/>
            <px:PXDropDown ID="edScrapAction" runat="server" DataField="ScrapAction"  />
            <px:PXCheckBox ID="chkBflushMatl" runat="server" DataField="BflushMatl" />
            <px:PXCheckBox ID="chkBflushLbr" runat="server" DataField="BflushLbr" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="WCRecordsSelected">
		<Items>
			<px:PXTabItem Text="Shift Info">
                <Template>
                    <px:PXGrid ID="grid" runat="server" AdjustPageSize="Auto" AllowPaging="True" AllowSearch="true" 
                        DataSourceID="ds" SkinID="DetailsInTab" Width="100%">
                        <ActionBar ActionsText="False">
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataKeyNames="ShiftID,WcID" DataMember="WCShifts">
                                <RowTemplate>
                                    <px:PXSelector ID="edShiftID" runat="server" DataField="ShiftID" MaxLength="4"  />
                                    <px:PXNumberEdit ID="edCrewSize" runat="server" AllowNull="False" DataField="CrewSize" />
                                    <px:PXNumberEdit ID="edMachNbr" runat="server" AllowNull="False" DataField="MachNbr" />
                                    <px:PXNumberEdit ID="edShftEff" runat="server" AllowNull="False" DataField="ShftEff"  />
                                    <px:PXSelector ID="edCalendarID" runat="server" DataField="CalendarID" AllowEdit="True"/>
                                    <px:PXDropDown ID="edDiffType" runat="server" DataField="AMShiftMst__DiffType" />
                                    <px:PXNumberEdit ID="edShftDiff" runat="server" DataField="AMShiftMst__ShftDiff"  />
                                    <px:PXSelector ID="edLaborCodeID" runat="server" DataField="LaborCodeID" AllowEdit="True"/>
                                    <px:PXTextEdit ID="edWcID" runat="server" Visible="false" AllowNull="False" DataField="WcID"  MaxLength="10" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ShiftID" Width="100px" />
                                    <px:PXGridColumn AllowNull="False" DataField="CrewSize" Label="Crew Size" TextAlign="Right" Width="90px" />
                                    <px:PXGridColumn AllowNull="False" DataField="MachNbr" Label="Machines" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowNull="False" DataField="ShftEff" Label="Efficiency" TextAlign="Right" Width="67px" />
                                    <px:PXGridColumn DataField="CalendarID" Width="100px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AMShiftMst__DiffType" Label="Diff Type" MaxLength="1" RenderEditorText="True" Width="80px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AMShiftMst__ShftDiff" Label="Differential" TextAlign="Right" Width="117px" />
                                    <px:PXGridColumn DataField="LaborCodeID" Label="Labor Code" Width="150px" />
                                    <px:PXGridColumn DataField="WcID" Label="WcID" MaxLength="10" Visible="False" Width="117px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True"/>
                    </px:PXGrid>
                </Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Overhead">
                <Template>
                    <px:PXGrid ID="gridOverhead" runat="server" AdjustPageSize="Auto" AllowPaging="True" AllowSearch="true"
                        DataSourceID="ds" SkinID="DetailsInTab" width="100%" MatrixMode="true">
                        <ActionBar ActionsText="False">
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="WCOverheads" DataKeyNames="OvhdID,WcID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edOvhdID" runat="server" DataField="OvhdID" AllowEdit="True"/>
                                    <px:PXTextEdit ID="edAMOverhead__Descr" runat="server" DataField="AMOverhead__Descr" />
                                    <px:PXDropDown ID="edAMOverhead__OvhdType" runat="server" DataField="AMOverhead__OvhdType" />
                                    <px:PXNumberEdit ID="edOFactor" runat="server" DataField="OFactor"  />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="OvhdID" Width="200px" CommitChanges="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AMOverhead__Descr" Width="351px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AMOverhead__OvhdType" RenderEditorText="True" Width="198px" />
                                    <px:PXGridColumn DataField="OFactor" Label="OFactor" TextAlign="Right" Width="115px" />
                                    <px:PXGridColumn DataField="WcID" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True"/>
                    </px:PXGrid>
                </Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Machines">
                <Template>
                    <px:PXGrid ID="gridMachines" runat="server" AllowPaging="True" SyncPosition="True" 
                        DataSourceID="ds" SkinID="DetailsInTab" Width="100%">
                        <ActionBar ActionsText="False">
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataKeyNames="WcID,MachID" DataMember="WCMachines">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edMachID" runat="server" DataField="MachID" AllowEdit="True" />
                                    <px:PXCheckBox ID="edMachineOverride" runat="server" DataField="MachineOverride" />
                                    <px:PXNumberEdit ID="edStdCost" runat="server" DataField="StdCost"  />
                                    <px:PXSegmentMask ID="edMachAcctID" runat="server" Datafield="MachAcctID"  />
                                    <px:PXSegmentMask ID="edMachSubID" runat="server" DataField="MachSubID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="MachID" Label="Machine ID" Width="200px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="MachineOverride" TextAlign="Center" Type="CheckBox" Width="80px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="StdCost" Width="99px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="MachAcctID" Width="130px" />
                                    <px:PXGridColumn DataField="MachSubID" Width="140px" />
                                    <px:PXGridColumn DataField="WcID" />
                                </Columns>	
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True"/>
                    </px:PXGrid>
                </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Where Used">
                <Template>
                    <px:PXGrid ID="gridWhereUsed" runat="server" DataSourceID="ds" Height="400px" SkinID="Inquire" Width="100%" 
                        SyncPosition="True" AdjustPageSize="Auto" AllowSearch="True" AllowPaging="True" >
                        <Levels>
                            <px:PXGridLevel DataMember="WCWhereUsed" DataKeyNames="BOMID,OperationID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edBOMID" runat="server" DataField="AMBomItem__BOMID" />
                                    <px:PXSelector ID="edRevisionID" runat="server" DataField="AMBomItem__RevisionID" />
                                    <px:PXTextEdit ID="edOperationCD" runat="server" AllowNull="False" DataField="OperationCD"  />
                                    <px:PXDateTimeEdit ID="edEffStartDate" runat="server" DataField="AMBomItem__EffStartDate" />
                                    <px:PXDateTimeEdit ID="edEffEndDate" runat="server" DataField="AMBomItem__EffEndDate" />
                                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="AMBomItem__InventoryID" AllowEdit="True"/>
                                    <px:PXSelector ID="edGridSiteID" runat="server" DataField="AMBomItem__SiteID" AllowEdit="True" />
                                    <px:PXTextEdit ID="edStatus" runat="server" DataField="AMBomItem__Status" />
                                    <px:PXTextEdit ID="edGridDescr" runat="server" DataField="Descr" />
                                    <px:PXDropDown ID="edGridScrapAction" runat="server" DataField="ScrapAction" />
                                    <px:PXCheckBox ID="edBFlush" runat="server" DataField="BFlush" />
                                    <px:PXCheckBox ID="chkOutsideProcess" runat="server" DataField="OutsideProcess" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Selected" AllowCheckAll="True" TextAlign="Center" DataType="Boolean" Type="CheckBox" Width="90px" />
                                    <px:PXGridColumn DataField="AMBomItem__BOMID" Width="100px" LinkCommand="ViewBOM" />
                                    <px:PXGridColumn DataField="AMBomItem__RevisionID" Width="85px" />
                                    <px:PXGridColumn DataField="OperationCD" Width="80px" />
                                    <px:PXGridColumn DataField="AMBomItem__EffStartDate" Width="85px"/>
                                    <px:PXGridColumn DataField="AMBomItem__EffEndDate" Width="85px" />
                                    <px:PXGridColumn DataField="AMBomItem__InventoryID" Width="100px" />
                                    <px:PXGridColumn DataField="AMBomItem__SiteID" Width="100px" />
                                    <px:PXGridColumn DataField="AMBomItem__Status" TextAlign="Center" />
                                    <px:PXGridColumn DataField="Descr" Width="120px" />
                                    <px:PXGridColumn DataField="ScrapAction" Width="80px" />
                                    <px:PXGridColumn DataField="BFlush" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="OutsideProcess" TextAlign="Center" Type="CheckBox" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    <ActionBar>
                        <CustomItems>
                            <px:PXToolBarButton Text="Mass Update" >
                                <AutoCallBack  Command="MassUpdate" Target="ds" />
                            </px:PXToolBarButton>
                        </CustomItems>
                    </ActionBar>       
                        <AutoSize Enabled="True"/>
                    </px:PXGrid>s
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Substitute Work Centers">
                <Template>
                    <px:PXGrid ID="gridSubstituteWorkCenters" runat="server" AllowPaging="True" SyncPosition="True" 
                        DataSourceID="ds" SkinID="DetailsInTab" Width="100%">
                        <ActionBar ActionsText="False">
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="SubstituteWorkCenters">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True" />
                                    <px:PXSelector ID="edSubstituteWcID" runat="server" DataField="SubstituteWcID" AllowEdit="True" />
                                    <px:PXCheckBox ID="edUpdateOperDesc" runat="server" DataField="UpdateOperDesc" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SiteID" Width="150px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SubstituteWcID" Width="150px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="UpdateOperDesc" TextAlign="Center" Type="CheckBox" Width="150px" />
                                    <px:PXGridColumn DataField="WcID" Width="150px" />
                                </Columns>	
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True"/>
                    </px:PXGrid>
                </Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
    <px:PXSmartPanel runat="server" ID="UpdateOperationPanel" Height="195px" Width="315px" LoadOnDemand="True" CaptionVisible="True" Caption="Operation Mass Update" Key="MassUpdateFilter">
        <px:PXFormView runat="server" ID="UpdateOperationForm" SkinID="Transparent" CaptionVisible="False" Width="100%" DataSourceID="ds" DataMember="MassUpdateFilter">
            <Template>
                <px:PXLayoutRule runat="server" ID="panelrule1" GroupCaption="Fields" StartColumn="True" LabelsWidth="XS" ControlSize="M" />
                <px:PXCheckBox runat="server" DataField="BFlush" ID="panelBFlush" CommitChanges="True" />
                <px:PXCheckBox runat="server" DataField="ScrapAction" ID="panelScrapAction" CommitChanges="True" />
                <px:PXCheckBox runat="server" DataField="OperDescription" ID="panelOperDescription" CommitChanges="True" />
                <px:PXCheckBox runat="server" DataField="OutsideProcess" ID="panelOutsideProcess" CommitChanges="True" />
            </Template>
        </px:PXFormView>
        <px:PXPanel runat="server" ID="MassUpdateButtonPanel" SkinID="Buttons">
            <px:PXButton runat="server" ID="UpdateOperation1" Text="Update" DialogResult="OK" CommandSourceID="ds" />
            <px:PXButton runat="server" ID="UpdateOperation2" Text="Cancel" DialogResult="Cancel" CommandSourceID="ds" /></px:PXPanel></px:PXSmartPanel>
        <px:PXSmartPanel ID="CalendarPanel" runat="server" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="CalendarForm" Caption="Calculated Calendar" CaptionVisible="True" Key="CalendarInquiryFilter"
        DesignView="Content" Height="280px" Width="725px" LoadOnDemand="true">
        <px:PXFormView ID="CalendarForm" runat="server" DataSourceID="ds" CaptionVisible="False"
            DataMember="CalendarInquiryFilter" SkinID="Transparent" Width="100%">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XXS" LabelsWidth="XXS" />
				<px:PXLabel ID="LL1" runat="server">Date </px:PXLabel>
				<px:PXDateTimeEdit width="85px" ID="edWcCal1Date" runat="server" DataField="Day1Date" SuppressLabel="True" />
				<px:PXDateTimeEdit width="85px" ID="edWcCal2Date" runat="server" DataField="Day2Date" SuppressLabel="True" />
				<px:PXDateTimeEdit width="85px" ID="edWcCal3Date" runat="server" DataField="Day3Date" SuppressLabel="True" />
				<px:PXDateTimeEdit width="85px" ID="edWcCal4Date" runat="server" DataField="Day4Date" SuppressLabel="True" />
				<px:PXDateTimeEdit width="85px" ID="edWcCal5Date" runat="server" DataField="Day5Date" SuppressLabel="True" />
				<px:PXDateTimeEdit width="85px" ID="edWcCal6Date" runat="server" DataField="Day6Date" SuppressLabel="True" />
				<px:PXDateTimeEdit width="85px" ID="edWcCal7Date" runat="server" DataField="Day7Date" SuppressLabel="True" />
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XS" LabelsWidth="XXS" />
				<px:PXLabel ID="LL2" runat="server">Day of week </px:PXLabel>
                <px:PXTextEdit width="85px" ID="edWcCal1DayOfWeek" runat="server" DataField="Day1DayOfWeek" SuppressLabel="True" />
                <px:PXTextEdit width="85px" ID="edWcCal2DayOfWeek" runat="server" DataField="Day2DayOfWeek" SuppressLabel="True" />
                <px:PXTextEdit width="85px" ID="edWcCal3DayOfWeek" runat="server" DataField="Day3DayOfWeek" SuppressLabel="True" />
                <px:PXTextEdit width="85px" ID="edWcCal4DayOfWeek" runat="server" DataField="Day4DayOfWeek" SuppressLabel="True" />
                <px:PXTextEdit width="85px" ID="edWcCal5DayOfWeek" runat="server" DataField="Day5DayOfWeek" SuppressLabel="True" />
                <px:PXTextEdit width="85px" ID="edWcCal6DayOfWeek" runat="server" DataField="Day6DayOfWeek" SuppressLabel="True" />
                <px:PXTextEdit width="85px" ID="edWcCal7DayOfWeek" runat="server" DataField="Day7DayOfWeek" SuppressLabel="True" />
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XXS" LabelsWidth="XXS" />
				<px:PXLabel ID="LL3" runat="server"> Start time </px:PXLabel>
				<px:PXDateTimeEdit width="70px" ID="edWcCal1StartTime" runat="server" DataField="Day1StartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
				<px:PXDateTimeEdit width="70px" ID="edWcCal2StartTime" runat="server" DataField="Day2StartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
				<px:PXDateTimeEdit width="70px" ID="edWcCal3StartTime" runat="server" DataField="Day3StartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
				<px:PXDateTimeEdit width="70px" ID="edWcCal4StartTime" runat="server" DataField="Day4StartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
				<px:PXDateTimeEdit width="70px" ID="edWcCal5StartTime" runat="server" DataField="Day5StartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
				<px:PXDateTimeEdit width="70px" ID="edWcCal6StartTime" runat="server" DataField="Day6StartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
				<px:PXDateTimeEdit width="70px" ID="edWcCal7StartTime" runat="server" DataField="Day7StartTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XXS" LabelsWidth="XXS" />
				<px:PXLabel ID="LL4" runat="server"> End time </px:PXLabel>
				<px:PXDateTimeEdit width="70px" ID="edWcCal1EndTime" runat="server" DataField="Day1EndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
				<px:PXDateTimeEdit width="70px" ID="edWcCal2EndTime" runat="server" DataField="Day2EndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
				<px:PXDateTimeEdit width="70px" ID="edWcCal3EndTime" runat="server" DataField="Day3EndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
				<px:PXDateTimeEdit width="70px" ID="edWcCal4EndTime" runat="server" DataField="Day4EndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
				<px:PXDateTimeEdit width="70px" ID="edWcCal5EndTime" runat="server" DataField="Day5EndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
				<px:PXDateTimeEdit width="70px" ID="edWcCal6EndTime" runat="server" DataField="Day6EndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
				<px:PXDateTimeEdit width="70px" ID="edWcCal7EndTime" runat="server" DataField="Day7EndTime" DisplayFormat="t" EditFormat="t" TimeMode="True" SuppressLabel="True" />
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XS" LabelsWidth="XXS" />
                <px:PXLabel ID="LL5" runat="server"> Work time </px:PXLabel>
                <px:PXTimeSpan Width="55px" ID="edWcCal1WorkTime" runat="server" DataField="Day1WorkTime" InputMask="hh:mm" SuppressLabel="True"/>
                <px:PXTimeSpan Width="55px" ID="edWcCal2WorkTime" runat="server" DataField="Day2WorkTime" InputMask="hh:mm" SuppressLabel="True"/>
                <px:PXTimeSpan Width="55px" ID="edWcCal3WorkTime" runat="server" DataField="Day3WorkTime" InputMask="hh:mm" SuppressLabel="True"/>
                <px:PXTimeSpan Width="55px" ID="edWcCal4WorkTime" runat="server" DataField="Day4WorkTime" InputMask="hh:mm" SuppressLabel="True"/>
                <px:PXTimeSpan Width="55px" ID="edWcCal5WorkTime" runat="server" DataField="Day5WorkTime" InputMask="hh:mm" SuppressLabel="True"/>
                <px:PXTimeSpan Width="55px" ID="edWcCal6WorkTime" runat="server" DataField="Day6WorkTime" InputMask="hh:mm" SuppressLabel="True"/>
                <px:PXTimeSpan Width="55px" ID="edWcCal7WorkTime" runat="server" DataField="Day7WorkTime" InputMask="hh:mm" SuppressLabel="True"/>
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XS" LabelsWidth="XXS" />
                <px:PXLabel ID="LL6" runat="server"> Break time </px:PXLabel>
                <px:PXTimeSpan Width="55px" ID="edWcCal1BreakTime" runat="server" DataField="Day1BreakTime" InputMask="hh:mm" SuppressLabel="True"/>
                <px:PXTimeSpan Width="55px" ID="edWcCal2BreakTime" runat="server" DataField="Day2BreakTime" InputMask="hh:mm" SuppressLabel="True"/>
                <px:PXTimeSpan Width="55px" ID="edWcCal3BreakTime" runat="server" DataField="Day3BreakTime" InputMask="hh:mm" SuppressLabel="True"/>
                <px:PXTimeSpan Width="55px" ID="edWcCal4BreakTime" runat="server" DataField="Day4BreakTime" InputMask="hh:mm" SuppressLabel="True"/>
                <px:PXTimeSpan Width="55px" ID="edWcCal5BreakTime" runat="server" DataField="Day5BreakTime" InputMask="hh:mm" SuppressLabel="True"/>
                <px:PXTimeSpan Width="55px" ID="edWcCal6BreakTime" runat="server" DataField="Day6BreakTime" InputMask="hh:mm" SuppressLabel="True"/>
                <px:PXTimeSpan Width="55px" ID="edWcCal7BreakTime" runat="server" DataField="Day7BreakTime" InputMask="hh:mm" SuppressLabel="True"/>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="calendarPanelButtons" runat="server" SkinID="Buttons" Width="95%">
            <px:PXButton ID="btnCalendarPanelOk" runat="server" DialogResult="OK" Text="OK" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>