<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA201000.aspx.cs" Inherits="Page_CA201000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server"  Visible="True" Width="100%" PrimaryView="PaymentType" TypeName="PX.Objects.CA.PaymentTypeMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Height="257px" 
		Style="z-index: 100" Width="100%"  
		DataMember="PaymentType" Caption="Payment Type" DefaultControlID="" 
		 NoteIndicator="True" 
		 FilesIndicator="True" 
		ActivityIndicator="true" ActivityField="NoteActivity" >
<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        
		<Template>
			<px:PXLabel ID="lblDescr" runat="server" Style="z-index: 102; left: 9px; position: absolute;
				top: 36px" width="115px">Description :</px:PXLabel>
			<px:PXLabel ID="lblPaymentTypeID" runat="server" Style="z-index: 100; left: 9px;
				position: absolute; top: 9px; width: 115px;">Payment Type ID :</px:PXLabel>
			<px:PXSelector ID="edPaymentTypeID" runat="server" AllowNull="True" DataField="PaymentTypeID"
				   LabelID="lblPaymentTypeID"
				 Style="z-index: 101; left: 129px; position: absolute; top: 10px"
				TabIndex="5" Width="81px">
				<AutoCallBack Command="Cancel" Enabled="True" Target="ds">
				</AutoCallBack>
				<GridProperties>
					
					<Layout ColumnsMenu="False" />
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
			</px:PXSelector>
			
			<px:PXCheckBox ID="chkAllowInstances" runat="server" DataField="AllowInstances" Style="z-index: 100;
				left: 552px; position: absolute; top: 91px" TabIndex="35" Text="May Have Cards">
				<AutoCallBack Command="Save" Enabled="True" Target="form">
				</AutoCallBack>
			</px:PXCheckBox>			
			<px:PXTextEdit ID="edDescr" runat="server" AllowNull="True" DataField="Descr" LabelID="lblDescr"
				 Style="z-index: 103; left: 129px; position: absolute; top: 37px"
				TabIndex="10" Width="180px">
			</px:PXTextEdit>
			<px:PXPanel ID="pnlBatchSettings" runat="server" 
                Caption="Batch Export Settings" Style="left: 399px; position: absolute; top: 117px; margin-top: 0px;
						margin-left: 0px; height: 54px; width: 396px;" >
			
				<px:PXLabel ID="lblBatchExportSYMappingID" runat="server" 
				EnableClientScript="False" 
				style="z-index:101;position:absolute;left:4px; top:5px;">Batch Export Schenario:</px:PXLabel>
				<px:PXSelector ID="edBatchExportSYMappingID" runat="server" 
				DataField="BatchExportSYMappingID"   
				 LabelID="lblBatchExportSYMappingID" 
				style="z-index:102;position:absolute;left:148px; top:5px;" TabIndex="24" 
				Width="108px">
				<AutoCallBack Command="Save" Enabled="True" Target="form">
				</AutoCallBack>
				
				</px:PXSelector>
			</px:PXPanel>
			<px:PXCheckBox ID="chkPrintChecks" runat="server" DataField="PrintChecks" Style="z-index: 100; left: 129px; position: absolute;
					top: 64px; width: 140px;" TabIndex="15" Text="Print Checks">
				<AutoCallBack Command="Save" Enabled="True" Target="form">
				</AutoCallBack>
			</px:PXCheckBox>
			<px:PXPanel ID="pnlPrintCheck" runat="server" 
                Caption="Check Printing Settings" Style="left: 3px; position: absolute; top: 117px; margin-top: 0px;
						margin-left: 0px; height: 107px; width: 387px; margin-bottom: 0px;" >
			
				<px:PXLabel ID="lblStubLines" runat="server" Style="z-index: 100; left:4px; position: absolute;
					top: 32px; width: 90px;" EnableClientScript="False">Lines per Stab :</px:PXLabel>
				<px:PXLabel ID="lblReportID" runat="server" Style="z-index: 100; left: 4px; position: absolute;
					top: 5px; width: 99px;">Report ID :</px:PXLabel>
				<px:PXLabel ID="lblReportIDH" runat="server" Style="z-index: 100; left: 220px; position: absolute;
					top: 5px; width: 153px; height: 18px;"></px:PXLabel>
				<px:PXSelector ID="edReportID" runat="server" DataField="ReportID" 
					   LabelID="lblReportID"
					 Style="z-index: 101; left: 121px; position: absolute; top: 5px; width: 90px;"
					TabIndex="20" HintField="Title" HintLabelID="lblReportIDH" 
					AutoGenerateColumns="true" ValueField="ScreenID">
					<GridProperties>
					
					<Layout ColumnsMenu="False" />
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
					<AutoCallBack Command="Save" Target="form" Enabled="True"></AutoCallBack>
				</px:PXSelector>
				<px:PXNumberEdit ID="edStubLines" runat="server" AllowNull="False" 
					DataField="StubLines" LabelID="lblStubLines"   
					Style="z-index: 107; left: 121px; position: absolute; top: 32px" TabIndex="21" 
					ValueType="Int16" Width="90px">
					<AutoCallBack Command="Save" Enabled="True" Target="form">
					</AutoCallBack>
				</px:PXNumberEdit>
				<px:PXCheckBox ID="chkPrintRemittanceReport" runat="server" 
					DataField="PrintRemittanceReport" Style="z-index: 100; left: 121px; position: absolute;
						top: 58px; width: 216px;" TabIndex="22" Text="Print Checks">
					<AutoCallBack Command="Save" Enabled="True" Target="form">
					</AutoCallBack>
				</px:PXCheckBox>	
				<px:PXLabel ID="lblRemittanceReportID" runat="server" Style="z-index: 100; left: 4px; position: absolute;
					top: 85px; width: 99px;">Report ID :</px:PXLabel>
				<px:PXLabel ID="lblRemittanceReportIDH" runat="server" Style="z-index: 100; left: 220px; position: absolute;
					top: 85px; width: 153px; height: 18px;"></px:PXLabel>
				<px:PXSelector ID="edRemittanceReportID" runat="server" 
					DataField="RemittanceReportID" 
					   LabelID="lblRemittanceReportID"
					 Style="z-index: 101; left: 121px; position: absolute; top: 85px; width: 90px;"
					TabIndex="23" HintField="Title" HintLabelID="lblRemittanceReportIDH" 
					AutoGenerateColumns="true" ValueField="ScreenID">
					<GridProperties>
					
					<Layout ColumnsMenu="False" />
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
					<AutoCallBack Command="Save" Target="form" Enabled="True"></AutoCallBack>
				</px:PXSelector>
			</px:PXPanel>
			<px:PXCheckBox ID="chkCreateBatch" runat="server" DataField="CreateBatch" 
				style="z-index:100;position:absolute;left:129px; top:91px; width: 99px;" 
				TabIndex="24" Text="Create Batch">
				<AutoCallBack Command="Save" Enabled="True" Target="form">
				</AutoCallBack>
			</px:PXCheckBox>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXTab ID="tab" runat="server" Height="200px" Style="z-index: 100;" Width="100%"
		TabIndex="20" SelectedIndex="1" DataMember="CurrentPaymentType"
		DataSourceID="ds"   BorderStyle="None">
		<Items>
		    <px:PXTabItem Text="Cash Account Settings">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
                        Width="100%"  ActionsPosition="Top" TabIndex="20"
                        SkinID="Details">
                        <Levels>
                            <px:PXGridLevel  DataMember="DetailsForCashAccount">
                                <Columns>
                                    <px:PXGridColumn AllowUpdate="False" AutoGenerateOption="NotSet" 
                                        DataField="DetailID" DataType="Int32" Label="ID" TextAlign="Right" Width="63px">
                                        <Header Text="ID">
                                        </Header>
                                    </px:PXGridColumn>                                   
                                    <px:PXGridColumn DataField="Descr"  Width="140px">
                                        <Header Text="Description">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn AllowNull="False" DataField="IsRequired" DataType="Boolean" DefValueText="False"
                                        TextAlign="Center" Type="CheckBox">
                                        <Header Text="IsRequired">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn AllowNull="False" DataField="IsEncrypted" DataType="Boolean" DefValueText="False"
                                        TextAlign="Center" Type="CheckBox">
                                        <Header Text="Is Encripted">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn AllowNull="False" DataField="IsIdentifier" DataType="Boolean" DefValueText="False"
                                        TextAlign="Center" Type="CheckBox">
                                        <Header Text="IsIdentifier">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="OrderIndex" DataType="Int16" TextAlign="Right" Width="54px">
                                        <Header Text="Order Index">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EntryMask"  Width="180px">
                                        <Header Text="EntryMask">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ValidRegexp"  Width="250px">
                                        <Header Text="Validation Reg. Exp.">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="DisplayMask"  Width="180px">
                                        <Header Text="Display Mask">
                                        </Header>
                                    </px:PXGridColumn>                                    
                                </Columns>
                                <RowTemplate>
                                    <px:PXLabel ID="lblDetailID" runat="server" Style="z-index: 100; left: 9px; position: absolute;
                                        top: 9px">ID :</px:PXLabel>
                                    <px:PXTextEdit ID="edDetailID" runat="server" AllowNull="True" DataField="DetailID"
                                        LabelID="lblDetailID"  Style="z-index: 101; left: 126px; position: absolute;
                                        top: 9px" TabIndex="10" Width="63px">
                                    </px:PXTextEdit>
                                    <px:PXLabel ID="lblEntryMask" runat="server" Style="z-index: 102; left: 9px; position: absolute;
                                        top: 36px">EntryMask :</px:PXLabel>
                                    <px:PXTextEdit ID="edEntryMask" runat="server" AllowNull="True" DataField="EntryMask"
                                        LabelID="lblEntryMask"  Style="z-index: 103; left: 126px; position: absolute;
                                        top: 36px" TabIndex="30" Width="300px">
                                    </px:PXTextEdit>
                                    <px:PXLabel ID="lblValidRegexp" runat="server" Style="z-index: 104; left: 9px; position: absolute;
                                        top: 63px">Validation Regexp :</px:PXLabel>
                                    <px:PXTextEdit ID="edValidRegexp" runat="server" AllowNull="True" DataField="ValidRegexp"
                                        LabelID="lblValidRegexp"  Style="z-index: 105; left: 126px; position: absolute;
                                        top: 63px" TabIndex="35" Width="500px">
                                    </px:PXTextEdit>
                                    <px:PXLabel ID="lblDisplayMask" runat="server" Style="z-index: 100; position: absolute;
                                        left: 9px; top: 9px;">Display Mask :</px:PXLabel>
                                    <px:PXTextEdit ID="edDisplayMask" runat="server" DataField="DisplayMask" LabelID="lblDisplayMask"
                                         Style="z-index: 101; position: absolute; left: 126px; top: 9px;"
                                        TabIndex="10" Width="500px">
                                    </px:PXTextEdit>
                                    <px:PXCheckBox ID="chkIsEncrypted" runat="server" DataField="IsEncrypted" Style="z-index: 102;
                                        position: absolute; left: 126px; top: 36px;" TabIndex="11" Text="Is Encripted">
                                    </px:PXCheckBox>
                                    <px:PXCheckBox ID="chkIsRequired" runat="server" DataField="IsRequired" Style="z-index: 103;
                                        position: absolute; left: 126px; top: 63px;" TabIndex="12" Text="IsRequired">
                                    </px:PXCheckBox>
                                    <px:PXCheckBox ID="chkIsIdentifier" runat="server" DataField="IsIdentifier" Style="z-index: 104;
                                        position: absolute; left: 126px; top: 90px;" TabIndex="13" Text="IsIdentifier">
                                    </px:PXCheckBox>
                                    <px:PXLabel ID="lblOrderIndex" runat="server" Style="z-index: 100; position: absolute;
                                        left: 9px; top: 9px;">Order Index :</px:PXLabel>
                                    <px:PXNumberEdit ID="edOrderIndex" runat="server" DataField="OrderIndex" LabelID="lblOrderIndex"
                                          Style="z-index: 101; position: absolute; left: 126px;
                                        top: 9px;" TabIndex="10" ValueType="Int16" Width="54px">
                                    </px:PXNumberEdit>
                                </RowTemplate>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True"/>
                        <ActionBar>
                            <Actions>
                                <NoteShow Enabled="False" />
                                <EditRecord Enabled="False" />
                                <FilterShow Enabled="False" />
                                <FilterSet Enabled="False" />
                                <Save Enabled="False" />
                            </Actions>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
		
            <px:PXTabItem Text="Vendor Payment Settings">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
                        Width="100%" ActionsPosition="Top" TabIndex="20"
                        SkinID="Details">
                        <Levels>
                            <px:PXGridLevel  DataMember="DetailsForVendor">
                                <Columns>
                                    <px:PXGridColumn AllowUpdate="False" AutoGenerateOption="NotSet" 
                                        DataField="DetailID" DataType="Int32" Label="ID" TextAlign="Right" Width="63px">
                                        <Header Text="ID">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Descr"  Width="140px">
                                        <Header Text="Description">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn AllowNull="False" DataField="IsRequired" DataType="Boolean" DefValueText="False"
                                        TextAlign="Center" Type="CheckBox">
                                        <Header Text="IsRequired">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn AllowNull="False" DataField="IsEncrypted" DataType="Boolean" DefValueText="False"
                                        TextAlign="Center" Type="CheckBox">
                                        <Header Text="Is Encripted">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn AllowNull="False" DataField="IsIdentifier" DataType="Boolean" DefValueText="False"
                                        TextAlign="Center" Type="CheckBox">
                                        <Header Text="IsIdentifier">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="OrderIndex" DataType="Int16" TextAlign="Right" Width="54px">
                                        <Header Text="Order Index">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EntryMask"  Width="180px">
                                        <Header Text="EntryMask">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ValidRegexp"  Width="250px">
                                        <Header Text="Validation Reg. Exp.">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="DisplayMask"  Width="180px">
                                        <Header Text="Display Mask">
                                        </Header>
                                    </px:PXGridColumn>                                                                       
                                </Columns>
                                <RowTemplate>
                                    <px:PXLabel ID="lblDetailID1" runat="server" Style="z-index: 100; left: 9px; position: absolute;
                                        top: 9px">ID :</px:PXLabel>
                                    <px:PXTextEdit ID="edDetailID1" runat="server" AllowNull="True" DataField="DetailID"
                                        LabelID="lblDetailID1"  Style="z-index: 101; left: 126px; position: absolute;
                                        top: 9px" TabIndex="10" Width="63px">
                                    </px:PXTextEdit>
                                    <px:PXLabel ID="lblEntryMask1" runat="server" Style="z-index: 102; left: 9px; position: absolute;
                                        top: 36px">EntryMask :</px:PXLabel>
                                    <px:PXTextEdit ID="edEntryMask1" runat="server" AllowNull="True" DataField="EntryMask"
                                        LabelID="lblEntryMask1"  Style="z-index: 103; left: 126px; position: absolute;
                                        top: 36px" TabIndex="30" Width="300px">
                                    </px:PXTextEdit>
                                    <px:PXLabel ID="lblValidRegexp1" runat="server" Style="z-index: 104; left: 9px; position: absolute;
                                        top: 63px">Validation Regexp :</px:PXLabel>
                                    <px:PXTextEdit ID="edValidRegexp1" runat="server" AllowNull="True" DataField="ValidRegexp"
                                        LabelID="lblValidRegexp1"  Style="z-index: 105; left: 126px; position: absolute;
                                        top: 63px" TabIndex="35" Width="500px">
                                    </px:PXTextEdit>
                                    <px:PXLabel ID="lblDisplayMask1" runat="server" Style="z-index: 100; position: absolute;
                                        left: 9px; top: 9px;">Display Mask :</px:PXLabel>
                                    <px:PXTextEdit ID="edDisplayMask1" runat="server" DataField="DisplayMask" LabelID="lblDisplayMask1"
                                         Style="z-index: 101; position: absolute; left: 126px; top: 9px;"
                                        TabIndex="10" Width="500px">
                                    </px:PXTextEdit>
                                    <px:PXCheckBox ID="chkIsEncrypted1" runat="server" DataField="IsEncrypted" Style="z-index: 102;
                                        position: absolute; left: 126px; top: 36px;" TabIndex="11" Text="Is Encripted">
                                    </px:PXCheckBox>
                                    <px:PXCheckBox ID="chkIsRequired1" runat="server" DataField="IsRequired" Style="z-index: 103;
                                        position: absolute; left: 126px; top: 63px;" TabIndex="12" Text="IsRequired">
                                    </px:PXCheckBox>
                                    <px:PXCheckBox ID="chkIsIdentifier1" runat="server" DataField="IsIdentifier" Style="z-index: 104;
                                        position: absolute; left: 126px; top: 90px;" TabIndex="13" Text="IsIdentifier">
                                    </px:PXCheckBox>
                                    <px:PXLabel ID="lblOrderIndex1" runat="server" Style="z-index: 100; position: absolute;
                                        left: 9px; top: 9px;">Order Index :</px:PXLabel>
                                    <px:PXNumberEdit ID="edOrderIndex1" runat="server" DataField="OrderIndex" LabelID="lblOrderIndex1"
                                          Style="z-index: 101; position: absolute; left: 126px;
                                        top: 9px;" TabIndex="10" ValueType="Int16" Width="54px">
                                    </px:PXNumberEdit>
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True"/>
                        <ActionBar>
                            <Actions>
                                <NoteShow Enabled="False" />
                                <EditRecord Enabled="False" />
                                <FilterShow Enabled="False" />
                                <FilterSet Enabled="False" />
                                <Save Enabled="False" />
                            </Actions>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
	    </Items>
	    <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXTab>
</asp:Content>
