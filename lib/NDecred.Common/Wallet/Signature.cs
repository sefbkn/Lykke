using System.IO;
using System.Linq;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Math;

namespace NDecred.Common.Wallet
{
    public class Signature
    {
        private BigInteger CurveOrder { get; }
        private BigInteger HalfOrder { get; }
        
        internal Signature(BigInteger r, BigInteger s)
        {
            R = r;
            S = s;
	        
            CurveOrder = CustomNamedCurves.GetByOid(SecObjectIdentifiers.SecP256k1).N;
            HalfOrder = new BigInteger(CurveOrder.ToByteArray()).ShiftRight(1);
        }

        internal Signature(byte[] derSignature)
        {
            using (var decoder = new Asn1InputStream(derSignature))
            {
                var seq = (DerSequence) decoder.ReadObject();
                R = ((DerInteger) seq[0]).Value;
                S = ((DerInteger) seq[1]).Value;
            }
        }

        public BigInteger R { get; }
        public BigInteger S { get; }

        public byte[] ToDer()
        {
            // Usually 70-72 bytes.
            using (var ms = new MemoryStream(72))
            {
                var seq = new DerSequenceGenerator(ms);
                seq.AddObject(new DerInteger(R));
                seq.AddObject(new DerInteger(S.ToByteArray().Take(32).ToArray()));
                seq.Close();
                return ms.ToArray();
            }
        }
	    

        public Signature MakeCanonical()
        {
            if(!IsLowS)
            {
                return new Signature(this.R, CurveOrder.Subtract(this.S));
            }
            else
                return this;
        }

        public bool IsLowS
        {
            get
            {
                return this.S.CompareTo(HalfOrder) <= 0;
            }
        }

    }

}
