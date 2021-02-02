<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" 
ValidateRequest="false" CodeFile="FS303800.aspx.cs" Inherits="Page_FS303800" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%" Visible="True" SuspendUnloading="False"
        TypeName="PX.Objects.FS.RouteSequenceMaint"
        PrimaryView="Filter">
        <CallbackCommands>  
            <px:PXDSCallbackCommand Name="Resequence" CommitChanges="True" ></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Save" Visible="True" DependOnGrid="grid" CommitChanges="True" ></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenContract" Visible="False" DependOnGrid="grid" CommitChanges="True"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenSchedule" Visible="False" DependOnGrid="grid" CommitChanges="True"></px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter" TabIndex="3700" DefaultControlID="edRouteID">
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" 
                ControlSize="M" LabelsWidth="S">
            </px:PXLayoutRule>
            <px:PXSelector ID="edRouteID" runat="server" 
                DataField="RouteID" CommitChanges="True">
            </px:PXSelector>
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="S">
            </px:PXLayoutRule>
            <px:PXCheckBox ID="edServiceContractFlag" runat="server" 
                DataField="ServiceContractFlag" AlignLeft="True" CommitChanges="True">
            </px:PXCheckBox>
            <px:PXCheckBox ID="edScheduleFlag" runat="server" DataField="ScheduleFlag" 
                AlignLeft="True" CommitChanges="True">
            </px:PXCheckBox>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Style="z-index: 100" Width="100%" DataSourceID="ds" 
        AllowPaging="True" AdjustPageSize="Auto" SkinID="Inquire" TabIndex="3000" AllowSearch="True"  
        ActionsPosition="None" >
		<Levels>
			<px:PXGridLevel DataMember="ServiceContracts" DataKeyNames="ScheduleID">
			    <RowTemplate>
                    <px:PXTextEdit ID="edGlobalSequence" runat="server" 
                        DataField="GlobalSequence" AllowEdit = "True">
                    </px:PXTextEdit>
                    <px:PXSegmentMask ID="edFSServiceContract__CustomerID" runat="server" 
                        DataField="FSServiceContract__CustomerID" AllowEdit="true">
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edFSServiceContract__CustomerLocationID" runat="server" DataField="FSServiceContract__CustomerLocationID">
                    </px:PXSegmentMask>
                    <px:PXTextEdit ID="edLocation__Descr" runat="server" DataField="Location__Descr">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edAddress__AddressLine1" runat="server" DataField="Address__AddressLine1">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edAddress__City" runat="server" DataField="Address__City">
                    </px:PXTextEdit>
                    <px:PXSelector ID="edAddress__State" runat="server" DataField="Address__State">
                    </px:PXSelector>
                    <px:PXSelector ID="edFSServiceContract__RefNbr" runat="server" 
                        DataField="FSServiceContract__RefNbr" AllowEdit = "True">
                    </px:PXSelector>
                    <px:PXTextEdit ID="edFSServiceContract__CustomerContractNbr" runat="server" 
                        DataField="FSServiceContract__CustomerContractNbr">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edFSServiceContract__DocDesc" runat="server" 
                        DataField="FSServiceContract__DocDesc">
                    </px:PXTextEdit>
                    <px:PXDropDown ID="edFSServiceContract__Status" runat="server" 
                        DataField="FSServiceContract__Status">
                    </px:PXDropDown>
                    <px:PXSelector ID="edFSSchedule__RefNbr" runat="server" 
                        DataField="FSSchedule__RefNbr" AllowEdit = "True">
                    </px:PXSelector>
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn DataField="GlobalSequence" Width="180px" DisplayMode="Text">
                    </px:PXGridColumn>  
                    <px:PXGridColumn DataField="FSServiceContract__CustomerID" Width="200px" DisplayMode="Hint">
                    </px:PXGridColumn>                                      
                    <px:PXGridColumn DataField="FSServiceContract__CustomerLocationID" Width="120px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Location__Descr" Width="120px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Address__AddressLine1" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Address__City" Width="100px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Address__State" Width="100px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="FSServiceContract__RefNbr" Width="100px" LinkCommand="OpenContract">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="FSServiceContract__CustomerContractNbr" Width="100px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="FSServiceContract__DocDesc" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="FSServiceContract__Status">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="FSSchedule__RefNbr" LinkCommand="OpenSchedule" Width="90px">
                    </px:PXGridColumn>
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowUpload="True" AllowAddNew="False" />
	</px:PXGrid>
</asp:Content>
