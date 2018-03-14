using System;

namespace NDecred.Common
{
    [Flags]
    public enum SignatureHashType
    {
        Old          = 0x0,
        All          = 0x1,
        None         = 0x2,
        Single       = 0x3,
        AllValue     = 0x4,
        AnyOneCanPay = 0x80
    }
}