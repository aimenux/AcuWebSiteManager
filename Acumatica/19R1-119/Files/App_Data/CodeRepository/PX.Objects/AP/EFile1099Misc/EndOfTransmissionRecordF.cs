using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.AP
{
    /// <summary>
    /// End of Transmission Record (F)
    /// File format is based on IRS publication 1220 (http://www.irs.gov/pub/irs-pdf/p1220.pdf)
    /// </summary>
    public class EndOfTransmissionRecordF
    {
        [FixedLength(StartPosition = 1, FieldLength = 1)]
        public string RecordType { get; set; }

        [FixedLength(StartPosition = 2, FieldLength = 8, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public string NumberOfARecords { get; set; }

        [FixedLength(StartPosition = 10, FieldLength = 21, PaddingChar = '0')]
        public string Zero1 { get; set; }

        [FixedLength(StartPosition = 31, FieldLength = 19)]
        public string Blank1 { get; set; }

        [FixedLength(StartPosition = 50, FieldLength = 8, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public string TotalNumberOfPayees { get; set; }

        [FixedLength(StartPosition = 58, FieldLength = 442)]
        public string Blank2 { get; set; }

        [FixedLength(StartPosition = 500, FieldLength = 8, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public string RecordSequenceNumber { get; set; }

        [FixedLength(StartPosition = 508, FieldLength = 241)]
        public string Blank3 { get; set; }

        [FixedLength(StartPosition = 749, FieldLength = 2)]
        public string Blank4 { get; set; }
    }
}
