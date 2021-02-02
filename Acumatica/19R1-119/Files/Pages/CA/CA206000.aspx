<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA206000.aspx.cs" Inherits="Page_AR513000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" PrimaryView="Filter" TypeName="PX.Objects.CA.CCSynchronizeCards" SuspendUnloading="False">
	   <CallbackCommands>
            <px:PXDSCallbackCommand Name="PaymentMethodOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" /> 
            <px:PXDSCallbackCommand Name="setDefaultPaymentMethod" Visible="False" CommitChanges="True" /> 
			<px:PXDSCallbackCommand Name="ViewCustomer" Visible="False" DependOnGrid="grid" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" StartColumn="True"/>
		    <px:PXCheckBox ID="edScheduleSync" runat="server" AlreadyLocalized="False" CommitChanges="True" DataField="ScheduledServiceSync" Text="Schedule Sync">
			</px:PXCheckBox>
		    <px:PXSelector ID="edProcessingCenterId" runat="server" DataField="ProcessingCenterId" CommitChanges="True"></px:PXSelector>
			<px:PXCheckBox ID="edLoadExpiredCards" runat="server" AlreadyLocalized="False" DataField="LoadExpiredCards" CommitChanges="True"></px:PXCheckBox>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="100%" SkinID="Details" TabIndex="100" SyncPosition="true" AdjustPageSize="Auto"> 
		<Levels>
			<px:PXGridLevel DataMember="CustomerCardPaymentData">
			    <RowTemplate>
                	<px:PXSelector ID="edBAccountID" runat="server" AutoRefresh="True" DataField="BAccountID" CommitChanges="True">
					</px:PXSelector>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" Width="60px" TextAlign="Center" Type="CheckBox" AllowCheckAll="True">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="BAccountID" Width="120px" CommitChanges="true" LinkCommand="ViewCustomer">
					</px:PXGridColumn>
                    <px:PXGridColumn DataField="BAccountID_Customer_acctName" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PaymentMethodID" CommitChanges="True">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CashAccountID" CommitChanges="True" >
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerCCPID" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PCCustomerID" Width="120px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PCCustomerDescription" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PCCustomerEmail" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PaymentCCPID" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="FirstName" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="LastName" Width="200px">
                    </px:PXGridColumn>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" />
	</px:PXGrid>
    <px:PXSmartPanel ID="PanelPaymentMethod" runat="server" Caption="Select Payment Method" CaptionVisible="True" LoadOnDemand="True"
        Key="PMFilter" AutoCallBack-Target="formPaymentMethod" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
		CallBackMode-PostData="Page" AlreadyLocalized="False" CreateOnDemand="True" TabIndex="100">
		<div style="padding: 5px">
			<px:PXFormView ID="formPaymentMethod" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="PMFilter">
				<Activity Height="" HighlightColor="" SelectedColor="" Width="" />
				<ContentStyle BackColor="Transparent" BorderStyle="None" />
				<Template>
					<px:PXLayoutRule runat="server" StartRow="True" StartColumn="True"/>
                    <px:PXSelector ID="edPaymentMethodId" runat="server" AutoRefresh = "True" DataField="PaymentMethodId" CommitChanges="true" >
                    </px:PXSelector>
                    <px:PXCheckBox ID="edOverwritePaymentMethod" runat="server" AlignLeft="True" DataField="OverwritePaymentMethod" CommitChanges="true" >
                    </px:PXCheckBox>
				</Template>
			</px:PXFormView>
		</div>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="OK" CommandName="PaymentMethodOk" CommandSourceID="ds" />
		</px:PXPanel>
	</px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelCustomerPaymentSet" runat="server" Caption="Multiple Customer Payment Profiles" Width="80%" Height="500px" CaptionVisible="True" LoadOnDemand="true"
        Key="CustPaymentProfileForDialog" AutoCallBack-Enabled="true" AutoCallBack-Target="formCustomerPaymentSet" AutoCallBack-Command="Refresh" AutoReload="true"
	    TabIndex="100" DesignView="Hidden">
        <px:PXFormView ID="PXFormView2" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="PMFilter" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" StartColumn="True"/>
		    <px:PXTextEdit ID="edCustomerName"  LabelWidth="350px" runat="server" DataField="CustomerName" >
            </px:PXTextEdit>
		</Template>
        <ContentStyle>
            <Margin Left="0px" Right="0px" Bottom="0px" Top="0px" />
            <Padding Left="0px" Right="0px" Bottom="0px" Top="0px" />
        </ContentStyle>
        <CaptionStyle>
            <Margin Left="0px" Right="0px" Bottom="0px" Top="0px" />
            <Padding Left="0px" Right="0px" Bottom="0px" Top="0px" />
        </CaptionStyle>
        </px:PXFormView>
        <px:PXGrid ID="formCustomerPaymentSet" runat="server" DataSourceID="ds" Style="z-index: 100"  BatchUpdate="true"
	  	 Width ="100%" SkinID="Details" TabIndex="100"  AdjustPageSize="Auto"> 
         <ActionBar>
             <Actions>
                 <AddNew ToolBarVisible = "False" MenuVisible="false" />
                 <Refresh ToolBarVisible = "False" MenuVisible="false" />
                 <Delete  ToolBarVisible="False" MenuVisible="false"/>
                 <ExportExcel ToolBarVisible="False"  MenuVisible="false" />
             </Actions>
         </ActionBar>
         <Levels>
			<px:PXGridLevel DataMember="CustPaymentProfileForDialog">
			    <RowTemplate>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" Width="60px" TextAlign="Center" Type="CheckBox" AllowCheckAll="True">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerCCPID" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PCCustomerID" Width="120px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PCCustomerDescription" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PCCustomerEmail" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PaymentCCPID" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PaymentProfileFirstName" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PaymentProfileLastName" Width="200px">
                    </px:PXGridColumn>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize  Enabled="true" />
	    </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
		    <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK"  />
            <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
