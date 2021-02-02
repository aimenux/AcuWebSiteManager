<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN206600.aspx.cs" Inherits="Page_IN206600" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INReplenishmentPolicyMaint" PrimaryView="Policies">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds"
        Style="z-index: 100" Width="100%" DataMember="Policies" 
         
        Caption="Seasonality Summary" 
         NoteIndicator="True" 
		 FilesIndicator="True" 
		ActivityIndicator="True" ActivityField="NoteActivity" 
        DefaultControlID="edReplenishmentPolicyID">
		
	    <Template>
            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
            <px:PXSelector ID="edReplenishmentPolicyID" runat="server" DataField="ReplenishmentPolicyID" DataSourceID="ds" AutoRefresh="True">
                <AutoCallBack Command="Cancel" Target="ds" />
            </px:PXSelector>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" CommitChanges="True"/>
            <px:PXSelector ID="edCalendarID" runat="server" DataField="CalendarID" DataSourceID="ds" CommitChanges="True"/>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="179px" 
		Style="z-index: 100; height: 179px;" Width="100%" Caption="Low Seasons" TabIndex="200" 
		SkinID="Details">
		<Mode InitNewRow="True" AllowUpload="True"/>
	    <Levels>
			<px:PXGridLevel DataMember="seasons" >				
			    <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" />
                    <px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active" />
                    <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" />
                    <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" />
                    <px:PXNumberEdit ID="edFactor" runat="server" DataField="Factor" />
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn AllowNull="False" CommitChanges="True" 
                        DataField="Active" DataType="Boolean" DefValueText="True" TextAlign="Center" 
                        Type="CheckBox" Width="60px" />
                    <px:PXGridColumn DataField="StartDate" DataType="DateTime" Width="120px" CommitChanges="True"/>
                    <px:PXGridColumn DataField="EndDate" DataType="DateTime" Width="120px" CommitChanges="True"/>
                    <px:PXGridColumn AllowNull="False" DataField="Factor" DataType="Decimal" DefValueText="0.0" TextAlign="Right" 
                        Decimals="2" Width="100px" />
                </Columns>
                   <Layout FormViewHeight=""></Layout>				
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
