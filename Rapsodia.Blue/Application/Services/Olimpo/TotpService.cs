// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Rapsodia.Blue.Application.Interfaces.Olimpo;

namespace Rapsodia.Blue.Application.Services.Olimpo;

public class TotpService : ITotpService
{
    public string GenerateCode(string secret, string algorithm, int digits, int period)
    {
        var counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / period;
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes);

        var secretBytes = Base32Decode(secret);
        using var hmac = algorithm switch
        {
            "SHA256" => (HMAC)new HMACSHA256(secretBytes),
            "SHA512" => (HMAC)new HMACSHA512(secretBytes),
            _ => (HMAC)new HMACSHA1(secretBytes)
        };

        var hash = hmac.ComputeHash(counterBytes);
        var offset = hash[^1] & 0x0F;
        var code = (hash[offset] & 0x7F) << 24
                 | (hash[offset + 1] & 0xFF) << 16
                 | (hash[offset + 2] & 0xFF) << 8
                 | (hash[offset + 3] & 0xFF);

        return (code % (int)Math.Pow(10, digits)).ToString().PadLeft(digits, '0');
    }

    private static byte[] Base32Decode(string base32)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        base32 = base32.TrimEnd('=').ToUpper();
        var bytes = new List<byte>();
        int buffer = 0, bits = 0;

        foreach (var c in base32)
        {
            var value = alphabet.IndexOf(c);
            if (value < 0) continue;
            buffer = (buffer << 5) | value;
            bits += 5;
            if (bits >= 8)
            {
                bytes.Add((byte)(buffer >> (bits - 8)));
                bits -= 8;
            }
        }
        return bytes.ToArray();
    }
}