﻿// Copyright (c) 2016 The btcsuite developers
// Copyright (c) 2016 The Decred developers
// Licensed under the ISC license.  See LICENSE file in the project root for full license information.

using Paymetheus.Decred.Util;
using System;
using System.Security.Cryptography;

namespace Paymetheus.Decred.Wallet
{
    public static class WalletSeed
    {
        /// <summary>
        /// The length in bytes of the wallet's BIP0032 seed.
        /// </summary>
        public const int SeedLength = 32;

        private static readonly RNGCryptoServiceProvider CRandom = new RNGCryptoServiceProvider();
        
        public static byte[] GenerateRandomSeed()
        {
            var bytes = new byte[SeedLength];
            CRandom.GetBytes(bytes);
            return bytes;
        }
        
        /// <summary>
        /// Decodes user input as either the hexadecimal or PGP word list encoding
        /// of a seed and validates the seed length.
        /// </summary>
        public static byte[] DecodeAndValidateUserInput(string userInput, PgpWordList pgpWordList)
        {
            if (userInput == null)
                throw new ArgumentNullException(nameof(userInput));
            if (pgpWordList == null)
                throw new ArgumentNullException(nameof(pgpWordList));

            var decodedInput = DecodeUserInput(userInput, pgpWordList);

            switch (decodedInput.Length)
            {
                // No checksum
                case SeedLength:
                    return decodedInput;

                // Extra byte of checksum appended to end
                case SeedLength + 1:
                    var seed = new byte[SeedLength];
                    Array.Copy(decodedInput, seed, SeedLength);
                    var digest = DoubleSha256(seed);
                    if (decodedInput[SeedLength] != digest[0])
                    {
                        throw new ChecksumException("Invalid checksum");
                    }
                    return seed;

                // Wrong size or seed is using a newer format not understood yet
                default:
                    throw new Exception($"Decoded seed must have byte length {SeedLength}");
            }
        }

        private static byte[] DecodeUserInput(string userInput, PgpWordList pgpWordList)
        {
            byte[] seed;
            if (Hexadecimal.TryDecode(userInput, out seed))
                return seed;

            var splitInput = userInput.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            if (splitInput.Length == 1)
            {
                // Hex decoding failed, but it's not a multi-word mneumonic either.
                // Assume the user intended hex.
                throw new HexadecimalEncodingException();
            }
            return pgpWordList.Decode(splitInput);
        }

        // Returned array contains the double SHA256 hash.
        public static byte[] DoubleSha256(byte[] value)
        {
            using (var hasher = new SHA256Managed())
            {
                var intermediateHash = hasher.ComputeHash(value);
                return hasher.ComputeHash(intermediateHash);
            }
        }

        public static string[] EncodeWordList(PgpWordList pgpWordList, byte[] seed)
        {
            var seedHash = DoubleSha256(seed);
            var seedWithChecksum = new byte[seed.Length + 1];
            Array.Copy(seed, seedWithChecksum, seed.Length);
            seedWithChecksum[seedWithChecksum.Length - 1] = seedHash[0];
            return pgpWordList.Encode(seedWithChecksum);
        }
    }
}
