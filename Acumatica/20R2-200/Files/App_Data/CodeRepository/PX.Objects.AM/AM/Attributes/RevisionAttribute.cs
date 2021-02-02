using System;
using System.Collections;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Revision PX Selector Attribute
    /// </summary>

    public class Rev
    {
        public class IDAttribute : PXAggregateAttribute, IPXFieldDefaultingSubscriber
        {
            private PXDefaultAttribute _DefaultAttribute;

            public IDAttribute(Type defaultType, Type keyField, Type revisionField, params Type[] fieldList)
            {
                _Attributes.Add(new Selector(keyField, revisionField, fieldList));
                _DefaultAttribute = new PXDefaultAttribute(defaultType);
            }

            public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
            {
                _DefaultAttribute.FieldDefaulting(sender, e);
                e.Cancel = false;
            }

            #region "Sub-Attributes"

            public class Selector : PXCustomSelectorAttribute
            {
                private Type _KeyField;
                private Type _RevisionField;

                public Selector(Type keyField, Type revisionField, params Type[] fieldList) : base(revisionField, fieldList)
                {
                    _KeyField = keyField;
                    _RevisionField = revisionField;
                    ValidateValue = false;
                }
                public IEnumerable GetRecords()
                {
                    var cache = _Graph.Caches[_BqlTable];
                    var keyFieldValue = cache.GetValue(cache.Current, _KeyField.Name);
                    var select = BqlCommand.Compose(
                                    typeof(Select<,,>),
                                        _BqlTable,
                                    typeof(Where<,>),
                                        _KeyField,
                                    typeof(Equal<>),
                                        typeof(Required<>),
                                            _KeyField,
                                    typeof(OrderBy<>),
                                        typeof(Desc<>),
                                            _RevisionField
                                    );

                    var cmd = BqlCommand.CreateInstance(select);
                    PXView view = new PXView(_Graph, true, cmd);

                    foreach (var ret in view.SelectMulti(keyFieldValue))
                    {
                        yield return ret;
                    }
                }
            }

            #endregion
        }

        public class KeyAttribute : PXAggregateAttribute
        {
            public KeyAttribute(Type numberingSeq, Type keyField, Type revisionField, params Type[] fieldList)
            {
                _Attributes.Add(new Selector(keyField, revisionField, fieldList));
                _Attributes.Add(new Numbering(numberingSeq, typeof(AccessInfo.businessDate)));
            }

            public void InsertRevision()
            {
                GetAttribute<Numbering>().SetAutoNumbering(false);
            }

            public string GetNewSymbol()
            {
                return GetAttribute<Numbering>().GetNewSymbol();
            }

            public bool IsAutoNumber()
            {
                return GetAttribute<Numbering>().IsAutoNumber();
            }

            public static void InsertRevision<TField>(PXCache cache)
                where TField : IBqlField
            {
                foreach (var keyAttribute in cache.GetAttributes<TField>())
                {
                    if (keyAttribute is KeyAttribute)
                    {
                        var autoNumberAttribute = (KeyAttribute)keyAttribute;
                        autoNumberAttribute.InsertRevision();
                    }
                }
            }

            public static string GetNewSymbol<TField>(PXCache cache)
                where TField : IBqlField
            {
                foreach (var keyAttribute in cache.GetAttributes<TField>())
                {
                    if (keyAttribute is KeyAttribute)
                    {
                        var autoNumberAttribute = (KeyAttribute)keyAttribute;
                        return autoNumberAttribute.GetNewSymbol();
                    }
                }
                return string.Empty;
            }

            public static bool GetIsAutoNumber<TField>(PXCache cache)
                where TField : IBqlField
            {
                foreach (var keyAttribute in cache.GetAttributes<TField>())
                {
                    if (keyAttribute is KeyAttribute)
                    {
                        var autoNumberAttribute = (KeyAttribute)keyAttribute;
                        return autoNumberAttribute.IsAutoNumber();
                    }
                }
                return false;
            }

            #region "Key Attributes"

            private class Selector : PXCustomSelectorAttribute
            {
                private Type _RevisionField;
                public Selector(Type keyField, Type revisionField, params Type[] fieldList) : base(keyField, fieldList)
                {
                    _RevisionField = revisionField;
                    ValidateValue = false;
                }
                public IEnumerable GetRecords()
                {
                    var cache = _Graph.Caches[_BqlTable];

                    var select = BqlCommand.Compose(
                                    typeof(Select<,,>),
                                        _BqlTable,
                                    typeof(Where<boolTrue, Equal<boolTrue>>),
                                    typeof(OrderBy<>),
                                        typeof(Asc<,>),
                                        this.Field,
                                        typeof(Desc<>),
                                            _RevisionField
                                    );

                    var cmd = BqlCommand.CreateInstance(select);
                    PXView view = new PXView(_Graph, true, cmd);

                    var prevKey = string.Empty;
                    foreach (var ret in view.SelectMulti())
                    {
                        var curKey = cache.GetValue(ret, _FieldOrdinal) as string;
                        if (curKey != prevKey)
                            yield return ret;
                        prevKey = curKey;
                    }
                }
            }

            private class Numbering : AutoNumberAttribute
            {
                private bool _IsAutoNumbering = true;
                public Numbering(Type setupField, Type dateField) : base(setupField, dateField) { }

                public void SetAutoNumbering(bool isActivated)
                {
                    _IsAutoNumbering = isActivated;
                }

                public string GetNewSymbol()
                {
                    return NewSymbol;
                }

                public bool IsAutoNumber()
                {
                    return UserNumbering != true;
                }

                protected override void Parameter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
                {
                    if (_IsAutoNumbering)
                    {
                        base.Parameter_RowSelected(sender, e);
                    }
                }

                public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
                {
                    var newKey = sender.GetValue(e.Row, _FieldOrdinal) as string;
                    if (this.NewSymbol == null || newKey?.Contains(this.NewSymbol) == true)
                        base.RowPersisting(sender, e);
                }

                public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
                {
                    var newKey = sender.GetValue(e.Row, _FieldOrdinal) as string;
                    if (this.NewSymbol == null || newKey?.Contains(this.NewSymbol) == true)
                        base.RowPersisted(sender, e);
                    else
                        SetAutoNumbering(true);
                }

            }

            #endregion

        }
    }
}