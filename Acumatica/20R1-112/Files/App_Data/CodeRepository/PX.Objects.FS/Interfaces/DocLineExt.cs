using PX.Data;
using PX.Objects.IN;
using PX.Objects.PM;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class GroupDocLineExt
    {
        public int? DocID;
        public int? LineID;
        public List<DocLineExt> Group;

        public GroupDocLineExt(int? docID, int? lineID, List<DocLineExt> group)
        {
            this.DocID = docID;
            this.LineID = lineID;
            this.Group = group;
        }
    }

    public class DocLineExt
    {
        public IDocLine docLine;
        public FSPostDoc fsPostDoc;
        public FSSrvOrdType fsSrvOrdType;
        public FSServiceOrder fsServiceOrder;
        public FSAppointment fsAppointment;
        public FSPostInfo fsPostInfo;
        public FSSODet fsSODet;
        public FSSODetSplit fsSODetSplit;
        public INItemPlan inItemPlan;
        public PMTask pmTask;

        public DocLineExt(PXResult<FSSODet, FSServiceOrder, FSSrvOrdType, FSPostDoc, FSPostInfo, FSSODetSplit, INItemPlan, PMTask> soDetLine)
        {
            this.docLine = (IDocLine)(FSSODet)soDetLine;
            this.fsPostDoc = (FSPostDoc)soDetLine;
            this.fsServiceOrder = (FSServiceOrder)soDetLine;
            this.fsSrvOrdType = (FSSrvOrdType)soDetLine;
            this.fsAppointment = null;
            this.fsPostInfo = (FSPostInfo)soDetLine;
            this.fsSODet = (FSSODet)soDetLine;
            this.fsSODetSplit = (FSSODetSplit)soDetLine;
            this.inItemPlan = (INItemPlan)soDetLine;
            this.pmTask = (PMTask)soDetLine;
        }

        public DocLineExt(PXResult<FSSODet, FSServiceOrder, FSSrvOrdType, FSSODetSplit, INItemPlan, PMTask> soDetLine)
        {
            this.docLine = (IDocLine)(FSSODet)soDetLine;
            this.fsPostDoc = null;
            this.fsServiceOrder = (FSServiceOrder)soDetLine;
            this.fsSrvOrdType = (FSSrvOrdType)soDetLine;
            this.fsAppointment = null;
            this.fsPostInfo = null;
            this.fsSODet = (FSSODet)soDetLine;
            this.fsSODetSplit = (FSSODetSplit)soDetLine;
            this.inItemPlan = (INItemPlan)soDetLine;
            this.pmTask = (PMTask)soDetLine;

        }

        public DocLineExt(PXResult<FSAppointmentDet, FSAppointment, FSServiceOrder, FSSrvOrdType, FSPostDoc, FSPostInfo, FSSODet, FSSODetSplit, INItemPlan, PMTask> appointmentDetLine)
        {
            this.docLine = (IDocLine)(FSAppointmentDet)appointmentDetLine;
            this.fsPostDoc = (FSPostDoc)appointmentDetLine;
            this.fsServiceOrder = (FSServiceOrder)appointmentDetLine;
            this.fsSrvOrdType = (FSSrvOrdType)appointmentDetLine;
            this.fsAppointment = (FSAppointment)appointmentDetLine;
            this.fsPostInfo = (FSPostInfo)appointmentDetLine;
            this.fsSODet = (FSSODet)appointmentDetLine;
            this.fsSODetSplit = (FSSODetSplit)appointmentDetLine;
            this.inItemPlan = (INItemPlan)appointmentDetLine;
            this.pmTask = (PMTask)appointmentDetLine;
        }

        public DocLineExt(PXResult<FSAppointmentDet, FSAppointment, FSServiceOrder, FSSrvOrdType, FSPostDoc, FSPostInfo, PMTask> appointmentDetLine)
        {
            this.docLine = (IDocLine)(FSAppointmentDet)appointmentDetLine;
            this.fsPostDoc = (FSPostDoc)appointmentDetLine;
            this.fsServiceOrder = (FSServiceOrder)appointmentDetLine;
            this.fsSrvOrdType = (FSSrvOrdType)appointmentDetLine;
            this.fsAppointment = (FSAppointment)appointmentDetLine;
            this.fsPostInfo = (FSPostInfo)appointmentDetLine;
            this.fsSODet = null;
            this.fsSODetSplit = null;
            this.inItemPlan = null;
            this.pmTask = (PMTask)appointmentDetLine;
        }
    }
}
