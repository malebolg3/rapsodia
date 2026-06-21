// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System;
using System.IO;
using System.Security.Cryptography;
using Rapsodia.Blue.Application.Interfaces.Olimpo;

namespace Rapsodia.Blue.Application.Services.Olimpo;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService()
    {
        var keyBase64 = Environment.GetEnvironmentVariable("OLP_KEY")
            ?? throw new InvalidOperationException("OLP_KEY nao configurada");
        _key = Convert.FromBase64String(keyBase64);
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, 16);
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Key = _key;
        var iv = new byte[16];
        Array.Copy(fullCipher, 0, iv, 0, 16);
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(fullCipher, 16, fullCipher.Length - 16);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}