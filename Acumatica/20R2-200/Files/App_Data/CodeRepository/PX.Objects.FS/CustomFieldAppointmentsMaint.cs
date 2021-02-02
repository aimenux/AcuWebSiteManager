using PX.Data;
using System.Collections;

namespace PX.Objects.FS
{
    public class CustomFieldAppointmentsMaint : PXGraph<CustomFieldAppointmentsMaint, FSCustomFieldAppointment>
    {
        [PXImport(typeof(FSCustomFieldAppointment))]
        public PXSelectOrderBy<FSCustomFieldAppointment,
               OrderBy<
                   Asc<FSCustomFieldAppointment.position>>> CustomFieldAppointmentRecords;

        #region ActionButtons
        public PXAction<FSCustomFieldAppointment> up;
        public PXAction<FSCustomFieldAppointment> down;      
        #region PositionEvent
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowUp)]
        public virtual IEnumerable Up(PXAdapter adapter)
        {
            FSCustomFieldAppointment fsCustomFieldAppointmentRow = (FSCustomFieldAppointment)CustomFieldAppointmentRecords.Cache.Current;

            PXResultset<FSCustomFieldAppointment> bqlResultSet = CustomFieldAppointmentRecords.Select();

            if (bqlResultSet != null && bqlResultSet.Count > 1)
            {
                int currentCustomFieldAppointmentIndex = (int)fsCustomFieldAppointmentRow.Position;

                if (currentCustomFieldAppointmentIndex > 0)
                {
                    FSCustomFieldAppointment fsCustomFieldAppointmentRow_Previous = (FSCustomFieldAppointment)bqlResultSet[currentCustomFieldAppointmentIndex - 1];
                    fsCustomFieldAppointmentRow.Position = fsCustomFieldAppointmentRow_Previous.Position;
                    fsCustomFieldAppointmentRow_Previous.Position = currentCustomFieldAppointmentIndex;

                    CustomFieldAppointmentRecords.Update(fsCustomFieldAppointmentRow);
                    CustomFieldAppointmentRecords.Update(fsCustomFieldAppointmentRow_Previous);
                }
            }
            
            return adapter.Get();
        }

        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowDown)]
        public virtual IEnumerable Down(PXAdapter adapter)
        {
            FSCustomFieldAppointment fsCustomFieldAppointmentRow = (FSCustomFieldAppointment)CustomFieldAppointmentRecords.Cache.Current;

            PXResultset<FSCustomFieldAppointment> bqlResultSet = CustomFieldAppointmentRecords.Select();

            if (bqlResultSet != null && bqlResultSet.Count > 1)
            {
                int currentCustomFieldAppointmentIndex = (int)fsCustomFieldAppointmentRow.Position;

                if (currentCustomFieldAppointmentIndex < bqlResultSet.Count - 1)
                {
                    FSCustomFieldAppointment fsCustomFieldAppointmentRow_Next = (FSCustomFieldAppointment)bqlResultSet[currentCustomFieldAppointmentIndex + 1];
                    fsCustomFieldAppointmentRow.Position = fsCustomFieldAppointmentRow_Next.Position;
                    fsCustomFieldAppointmentRow_Next.Position = currentCustomFieldAppointmentIndex;

                    CustomFieldAppointmentRecords.Update(fsCustomFieldAppointmentRow);
                    CustomFieldAppointmentRecords.Update(fsCustomFieldAppointmentRow_Next);
                }
            }

            return adapter.Get();
        }
        #endregion
        #endregion
        #region PrivateMethods
        public virtual IEnumerable customFieldAppointmentRecords()
        {
            PXSelectOrderBy<FSCustomFieldAppointment, OrderBy<Asc<FSCustomFieldAppointment.position>>>.Clear(this);
            return PXSelectOrderBy<FSCustomFieldAppointment, OrderBy<Asc<FSCustomFieldAppointment.position>>>.Select(this);
        }
        #endregion

        #region Event Handlers

        #region FSCustomFieldAppointment

        protected virtual void _(Events.RowInserting<FSCustomFieldAppointment> e)
        {
            if (e.Row == null) 
            { 
                return; 
            }

            FSCustomFieldAppointment fsCustomFieldAppointmentRow = (FSCustomFieldAppointment)e.Row;

            int maxSequence = 0;

            PXResultset<FSCustomFieldAppointment> bqlResultSet = CustomFieldAppointmentRecords.Select();

            if (bqlResultSet != null && bqlResultSet.Count > 0)
            {
                maxSequence = (int)(((FSCustomFieldAppointment)bqlResultSet[bqlResultSet.Count - 1]).Position + 1);
            }

            fsCustomFieldAppointmentRow.Position = maxSequence;
        }
        #endregion

        #endregion
    }
}