<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS205900.aspx.cs" Inherits="Page_FS205900" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" BorderStyle="NotSet" 
        PrimaryView="CalendarWeekCodeRecords" SuspendUnloading="False" 
        TypeName="PX.Objects.FS.CalendarWeekCodeMaint" Visible="True">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="GenerateWeekCode" CommitChanges="True">
            </px:PXDSCallbackCommand>
		    <px:PXDSCallbackCommand Name="Insert" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Delete" Visible="False">
            </px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">

    <px:PXSmartPanel ID="calendarWeekCodeGenerationPanel" runat="server"  Caption="Calendar Week Code Generation Options"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="CalendarWeekCodeGenerationOptions" CreateOnDemand="false" AutoCallBack-Enabled="true"
        AutoCallBack-Target="formChangeID" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnOK" CancelButtonID="btnCancel">
            <px:PXFormView ID="formChangeID" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
                DataMember="CalendarWeekCodeGenerationOptions">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True"  StartRow="True"></px:PXLayoutRule>
                    <px:PXDateTimeEdit ID="edDefaultStartDate" runat="server" DataField="DefaultStartDate">
                    </px:PXDateTimeEdit>
                    <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" CommitChanges="True">
                    </px:PXDateTimeEdit>
                    <px:PXLabel runat="server"> </px:PXLabel>
                    <px:PXLayoutRule runat="server" StartColumn="True"></px:PXLayoutRule>
                    <px:PXTextEdit ID="edInitialWeekCode" runat="server" DataField="InitialWeekCode" Width="100px">
                    </px:PXTextEdit>
                    <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" CommitChanges="True">
                    </px:PXDateTimeEdit>
                    <px:PXLayoutRule runat="server"  StartRow="True" Merge="True">
                    </px:PXLayoutRule>
                    <px:PXButton runat="server" ID="btnOK" DialogResult="OK" Text="Generate Week Codes"  
                         Width="200px" AlignLeft="True">
                    </px:PXButton>
                    <px:PXButton runat="server" ID="btnCancel" DialogResult="Cancel" Text="Close" AlignLeft="True">
                    </px:PXButton>
                </Template>
            </px:PXFormView>
    </px:PXSmartPanel>

    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" 
        SkinID="Inquire" TabIndex="1900" >
		<Levels>
			<px:PXGridLevel DataMember="CalendarWeekCodeRecords" DataKeyNames="WeekCodeDate">
			    <RowTemplate>
                    <px:PXDateTimeEdit ID="edWeekCodeDate" runat="server" DataField="WeekCodeDate" CommitChanges="True">
                    </px:PXDateTimeEdit>
                    <px:PXTextEdit ID="edWeekCode" runat="server" DataField="WeekCode">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edWeekCodeP1" runat="server" DataField="WeekCodeP1">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edWeekCodeP2" runat="server" DataField="WeekCodeP2">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edWeekCodeP3" runat="server" DataField="WeekCodeP3">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edWeekCodeP4" runat="server" DataField="WeekCodeP4">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edMem_DayOfWeek" runat="server" DataField="Mem_DayOfWeek">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edMem_WeekOfYear" runat="server" DataField="Mem_WeekOfYear">
                    </px:PXTextEdit>
                    <px:PXDateTimeEdit ID="edBeginDateOfWeek" runat="server" DataField="BeginDateOfWeek">
                    </px:PXDateTimeEdit>
                    <px:PXDateTimeEdit ID="edEndDateOfWeek" runat="server" DataField="EndDateOfWeek">
                    </px:PXDateTimeEdit>
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn DataField="WeekCodeDate" CommitChanges="True">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="WeekCode" CommitChanges="True">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="WeekCodeP1">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="WeekCodeP2">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="WeekCodeP3">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="WeekCodeP4">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Mem_DayOfWeek">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Mem_WeekOfYear">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="BeginDateOfWeek">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="EndDateOfWeek">
                    </px:PXGridColumn>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
	    <Mode AllowUpload="True" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
