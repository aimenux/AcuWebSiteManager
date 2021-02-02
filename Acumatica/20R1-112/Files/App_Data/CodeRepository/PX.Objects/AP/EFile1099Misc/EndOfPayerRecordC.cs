using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.AP
{
    /// <summary>
    /// End of Payer Record (C)
    /// File format is based on IRS publication 1220 (http://www.irs.gov/pub/irs-pdf/p1220.pdf)
    /// </summary>
    public class EndOfPayerRecordC
    {
        [FixedLength(StartPosition = 1, FieldLength = 1)]
        public string RecordType { get; set; }

        [FixedLength(StartPosition = 2, FieldLength = 8, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public string NumberOfPayees { get; set; }

        [FixedLength(StartPosition = 10, FieldLength = 6)]
        public string Blank1 { get; set; }

        [FixedLength(StartPosition = 16, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotal1 { get; set; }

        [FixedLength(StartPosition = 34, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotal2 { get; set; }

        [FixedLength(StartPosition = 52, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotal3 { get; set; }

        [FixedLength(StartPosition = 70, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotal4 { get; set; }

        [FixedLength(StartPosition = 88, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotal5 { get; set; }

        [FixedLength(StartPosition = 106, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotal6 { get; set; }

        [FixedLength(StartPosition = 124, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotal7 { get; set; }

        [FixedLength(StartPosition = 142, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotal8 { get; set; }

        [FixedLength(StartPosition = 160, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotal9 { get; set; }

        [FixedLength(StartPosition = 178, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotalA { get; set; }

        [FixedLength(StartPosition = 196, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotalB { get; set; }

        [FixedLength(StartPosition = 214, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotalC { get; set; }

        [FixedLength(StartPosition = 232, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotalD { get; set; }

        [FixedLength(StartPosition = 250, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotalE { get; set; }

        [FixedLength(StartPosition = 268, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotalF { get; set; }

        [FixedLength(StartPosition = 286, FieldLength = 18, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public decimal ControlTotalG { get; set; }

        [FixedLength(StartPosition = 304, FieldLength = 196, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public string Blank2 { get; set; }

        [FixedLength(StartPosition = 500, FieldLength = 8, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
        public string RecordSequenceNumber { get; set; }

        [FixedLength(StartPosition = 508, FieldLength = 24)]
        public string Blank3 { get; set; }

        [FixedLength(StartPosition = 749, FieldLength = 2)]
        public string Blank4 { get; set; }
    }    
}
