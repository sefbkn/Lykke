﻿namespace NDecred.Common
{
    public enum TxSerializeType : uint
    {
        Full = 0,
        NoWitness,
        OnlyWitness,
        WitnessSigning,
        WitnessValueSigning
    }
}
