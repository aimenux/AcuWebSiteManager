<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR209900.aspx.cs"
    Inherits="Page_PR209900" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.PR.PRCertifiedProjectMaint" PrimaryView="CertifiedProject" Visible="True" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="CertifiedProject" Width="100%" FastFilterFields="Description">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edContractID" runat="server" DataField="ProjectID" CommitChanges="True" Width="350" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="CurrentProject.Status" Enabled="False" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="CurrentProject.Description" Width="350" Enabled="False" />
            
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="0px" ControlSize="S" />
            <px:PXCheckBox ID="edFileEmptyReport" runat="server" DataField="CurrentProject.FileEmptyCertifiedReport" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds">
        <Items>
            <px:PXTabItem Text="Earning Rates">
                <Template>
                    <px:PXGrid ID="PXGrid1" runat="server" SkinID="DetailsInTab" SyncPosition="True" DataSourceID="ds" KeepPosition="True" Width="100%" Height="200px">
                        <Levels>
                            <px:PXGridLevel DataMember="EarningRates">
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edRate" runat="server" DataField="Rate" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="InventoryID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="TaskID" />
                                    <px:PXGridColumn DataField="Rate" TextAlign="Right" />
                                    <px:PXGridColumn DataField="EffectiveDate" Width="150px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Container="Parent" Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Deductions and Benefits package">
                <Template>
                    <px:PXGrid runat="server" ID="PXGrid2" SkinID="DetailsInTab" SyncPosition="True" DataSourceID="ds" KeepPosition="True" Width="100%" Height="200px">
                        <Levels>
                            <px:PXGridLevel DataMember="DeductionsAndBenefitsPackage">
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edDeductionAmount" runat="server" DataField="DeductionAmount" />
                                    <px:PXNumberEdit ID="edDeductionRate" runat="server" DataField="DeductionRate" />
                                    <px:PXNumberEdit ID="edBenefitAmount" runat="server" DataField="BenefitAmount" />
                                    <px:PXNumberEdit ID="edBenefitRate" runat="server" DataField="BenefitRate" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="DeductionAndBenefitCodeID" CommitChanges="True" Width="195" />
                                    <px:PXGridColumn DataField="LaborItemID" CommitChanges="true" />
                                    <px:PXGridColumn DataField="PRDeductCode__Description" />
                                    <px:PXGridColumn DataField="PRDeductCode__ContribType" Width="300px" />
                                    <px:PXGridColumn DataField="PRDeductCode__DedCalcType" />
                                    <px:PXGridColumn DataField="DeductionAmount" />
                                    <px:PXGridColumn DataField="DeductionRate" />
                                    <px:PXGridColumn DataField="PRDeductCode__CntCalcType" />
                                    <px:PXGridColumn DataField="BenefitAmount" />
                                    <px:PXGridColumn DataField="BenefitRate" />
                                    <px:PXGridColumn DataField="EffectiveDate" Width="150px" CommitChanges="true" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Container="Parent" Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Fringe Benefits">
				<Template>
                    <px:PXFormView ID="formFringeBenefits" runat="server" DataMember="CurrentProject">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XL" LabelsWidth="1px" />
                            <px:PXCheckBox ID="edWageAbovePrevailingAnnualizationException" runat="server" DataField="WageAbovePrevailingAnnualizationException" />
                            <px:PXLayoutRule StartRow="true" runat="server" ControlSize="M" LabelsWidth="XL" />
                            <px:PXSelector ID="edBenefitCodeReceivingFringeRate" runat="server" DataField="BenefitCodeReceivingFringeRate" />
                        </Template>
                    </px:PXFormView>
					<px:PXSplitContainer ID="splitContainerFringeBenefits" runat="server" PositionInPercent="true" SplitterPosition="50" Orientation="Vertical">
						<Template1>
							<px:PXGrid runat="server" ID="PXGrid3" SkinID="DetailsInTab" SyncPosition="True" DataSourceID="ds" KeepPosition="True"
								Caption="Rates" CaptionVisible="true" AllowPaging="True" Width="100%">
								<Levels>
									<px:PXGridLevel DataMember="FringeBenefitRates">
										<RowTemplate>
											<px:PXNumberEdit ID="edFringeRate" runat="server" DataField="Rate" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="LaborItemID" Width="150px" CommitChanges="true" />
											<px:PXGridColumn DataField="InventoryItem__Descr" Width="150px" />
											<px:PXGridColumn DataField="ProjectTaskID" Width="150px" CommitChanges="True" />
											<px:PXGridColumn DataField="Rate" TextAlign="Right" />
											<px:PXGridColumn DataField="EffectiveDate" Width="120px" CommitChanges="True" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="true" Container="Parent" MinHeight="150" />
							</px:PXGrid>
						</Template1>
						<Template2>
                            <px:PXGrid runat="server" ID="grdBenefitsReducingRate" SkinID="DetailsInTab" SyncPosition="True" DataSourceID="ds" KeepPosition="True"
							    Caption="Benefits Reducing the Rate" CaptionVisible="true" AllowPaging="True" Width="100%">
							    <Levels>
								    <px:PXGridLevel DataMember="FringeBenefitRateReducingDeductions">
									    <RowTemplate>
										    <px:PXSelector ID="edDeductCodeID" runat="server" DataField="DeductCodeID" />
									    </RowTemplate>
									    <Columns>
										    <px:PXGridColumn DataField="IsActive" Width="60px" Type="CheckBox" TextAlign="Center" />
										    <px:PXGridColumn DataField="DeductCodeID" Width="100px" CommitChanges="true" />
										    <px:PXGridColumn DataField="DeductCodeID_Description" Width="200px" />
										    <px:PXGridColumn DataField="PRDeductCode__CertifiedReportType" Width="150px" />
										    <px:PXGridColumn DataField="AnnualizationException" Width="120px" Type="CheckBox" TextAlign="Center" />
									    </Columns>
								    </px:PXGridLevel>
							    </Levels>
							    <AutoSize Enabled="true" Container="Parent" MinHeight="150" />
						    </px:PXGrid>
						</Template2>
						<AutoSize Enabled="true" Container="Parent" MinHeight="150" />
					</px:PXSplitContainer>
				</Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
