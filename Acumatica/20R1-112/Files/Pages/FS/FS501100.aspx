<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="FS501100.aspx.cs" Title="Untitled Page" Inherits="Page_FS501100" %>
    <%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FS.ServiceOrderProcess">
            </px:PXDataSource>
        </asp:Content>
        <asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
            <px:PXFormView ID="form" runat="server" DataSourceID="ds" 
                Style="z-index: 100" Width="100%" DataMember="Filter" TabIndex="700" 
                DefaultControlID="">
                <Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M">
                    </px:PXLayoutRule>
					<px:PXDropDown ID="edSOAction" runat="server" DataField="SOAction" CommitChanges="True"/>
					<px:PXSelector ID="edSrvOrdType" runat="server" CommitChanges="True" DataField="SrvOrdType"/>
					<px:PXSelector ID="edBranchID" runat="server" CommitChanges="True" DataField="BranchID"/>
					<px:PXSelector ID="edBranchLocationID" runat="server" CommitChanges="True" DataField="BranchLocationID"/>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M">
                    </px:PXLayoutRule>
					<px:PXSelector ID="edCustomerID" runat="server" CommitChanges="True" DataField="CustomerID"/>
					<px:PXSelector ID="edServiceContractID" runat="server" CommitChanges="True" DataField="ServiceContractID"/>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M">
                    </px:PXLayoutRule>
                    <px:PXDateTimeEdit runat="server" DataField="FromDate" ID="edFromDate" CommitChanges="True">
                    </px:PXDateTimeEdit>
					<px:PXDateTimeEdit runat="server" DataField="ToDate" ID="edToDate" CommitChanges="True">
                    </px:PXDateTimeEdit>
                </Template>
            </px:PXFormView>
        </asp:Content>
        <asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
            <px:PXGrid ID="gridInventoryLines" runat="server" AllowPaging="True" DataSourceID="ds" 
                Style="z-index: 100" Width="100%"
                SkinID="PrimaryInquire" TabIndex="500" SyncPosition="True" BatchUpdate="True">
                <Levels>
                    <px:PXGridLevel DataMember="ServiceOrderRecords">
                        <RowTemplate>
                            <px:PXCheckBox ID="edSelected" runat="server" DataField="Selected" Text="Selected" />
                            <px:PXSelector ID="edRefNbr" runat="server" AllowEdit="True" DataField="RefNbr"/>
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox"/>
							<px:PXGridColumn DataField="BranchID"/>
							<px:PXGridColumn DataField="BranchLocationID"/>
                            <px:PXGridColumn DataField="SrvOrdType"/>                            
                            <px:PXGridColumn DataField="RefNbr"/>
							<px:PXGridColumn DataField="DocDesc"/>
							<px:PXGridColumn DataField="CustomerID"/>
							<px:PXGridColumn DataField="LocationID"/>
							<px:PXGridColumn DataField="OrderDate"/>
							<px:PXGridColumn DataField="Status"/>
							<px:PXGridColumn DataField="WFStageID"/>
							<px:PXGridColumn DataField="ServiceContractID"/>
							<px:PXGridColumn DataField="ScheduleID"/>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                <ActionBar DefaultAction="viewDocument"></ActionBar>
            </px:PXGrid>
        </asp:Content>