// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Blue.Application.Interfaces.Olimpo;

public interface ITotpService
{
    string GenerateCode(string secret, string algorithm, int digits, int period);
}