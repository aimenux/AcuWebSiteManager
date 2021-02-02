using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.SQLTree;
using PX.Objects.Common;
using PX.Objects.Common.Abstractions.Periods;

namespace PX.Objects.GL.Descriptor
{
    public abstract class PeriodKeyProviderBase
    {
        #region Types

        public class SourcesSpecificationCollection : SourcesSpecificationCollection<SourceSpecificationItem>
        {

        }

        public class SourceSpecificationItem
        {
            public virtual Type BranchSourceType { get; set; }

            public virtual Type BranchSourceFormulaType { get; set; }

            public virtual IBqlCreator BranchSourceFormula { get; protected set; }

            public virtual Type OrganizationSourceType { get; set; }

            public virtual bool IsMain { get; set; }

            public virtual bool IsAnySourceSpecified => BranchSourceType != null
                                                        || BranchSourceFormula != null
                                                        || OrganizationSourceType != null;

            protected List<Type> SourceFields { get; set; }

            public virtual SourceSpecificationItem Initialize()
            {
                if (BranchSourceFormulaType != null && BranchSourceFormula == null)
                {
                    BranchSourceFormula = PXFormulaAttribute.InitFormula(BranchSourceFormulaType);
                }

                return this;
            }

            public virtual List<Type> GetSourceFields(PXCache cache)
            {
                if (SourceFields == null)
                {
                    SourceFields = BuildSourceFields(cache);
                }

                return SourceFields;
            }

            protected virtual List<Type> BuildSourceFields(PXCache cache)
            {
                var exp = SQLExpression.None();
                var fields = new List<Type>();

                if (BranchSourceFormula != null)
                {
                    BranchSourceFormula.AppendExpression(ref exp, cache.Graph,
                        new BqlCommandInfo(false) { Fields = fields, BuildExpression = false },
                        new BqlCommand.Selection());
                }

                if (BranchSourceType != null)
                {
                    fields.Add(BranchSourceType);
                }

                if (OrganizationSourceType != null)
                {
                    fields.Add(OrganizationSourceType);
                }

                return fields;
            }
        }

        #region Generic Types

        public class KeyWithSourceValues<TSourceSpecificationItem, TKey>
            where TSourceSpecificationItem : SourceSpecificationItem
            where TKey : OrganizationDependedPeriodKey, new()
        {
            public TSourceSpecificationItem SpecificationItem { get; set; }

            public virtual List<int?> KeyOrganizationIDs { get; set; }

            public TKey Key { get; set; }

            public virtual List<int?> SourceOrganizationIDs { get; set; }

            public virtual List<int?> SourceBranchIDs { get; set; }

            public KeyWithSourceValues()
            {
                Key = new TKey();
                KeyOrganizationIDs = new List<int?>();
            }

            public virtual bool SourcesEqual(object otherObject)
            {
                var otherKeyWithSourceValues = (KeyWithSourceValues<TSourceSpecificationItem, TKey>)otherObject;

                return SourceBranchIDs.OrderBy(v => v).SequenceEqual(otherKeyWithSourceValues.SourceBranchIDs.OrderBy(v => v))
                       && SourceOrganizationIDs.OrderBy(v => v).SequenceEqual(otherKeyWithSourceValues.SourceOrganizationIDs.OrderBy(v => v));
            }
        }

        public class KeyWithSourceValuesCollection<TKeyWithSourceValues, TSourceSpecificationItem, TKey>
            where TKeyWithSourceValues : KeyWithSourceValues<TSourceSpecificationItem, TKey>
            where TSourceSpecificationItem : SourceSpecificationItem
            where TKey : OrganizationDependedPeriodKey, new()
        {
            public List<TKeyWithSourceValues> Items { get; set; }

            public TKeyWithSourceValues MainItem => Items.SingleOrDefault(item => item.SpecificationItem.IsMain);

            public List<int?> ConsolidatedOrganizationIDs { get; set; }

            public TKey ConsolidatedKey { get; set; }

            public KeyWithSourceValuesCollection()
            {
                Items = new List<TKeyWithSourceValues>();
            }
        }

        public class SourcesSpecificationCollectionBase
        {
            public virtual IEnumerable<SourceSpecificationItem> SpecificationItemBases { get; }

            public List<Type> DependsOnFields { get; set; }

            protected List<Type> SourceFields { get; set; }

            public virtual List<Type> GetSourceFields(PXCache cache)
            {
                if (SourceFields == null)
                {
                    SourceFields = BuildSourceFields(cache);
                }

                return SourceFields;
            }

            protected virtual List<Type> BuildSourceFields(PXCache cache)
            {
                List<Type> fields = SpecificationItemBases.SelectMany(item => item.GetSourceFields(cache)).ToList();

                fields.AddRange(DependsOnFields);

                return fields;
            }
        }

        public class SourcesSpecificationCollection<TSourceSpecificationItem> : SourcesSpecificationCollectionBase
            where TSourceSpecificationItem : SourceSpecificationItem
        {
            public override IEnumerable<SourceSpecificationItem> SpecificationItemBases => SpecificationItems;

            public List<TSourceSpecificationItem> SpecificationItems { get; set; }

            public TSourceSpecificationItem MainSpecificationItem { get; set; }

            public SourcesSpecificationCollection()
            {
                SpecificationItems = new List<TSourceSpecificationItem>();
                DependsOnFields = new List<Type>();
            }
        }

        #endregion

        #region SourceSpecification

        public class SourceSpecification<TBranchSource, TIsMain> :
            SourceSpecification<TBranchSource, BqlNone, BqlHelper.fieldStub, TIsMain>
            where TBranchSource : IBqlField
            where TIsMain : BoolConstant<TIsMain>, new()
        {
        }

        public class SourceSpecification<TBranchSource, TBranchFormula, TIsMain> :
            SourceSpecification<TBranchSource, TBranchFormula, BqlHelper.fieldStub, TIsMain>
            where TBranchSource : IBqlField
            where TBranchFormula : IBqlCreator
            where TIsMain : BoolConstant<TIsMain>, new()
		{
        }

        public class SourceSpecification<TBranchSource, TBranchFormula, TOrganizationSource, TIsMain> : SourceSpecificationItem
            where TBranchSource : IBqlField
            where TBranchFormula : IBqlCreator
            where TOrganizationSource : IBqlField
            where TIsMain : BoolConstant<TIsMain>, new()
		{
            public override Type BranchSourceType => BqlHelper.GetTypeNotStub<TBranchSource>();

            public override Type BranchSourceFormulaType => BqlHelper.GetTypeNotStub<TBranchFormula>();

            public override Type OrganizationSourceType => BqlHelper.GetTypeNotStub<TOrganizationSource>();

            public override SourceSpecificationItem Initialize()
            {
                base.Initialize();
				IsMain = new TIsMain().Value;
				return this;
            }
        }

        #endregion

        #endregion
    }
}
