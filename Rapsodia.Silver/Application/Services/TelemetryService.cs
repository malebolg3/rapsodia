// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;

namespace Rapsodia.Silver.Application.Services;

public class TelemetryService
{
    private static readonly ActivitySource ActivitySource = new("Rapsodia.Silver", "1.0.0");
    private static readonly Regex SafeTagRegex = new(@"^[a-zA-Z0-9_\-\.]+$", RegexOptions.Compiled);
    private readonly Meter _meter;
    private readonly Counter<int> _scanCounter;
    private readonly Counter<int> _exploitCounter;
    private readonly Counter<int> _labCounter;
    private readonly Counter<int> _aiAnalysisCounter;
    private readonly Histogram<double> _scanDuration;
    private readonly Histogram<double> _aiAnalysisDuration;

    public TelemetryService()
    {
        _meter = new Meter("Rapsodia.Silver", "1.0.0");
        _scanCounter = _meter.CreateCounter<int>("scans_total", "scans");
        _exploitCounter = _meter.CreateCounter<int>("exploits_total", "exploits");
        _labCounter = _meter.CreateCounter<int>("labs_total", "labs");
        _aiAnalysisCounter = _meter.CreateCounter<int>("ai_analyses_total", "analyses");
        _scanDuration = _meter.CreateHistogram<double>("scan_duration_seconds", "s");
        _aiAnalysisDuration = _meter.CreateHistogram<double>("ai_analysis_duration_seconds", "s");
    }

    public void RecordScan(string scanType)
    {
        _scanCounter.Add(1, new KeyValuePair<string, object?>("scan_type", Sanitize(scanType)));
    }

    public void RecordExploit(string exploitName)
    {
        _exploitCounter.Add(1, new KeyValuePair<string, object?>("exploit", Sanitize(exploitName)));
    }

    public void RecordLabCreated(int level)
    {
        _labCounter.Add(1, new KeyValuePair<string, object?>("level", level));
    }

    public void RecordAIAnalysis(string model)
    {
        _aiAnalysisCounter.Add(1, new KeyValuePair<string, object?>("model", Sanitize(model)));
    }

    public Activity? StartActivity(string name)
    {
        return ActivitySource.StartActivity(Sanitize(name));
    }

    public void RecordScanDuration(double seconds)
    {
        if (seconds >= 0) _scanDuration.Record(seconds);
    }

    public void RecordAIAnalysisDuration(double seconds)
    {
        if (seconds >= 0) _aiAnalysisDuration.Record(seconds);
    }

    private static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "unknown";
        var trimmed = input.Trim();
        return SafeTagRegex.IsMatch(trimmed) ? trimmed : "invalid_format";
    }
}