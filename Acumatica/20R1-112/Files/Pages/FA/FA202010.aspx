<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA202010.aspx.cs" Inherits="Page_FA202010" Title="" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<%--Not used in 4.00. Not convertrd. --%>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.FA.DepreciationCalculation"
		PrimaryView="DepreciationMethod" BorderStyle="NotSet" Height="26px">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="CalculatePercents" CommitChanges="True" StartNewGroup = "true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Height="117px" 
		Style="z-index: 100" Width="100%"  
		DataMember="DepreciationMethod" Caption="Depreciation Method" TabIndex="1" 
		DefaultControlID="">
        
        <Template>
            <px:PXLabel ID="lblMethodCD" runat="server" Style="z-index: 100; left: 9px; position: absolute;
                top: 9px;" EnableClientScript="False">Method ID:</px:PXLabel>
            <px:PXSelector ID="edMethodCD" runat="server" DataField="MethodCD" 
				   
				 LabelID="lblMethodCD" 
				 style="z-index:101;position:absolute;left:146px; top:9px; width: 126px;" 
				TabIndex="10" AutoRefresh="True">
				<GridProperties>
					
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
				<AutoCallBack Enabled="True" Target="ds" Command="Cancel"></AutoCallBack>
			</px:PXSelector>
            <px:PXLabel ID="lblDescription" runat="server" Style="z-index: 102; left: 9px; position: absolute;
                top: 90px; " EnableClientScript="False">Description:</px:PXLabel>
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description"
                LabelID="lblDescription"  Style="z-index: 103; left: 146px; position: absolute;
                top: 90px; width: 381px;" TabIndex="11">
            </px:PXTextEdit>
			<px:PXLabel ID="lblAveragingConvention" runat="server" 
				EnableClientScript="False" 
				style="z-index:102; position:absolute;left:9px; top:63px;">Avereging Convention:</px:PXLabel>
			<px:PXDropDown ID="edAveragingConvention" runat="server" 
				DataField="AveragingConvention" LabelID="lblAveragingConvention" 
				style="z-index:103;position:absolute;left:146px; top:63px;" TabIndex="11" 
				Width="126px">
				
				<AutoCallBack Enabled="True" Target="form" Command="Save"></AutoCallBack>
			</px:PXDropDown>
			<px:PXLabel ID="lblDepreciationPeriod" runat="server" EnableClientScript="False" 
				style="z-index:104;position:absolute;left:301px; top:9px; height: 13px; width: 131px;">Recovery Period, months:</px:PXLabel>
        	<px:PXNumberEdit ID="edRecoveryPeriod" runat="server" 
				DataField="RecoveryPeriod" LabelID="lblRecoveryPeriod"  
				 
				style="z-index:105; position:absolute;left:448px; top:9px;" TabIndex="12" Width="49px" 
				AllowNull="False">
				<AutoCallBack Enabled="True" Target="form" Command="Save"></AutoCallBack>
			</px:PXNumberEdit>
        	<px:PXLabel ID="lblDepreciationMethod" runat="server" EnableClientScript="False" 
				style="z-index:100;position:absolute;left:9px; top:36px;">Depreciation Method:</px:PXLabel>
			<px:PXDropDown ID="edDepreciationMethod" runat="server" 
				DataField="DepreciationMethod" LabelID="lblDepreciationMethod" 
				style="z-index:101;position:absolute;left:146px; top:36px; width: 126px;" 
				TabIndex="10">
				
			</px:PXDropDown>
        	<px:PXLabel ID="lblTotalPercents" runat="server" EnableClientScript="False" 
				style="z-index:100;position:absolute;left:769px; top:9px; width: 74px;">Total Percent:</px:PXLabel>
			<px:PXNumberEdit ID="edTotalPercents" runat="server" DataField="TotalPercents" 
				Decimals="4" Enabled="False" LabelID="lblTotalPercents" 
				  MaxLength="8" DisplayFormat="##0.0000%" 
				style="z-index:101;position:absolute;left:851px; top:9px; width: 98px;" TabIndex="10" 
				ValueType="Decimal">
			</px:PXNumberEdit>
			<px:PXDropDown ID="edDepreciationPeriodsInYear" runat="server" DataField="DepreciationPeriodsInYear" 
				LabelID="lblDepreciationPeriodsInYear" 
				style="z-index:101;position:absolute;left:448px; top:36px; width: 78px;" 
				TabIndex="10" ValueType="Int16">
				
				<AutoCallBack Enabled="True" Target="form" Command="Save"></AutoCallBack>
			</px:PXDropDown>
			<px:PXLabel ID="lblDepreciationPeriodsInYear" runat="server" 
				EnableClientScript="False" 
				style="z-index:100;position:absolute;left:301px; top:36px;" height="13px">Depreciation periods in Year:</px:PXLabel>
        	<px:PXCheckBox ID="chkIsTableMethod" runat="server" Checked="True" 
				DataField="IsTableMethod" Enabled="False" 
				style="z-index:100;position:absolute;left:851px; top:63px;" TabIndex="10" 
				Text="Is Table Method">
				<AutoCallBack Target="form" Command="Save" Enabled="True"></AutoCallBack>
			</px:PXCheckBox>
			<px:PXCheckBox ID="chkYearlyAccountancy" runat="server" 
				DataField="YearlyAccountancy" 
				style="z-index:101;position:absolute;left:851px; top:90px;" TabIndex="11" 
				Text="Yearly Accountancy">
				<AutoCallBack Target="form" Command="Save" Enabled="True"></AutoCallBack>
			</px:PXCheckBox>
        	<px:PXLabel ID="lblDBMultiPlier" runat="server" EnableClientScript="False" 
				style="z-index:100;position:absolute;left:546px; top:63px;">DB Multiplier:</px:PXLabel>
			<px:PXNumberEdit ID="edDBMultiPlier" runat="server" DataField="DBMultiPlier" 
				Decimals="4" LabelID="lblDBMultiPlier"   
				style="z-index:101;position:absolute;left:669px; top:63px; width: 59px;" 
				TabIndex="10" ValueType="Decimal">
			</px:PXNumberEdit>
			<px:PXCheckBox ID="chkSwitchToSL" runat="server" DataField="SwitchToSL" 
				style="z-index:102;position:absolute;left:851px; top:36px;" TabIndex="11" 
				Text="Switch to SL">
			</px:PXCheckBox>
        	<px:PXLabel ID="lblDepreciationStartDate" runat="server" EnableClientScript="False" 
				style="z-index:100;position:absolute;left:546px; top:9px;">Depreciation Start Date:</px:PXLabel>
			<px:PXDateTimeEdit ID="edDepreciationStartDate" runat="server" 
				DataField="DepreciationStartDate" LabelID="lblDepreciationStartDate" MaxValue="9999-06-06" 
				style="z-index:101;position:absolute;left:669px; top:9px;" TabIndex="10" 
				Width="90px">
				<AutoCallBack Enabled="True" Target="form" Command="Save"></AutoCallBack>
			</px:PXDateTimeEdit>
        	<px:PXLabel ID="lblDepreciationStopDate" runat="server" 
				EnableClientScript="False" 
				style="z-index:100;position:absolute;left:546px; top:36px;">Depreciation Stop Date:</px:PXLabel>
			<px:PXDateTimeEdit ID="edDepreciationStopDate" runat="server" 
				DataField="DepreciationStopDate" LabelID="lblDepreciationStopDate" 
				MaxValue="9999-06-06" 
				style="z-index:101;position:absolute;left:669px; top:36px;" TabIndex="10" 
				Width="90px">
				<AutoCallBack Enabled="True" Target="form" Command="Save"></AutoCallBack>
			</px:PXDateTimeEdit>
			<px:PXLabel ID="lblBookID" runat="server" EnableClientScript="False" 
				style="z-index:102;position:absolute;left:301px; top:63px;">Book:</px:PXLabel>
			<px:PXSelector ID="edBookID" runat="server" DataField="BookID" 
				   
				HintField="description" HintLabelID="lblBookIDH" LabelID="lblBookID" 
				style="z-index:103;position:absolute;left:448px; top:63px;" TabIndex="11" 
				Width="78px">
				<GridProperties>
					<Columns>
						<px:PXGridColumn AutoGenerateOption="NotSet" DataField="BookCode" 
							Label="Book Code" >
							<Header Text="Book Code">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AutoGenerateOption="NotSet" DataField="Description" 
							Label="Description"  Width="200px">
							<Header Text="Description">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AutoGenerateOption="NotSet" DataField="LedgerID" 
							Label="Ledger">
							<Header Text="Ledger">
							</Header>
						</px:PXGridColumn>
					</Columns>
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
				<AutoCallBack Enabled="True" Target="form" Command="Save"></AutoCallBack>
			</px:PXSelector>
        	<px:PXLabel ID="lblRecordType" runat="server" EnableClientScript="False" 
				style="z-index:100;position:absolute;left:546px; top:90px;">Record Type:</px:PXLabel>
			<px:PXDropDown ID="edRecordType" runat="server" AllowNull="False" 
				DataField="RecordType" LabelID="lblRecordType" SelectedIndex="1" 
				style="z-index:101;position:absolute;left:669px; top:90px;" TabIndex="10" 
				Width="54px">
				
			</px:PXDropDown>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid SkinID="Details" ID="grid" BorderWidth="0px" 
            runat="server" DataSourceID="ds" 
            Style="z-index: 100; height: 518px;" Width="100%" AllowPaging="True"
            AdjustPageSize="Auto" ActionsPosition="Top" TabIndex="7" Caption="Depreciation Percents per Year"
			AllowSearch="True"  NoteField="NoteText" >
			<Levels>
				<px:PXGridLevel DataMember="DepreciationMethodLines" 
					>
					<RowTemplate>
						<px:PXLabel ID="lblYear" runat="server" EnableClientScript="False" 
							style="z-index:100; position:absolute;left:9px;top:9px;">Depreciation Year:</px:PXLabel>
						<px:PXNumberEdit ID="edYear" runat="server" 
							DataField="Year" LabelID="lblYear" 
							  
							style="z-index:101; position:absolute;left:126px;top:9px;" TabIndex="10" Width="54px" 
							Enabled="False">
						</px:PXNumberEdit>
						<px:PXLabel ID="lblRatioPerYear" runat="server" EnableClientScript="False" 
							style="z-index:102; position:absolute;left:9px;top:36px;">Percent per Year:</px:PXLabel>
						<px:PXNumberEdit ID="edRatioPerYear" runat="server" 
							DataField="RatioPerYear" Decimals="4" LabelID="lblRatioPerYear" 
							   MaxLength="8" DisplayFormat="##0.0000%"
							style="z-index:103; position:absolute;left:126px;top:36px;" TabIndex="11" 
							ValueType="Decimal" Width="81px" AllowNull="False">
							<AutoCallBack Enabled="True" Target="grid" Command="Save" />
						</px:PXNumberEdit>
						<px:PXLabel ID="lblRatioPerPeriod1" runat="server" EnableClientScript="False" 
							style="z-index:104;position:absolute;left:9px;top:63px;">Percent per Januar:</px:PXLabel>
						<px:PXNumberEdit ID="edRatioPerPeriod1" runat="server" 
							DataField="RatioPerPeriod1" Decimals="4" LabelID="lblRatioPerPeriod1" 
							   MaxLength="8" DisplayFormat="##0.0000%"
							style="z-index:105;position:absolute;left:126px;top:63px;" TabIndex="12" 
							ValueType="Decimal" Width="81px">
						</px:PXNumberEdit>
						<px:PXLabel ID="lblRatioPerPeriod2" runat="server" EnableClientScript="False" 
							style="z-index:106;position:absolute;left:9px;top:90px;">Percent per Februar:</px:PXLabel>
						<px:PXNumberEdit ID="edRatioPerPeriod2" runat="server" 
							DataField="RatioPerPeriod2" Decimals="4" LabelID="lblRatioPerPeriod2" 
							   MaxLength="8" DisplayFormat="##0.0000%"
							style="z-index:107;position:absolute;left:126px;top:90px;" TabIndex="13" 
							ValueType="Decimal" Width="81px">
						</px:PXNumberEdit>
						<px:PXLabel ID="lblRatioPerPeriod3" runat="server" EnableClientScript="False" 
							style="z-index:108;position:absolute;left:9px;top:117px;">Percent per March:</px:PXLabel>
						<px:PXNumberEdit ID="edRatioPerPeriod3" runat="server" 
							DataField="RatioPerPeriod3" Decimals="4" LabelID="lblRatioPerPeriod3" 
							   MaxLength="8" DisplayFormat="##0.0000%"
							style="z-index:109;position:absolute;left:126px;top:117px;" TabIndex="14" 
							ValueType="Decimal" Width="81px">
						</px:PXNumberEdit>
						<px:PXLabel ID="lblRatioPerPeriod4" runat="server" EnableClientScript="False" 
							style="z-index:110;position:absolute;left:9px;top:144px;">Percent per April:</px:PXLabel>
						<px:PXNumberEdit ID="edRatioPerPeriod4" runat="server" 
							DataField="RatioPerPeriod4" Decimals="4" LabelID="lblRatioPerPeriod4" 
							   MaxLength="8" DisplayFormat="##0.0000%"
							style="z-index:111;position:absolute;left:126px;top:144px;" TabIndex="15" 
							ValueType="Decimal" Width="81px">
						</px:PXNumberEdit>
						<px:PXLabel ID="lblRatioPerPeriod5" runat="server" EnableClientScript="False" 
							style="z-index:112;position:absolute;left:9px;top:171px;">Percent per May:</px:PXLabel>
						<px:PXNumberEdit ID="edRatioPerPeriod5" runat="server" 
							DataField="RatioPerPeriod5" Decimals="4" LabelID="lblRatioPerPeriod5" 
							   MaxLength="8" DisplayFormat="##0.0000%"
							style="z-index:113;position:absolute;left:126px;top:171px;" TabIndex="16" 
							ValueType="Decimal" Width="81px">
						</px:PXNumberEdit>
						<px:PXLabel ID="lblRatioPerPeriod6" runat="server" EnableClientScript="False" 
							style="z-index:114;position:absolute;left:9px;top:198px;">Percent per June:</px:PXLabel>
						<px:PXNumberEdit ID="edRatioPerPeriod6" runat="server" 
							DataField="RatioPerPeriod6" Decimals="4" LabelID="lblRatioPerPeriod6" 
							   MaxLength="8" DisplayFormat="##0.0000%"
							style="z-index:115;position:absolute;left:126px;top:198px;" TabIndex="17" 
							ValueType="Decimal" Width="81px">
						</px:PXNumberEdit>
						<px:PXLabel ID="lblRatioPerPeriod7" runat="server" EnableClientScript="False" 
							style="z-index:116;position:absolute;left:9px;top:225px;">Percent per July:</px:PXLabel>
						<px:PXNumberEdit ID="edRatioPerPeriod7" runat="server" 
							DataField="RatioPerPeriod7" Decimals="4" LabelID="lblRatioPerPeriod7" 
							   MaxLength="8" DisplayFormat="##0.0000%"
							style="z-index:117;position:absolute;left:126px;top:225px;" TabIndex="18" 
							ValueType="Decimal" Width="81px">
						</px:PXNumberEdit>
						<px:PXLabel ID="lblRatioPerPeriod8" runat="server" EnableClientScript="False" 
							style="z-index:118;position:absolute;left:9px;top:252px;">Percent per August:</px:PXLabel>
						<px:PXNumberEdit ID="edRatioPerPeriod8" runat="server" 
							DataField="RatioPerPeriod8" Decimals="4" LabelID="lblRatioPerPeriod8" 
							   MaxLength="8" DisplayFormat="##0.0000%"
							style="z-index:119;position:absolute;left:126px;top:252px;" TabIndex="19" 
							ValueType="Decimal" Width="81px">
						</px:PXNumberEdit>
						<px:PXLabel ID="lblRatioPerPeriod9" runat="server" EnableClientScript="False" 
							style="z-index:120;position:absolute;left:9px; top:279px;">Percent per September:</px:PXLabel>
						<px:PXNumberEdit ID="edRatioPerPeriod9" runat="server" 
							DataField="RatioPerPeriod9" Decimals="4" LabelID="lblRatioPerPeriod9" 
							   MaxLength="8" DisplayFormat="##0.0000%"
							style="z-index:121;position:absolute;left:126px; top:279px;" TabIndex="20" 
							ValueType="Decimal" Width="81px">
						</px:PXNumberEdit>
						<px:PXLabel ID="lblRatioPerPeriod10" runat="server" EnableClientScript="False" 
							style="z-index:122;position:absolute;left:9px; top:306px;">Percent per October:</px:PXLabel>
						<px:PXNumberEdit ID="edRatioPerPeriod10" runat="server" 
							DataField="RatioPerPeriod10" Decimals="4" LabelID="lblRatioPerPeriod10" 
							   MaxLength="8" DisplayFormat="##0.0000%"
							style="z-index:123;position:absolute;left:126px; top:306px;" TabIndex="21" 
							ValueType="Decimal" Width="81px">
						</px:PXNumberEdit>
						<px:PXLabel ID="lblRatioPerPeriod11" runat="server" EnableClientScript="False" 
							style="z-index:124;position:absolute;left:9px; top:333px; height: 17px;">Percent per November:</px:PXLabel>
						<px:PXNumberEdit ID="edRatioPerPeriod11" runat="server" 
							DataField="RatioPerPeriod11" Decimals="4" LabelID="lblRatioPerPeriod11" 
							   MaxLength="8" DisplayFormat="##0.0000%"
							style="z-index:125;position:absolute;left:126px; top:333px;" TabIndex="22" 
							ValueType="Decimal" Width="81px">
						</px:PXNumberEdit>
						<px:PXLabel ID="lblRatioPerPeriod12" runat="server" EnableClientScript="False" 
							style="z-index:126;position:absolute;left:9px; top:360px;">Percent per December:</px:PXLabel>
						<px:PXNumberEdit ID="edRatioPerPeriod12" runat="server" 
							DataField="RatioPerPeriod12" Decimals="4" LabelID="lblRatioPerPeriod12" 
							   MaxLength="8" DisplayFormat="##0.0000%"
							style="z-index:127;position:absolute;left:126px; top:360px;" TabIndex="23" 
							ValueType="Decimal" Width="81px">
						</px:PXNumberEdit>
					</RowTemplate>
					<Columns>
                        <px:PXGridColumn DataField="Year" DataType="Int32" 
							TextAlign="Right" Width="54px" AutoGenerateOption="NotSet" AllowUpdate="False" 
							Label="Recovery Year">
                            <Header Text="Recovery Year">
                            </Header>    
                        </px:PXGridColumn>
						<px:PXGridColumn DataField="RatioPerYear" DataType="Decimal" Decimals="4" 
							TextAlign="Right" Width="81px" 
							AutoGenerateOption="NotSet" AllowNull="False" DefValueText="0" AllowUpdate="False">
                            <Header Text="Percent per Year">
                            </Header>    
                        </px:PXGridColumn>
						<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" 
							DataField="RatioPerPeriod1" DataType="Decimal" Decimals="4" DefValueText="0" 
							TextAlign="Right" Width="81px" AutoCallBack="True">
							<Header Text="Percent per Period 1">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" 
							DataField="RatioPerPeriod2" DataType="Decimal" Decimals="4" DefValueText="0" 
							TextAlign="Right" Width="81px" AutoCallBack="True">
							<Header Text="Percent per Period 2">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" 
							DataField="RatioPerPeriod3" DataType="Decimal" Decimals="4" DefValueText="0" 
							TextAlign="Right" Width="81px" AutoCallBack="True">
							<Header Text="Percent per Period 3">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" 
							DataField="RatioPerPeriod4" DataType="Decimal" Decimals="4" DefValueText="0" 
							TextAlign="Right" Width="81px" AutoCallBack="True">
							<Header Text="Percent per Period 4">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" 
							DataField="RatioPerPeriod5" DataType="Decimal" Decimals="4" DefValueText="0" 
							TextAlign="Right" Width="81px" AutoCallBack="True">
							<Header Text="Percent per Period 5">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" 
							DataField="RatioPerPeriod6" DataType="Decimal" Decimals="4" DefValueText="0" 
							TextAlign="Right" Width="81px" AutoCallBack="True">
							<Header Text="Percent per Period 6">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" 
							DataField="RatioPerPeriod7" DataType="Decimal" Decimals="4" DefValueText="0" 
							TextAlign="Right" Width="81px" AutoCallBack="True">
							<Header Text="Percent per Period 7">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" 
							DataField="RatioPerPeriod8" DataType="Decimal" Decimals="4" DefValueText="0" 
							TextAlign="Right" Width="81px" AutoCallBack="True">
							<Header Text="Percent per Period 8">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" 
							DataField="RatioPerPeriod9" DataType="Decimal" Decimals="4" DefValueText="0" 
							TextAlign="Right" Width="81px" AutoCallBack="True">
							<Header Text="Percent per Period 9">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" 
							DataField="RatioPerPeriod10" DataType="Decimal" Decimals="4" DefValueText="0" 
							TextAlign="Right" Width="81px" AutoCallBack="True">
							<Header Text="Percent per Period 10">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" 
							DataField="RatioPerPeriod11" DataType="Decimal" Decimals="4" DefValueText="0" 
							TextAlign="Right" Width="81px" AutoCallBack="True">
							<Header Text="Percent per Period 11">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" 
							DataField="RatioPerPeriod12" DataType="Decimal" Decimals="4" DefValueText="0" 
							TextAlign="Right" Width="81px" AutoCallBack="True">
							<Header Text="Percent per Period 12">
							</Header>
						</px:PXGridColumn> 
					</Columns>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
			<ActionBar >
				<Actions>
					<AddNew Enabled = "false" />
					<Delete Enabled = "false" />
				</Actions>
			</ActionBar>
			<AutoSize Enabled="True" Container="Window" MinHeight="200" />
		</px:PXGrid>                    
</asp:Content>
