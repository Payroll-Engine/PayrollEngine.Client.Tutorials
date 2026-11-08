using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PayrollEngine.Lex;

namespace PayrollEngine.Client.Tutorial.LexComplianceValues;

/// <summary>Lex compliance values tutorial program</summary>
internal static class Program
{
    private const string SatelliteName = "Data.Demo.2026";
    private const string EvidenceFileName = "demo-rates-2026.html";
    private const string Attester = "Jane Doe";

    private const string AllowanceKey = "Rates.TaxFreeAllowance";
    private const string AllowanceQuote = "The annual tax-free allowance amounts to EUR 12,480.";
    private const string MinimumWageKey = "Rates.MinimumHourlyWage";

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>Program entry point</summary>
    static async Task Main()
    {
        // working copy: the tutorial changes the satellite, the project files stay unchanged
        var workDirectory = Path.Combine(Path.GetTempPath(), "PayrollEngine.Tutorial.Lex");
        if (Directory.Exists(workDirectory))
        {
            Directory.Delete(workDirectory, recursive: true);
        }
        var satelliteDirectory = Path.Combine(workDirectory, SatelliteName);
        CopyDirectory(Path.Combine(AppContext.BaseDirectory, SatelliteName), satelliteDirectory);
        var lexPath = Path.Combine(satelliteDirectory, ".lex", "lex.json");

        if (!await ValidateAsync(lexPath))
        {
            return;
        }
        await AddValueAsync(lexPath);
        await VerifySourcesAsync(lexPath);
        await AttestAsync(lexPath);
        await SnapshotAsync(lexPath);
        Check(await ReadManifestAsync(lexPath), lexPath);
        Apply(await ReadManifestAsync(lexPath), lexPath);
        await ReleaseCheckAsync(lexPath, workDirectory);
        RolloverPreview(satelliteDirectory);

        WriteTitle("Done");
        Console.WriteLine($"Working folder: {workDirectory}");
    }

    #region Steps

    /// <summary>Step 1: structural validation of the manifest</summary>
    /// <param name="lexPath">Path of lex.json</param>
    private static async Task<bool> ValidateAsync(string lexPath)
    {
        WriteTitle("1. Validate the manifest");
        var manifest = await ReadManifestAsync(lexPath);
        var errors = Validator.Validate(manifest, Path.GetDirectoryName(lexPath));
        if (errors.Count > 0)
        {
            errors.ForEach(x => Console.WriteLine($"  error: {x}"));
            return false;
        }
        Console.WriteLine($"  {manifest.Authority} {manifest.Regulation} from {manifest.ValidFrom}: {manifest.Values.Count} entries, valid");
        return true;
    }

    /// <summary>Step 2: verify a new value against its source before it is added to the manifest</summary>
    /// <param name="lexPath">Path of lex.json</param>
    private static async Task AddValueAsync(string lexPath)
    {
        WriteTitle("2. Document a new value");
        var lexDirectory = Path.GetDirectoryName(lexPath)!;
        var manifest = await ReadManifestAsync(lexPath);
        var document = await SourceDocument.LoadLocal(Path.Combine(lexDirectory, EvidenceFileName));
        var languages = LanguageSet.Get(manifest.Languages);

        // the source names the amount next to the previous year and in the statute text
        var withoutQuote = SourceVerifier.Verify(NewAllowance(quote: null), document, manifest.ValidFrom, languages);
        Console.WriteLine($"  without quote: {withoutQuote.Match} {withoutQuote.Ambiguity}");

        // the quote of the statute text anchors the amount
        var allowance = NewAllowance(AllowanceQuote);
        var withQuote = SourceVerifier.Verify(allowance, document, manifest.ValidFrom, languages);
        Console.WriteLine($"  with quote:    {withQuote.Match} {withQuote.Location}");
        if (withQuote.Match != SourceMatch.Found)
        {
            return;
        }

        var editor = ManifestEditor.Parse(await File.ReadAllTextAsync(lexPath));
        editor.AddValue(allowance);
        await editor.SaveAsync(lexPath);
        Console.WriteLine($"  added {allowance.Key}");
    }

    /// <summary>Step 3: look up the numbers of all entries in the local evidence and record the hits</summary>
    /// <param name="lexPath">Path of lex.json</param>
    private static async Task VerifySourcesAsync(string lexPath)
    {
        WriteTitle("3. Verify the entries against the evidence");
        var manifest = await ReadManifestAsync(lexPath);
        var results = await ManifestVerifier.VerifyManifest(manifest, Path.GetDirectoryName(lexPath));

        var editor = ManifestEditor.Parse(await File.ReadAllTextAsync(lexPath));
        foreach (var result in results)
        {
            Console.WriteLine($"  {result.Key,-28}{result.Match,-12}{result.Ambiguity ?? result.Location}");
            // only a clear hit gets an automatic verification record
            if (!result.CanWrite)
            {
                continue;
            }
            if (result.Location != null)
            {
                editor.SetSourceLocation(result.Key, result.Location);
            }
            editor.SetAutoVerification(result.Key, SourceVerifier.AutoVerification(Today, result.Evidence,
                result.Derived.Select(x => x.Property), result.Languages));
        }
        await editor.SaveAsync(lexPath);
        Console.WriteLine($"  {editor.Changes} change(s) written");
    }

    /// <summary>Step 4: a person confirms the ambiguous entry against the evidence</summary>
    /// <param name="lexPath">Path of lex.json</param>
    private static async Task AttestAsync(string lexPath)
    {
        WriteTitle("4. Attest the ambiguous entry");
        var editor = ManifestEditor.Parse(await File.ReadAllTextAsync(lexPath));
        editor.SetSourceLocation(MinimumWageKey, $"{EvidenceFileName}: Section \"Minimum hourly wage\"");
        var attest = editor.SetManualVerification(MinimumWageKey, new Verification
        {
            Mode = Verification.ModeManual,
            VerifiedOn = Today.ToString("yyyy-MM-dd"),
            VerifiedBy = Attester,
            Evidence = EvidenceFileName
        }, overwrite: false);
        await editor.SaveAsync(lexPath);
        Console.WriteLine($"  {MinimumWageKey}: {attest} by {Attester}");
    }

    /// <summary>Step 5: freeze manifest and evidence with their hashes</summary>
    /// <param name="lexPath">Path of lex.json</param>
    private static async Task SnapshotAsync(string lexPath)
    {
        WriteTitle("5. Freeze the snapshot");
        var lexDirectory = Path.GetDirectoryName(lexPath)!;
        var manifest = await ReadManifestAsync(lexPath);
        // keepLocalFiles: the evidence in the .lex folder is used, not downloaded again
        var snapshot = await new SnapshotWriter().WriteAsync(manifest, lexDirectory, lexDirectory,
            dryRun: false, keepLocalFiles: true);
        Console.WriteLine($"  {snapshot.SourceHash}");

        var differences = Verifier.Verify(manifest, snapshot, lexDirectory);
        Console.WriteLine(differences.Count == 0
            ? "  snapshot matches manifest and evidence"
            : string.Join(Environment.NewLine, differences.Select(x => $"  difference: {x}")));
    }

    /// <summary>Step 6: compare the lex entries with the lookup values of the regulation</summary>
    /// <param name="manifest">Manifest</param>
    /// <param name="lexPath">Path of lex.json</param>
    private static void Check(Manifest manifest, string lexPath)
    {
        WriteTitle("6. Check the regulation lookups");
        var result = Checker.Check(manifest, lexPath, requireLocation: true, requireVerification: true);
        WriteCheck(result);
    }

    /// <summary>Step 7: write the lex values into the regulation lookups</summary>
    /// <param name="manifest">Manifest</param>
    /// <param name="lexPath">Path of lex.json</param>
    private static void Apply(Manifest manifest, string lexPath)
    {
        WriteTitle("7. Apply the lex values");
        var preview = Applier.Apply(manifest, lexPath, dryRun: true);
        foreach (var change in preview.Changes)
        {
            Console.WriteLine($"  {change.Kind,-8}{change.EntryKey,-28}{change.Detail}");
        }
        var applied = Applier.Apply(manifest, lexPath, dryRun: false);
        foreach (var file in applied.WrittenFiles)
        {
            Console.WriteLine($"  written {Path.GetFileName(file)}");
        }
    }

    /// <summary>Step 8: release gate and check report</summary>
    /// <param name="lexPath">Path of lex.json</param>
    /// <param name="reportDirectory">Folder of the check report</param>
    private static async Task ReleaseCheckAsync(string lexPath, string reportDirectory)
    {
        WriteTitle("8. Release check");
        var manifest = await ReadManifestAsync(lexPath);
        var result = Checker.Check(manifest, lexPath, release: true);
        await Checker.CheckSources(manifest, result);
        WriteCheck(result);

        var reportPath = Path.Combine(reportDirectory, "LexCheck.md");
        CheckReport.Write([result], reportPath);
        Console.WriteLine($"  report {reportPath}");
    }

    /// <summary>Step 9: preview of the satellite for the next cycle</summary>
    /// <param name="satelliteDirectory">Satellite folder</param>
    private static void RolloverPreview(string satelliteDirectory)
    {
        WriteTitle("9. Roll over to the next cycle (dry run)");
        var result = Rollover.Run(satelliteDirectory, new DateTime(2027, 1, 1), dryRun: true);
        if (result.Error != null)
        {
            Console.WriteLine($"  error: {result.Error}");
            return;
        }
        Console.WriteLine($"  {Path.GetFileName(result.SourceDirectory)} -> {Path.GetFileName(result.TargetDirectory)}, " +
                          $"{result.OpenEntries} open entries");
        foreach (var mention in result.YearMentions)
        {
            Console.WriteLine($"  review {mention.File}({mention.Line}): {mention.Text}");
        }
    }

    #endregion

    #region Helpers

    /// <summary>Lex entry of the tax-free allowance</summary>
    /// <param name="quote">Source quote, null without quote</param>
    private static Entry NewAllowance(string quote) => new()
    {
        Key = AllowanceKey,
        Content = "12480",
        Unit = "EUR",
        SourceUrls = ["https://example.org/demo-tax-authority/payroll-rates"],
        SourceQuote = quote,
        LegalReference = "Demo Income Tax Act, section 4(2)",
        DocumentRef = EvidenceFileName,
        Description = "Annual tax-free allowance"
    };

    /// <summary>Display the findings of a check</summary>
    /// <param name="result">Check result</param>
    private static void WriteCheck(CheckResult result)
    {
        foreach (var drift in result.DriftSamples)
        {
            Console.WriteLine($"  difference:   {drift}");
        }
        foreach (var orphan in result.OrphanSamples)
        {
            Console.WriteLine($"  no lookup:    {orphan}");
        }
        foreach (var uncovered in result.UncoveredSamples)
        {
            Console.WriteLine($"  no lex entry: {uncovered}");
        }
        foreach (var unverified in result.UnverifiedSamples)
        {
            Console.WriteLine($"  unverified:   {unverified}");
        }
        foreach (var manual in result.ManualVerificationSamples)
        {
            Console.WriteLine($"  manual check: {manual}");
        }
        Console.WriteLine($"  {result.LexEntries} entries ({result.VerifiedAuto} auto, {result.VerifiedManual} manual), " +
                          $"{result.RegulationValues} lookup values: " +
                          (result.HasFindings ? "findings" : "no findings"));
    }

    /// <summary>Read the manifest from lex.json</summary>
    /// <param name="lexPath">Path of lex.json</param>
    private static async Task<Manifest> ReadManifestAsync(string lexPath) =>
        JsonSerializer.Deserialize<Manifest>(await File.ReadAllTextAsync(lexPath), ReadOptions);

    /// <summary>Copy a folder with its subfolders</summary>
    /// <param name="source">Source folder</param>
    /// <param name="target">Target folder</param>
    private static void CopyDirectory(string source, string target)
    {
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var targetFile = Path.Combine(target, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
            File.Copy(file, targetFile);
        }
    }

    /// <summary>Display a step title</summary>
    /// <param name="title">Step title</param>
    private static void WriteTitle(string title)
    {
        Console.WriteLine();
        Console.WriteLine(title);
    }

    #endregion
}
