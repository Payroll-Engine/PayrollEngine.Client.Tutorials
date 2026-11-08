# Payroll Engine Client Tutorials - Lex compliance values

## Overview
Tutorial topic: Trace the compliance values of a data regulation to their official sources

| :zap: **No backend required. The tutorial works on a copy of a fictitious data satellite in the temporary folder.** |
|----|

## Prerequisites
- .NET 10
- Tutorial: Import exchange data (regulation and lookup structure)
- [Payroll Engine Lex](https://github.com/Payroll-Engine/PayrollEngine.Lex)

## Learnings
- Lex manifest `lex.json`: entries, sources, evidence files, legal reference
- Edit the manifest without losing unknown properties
- Automatic source verification and the year rating of hits
- Resolve an ambiguous hit with a source quote or a manual verification
- Snapshot and snapshot verification
- Detect and apply differences between lex entries and regulation lookups
- Release check and check report
- Rollover to the next cycle

## Data Satellite
The fictitious satellite `Data.Demo.2026` holds the lookup `Rates` and its lex manifest:
```
Data.Demo.2026/
├── .lex/
│   ├── lex.json                  # manifest with 3 entries
│   └── demo-rates-2026.html      # local evidence of a fictitious authority
└── Regulation/
    └── Demo.Data.2026.json       # regulation with the lookup Rates
```

The regulation starts with an outdated contribution ceiling (`83100` instead of `84600`) and without the
tax-free allowance. The evidence names the minimum wage and the tax-free allowance next to the previous year.

## Notes
- Program usage
	- no arguments, run the program
	- the satellite is copied to `%TEMP%\PayrollEngine.Tutorial.Lex`, the project files stay unchanged
- Program structure (the matching Payroll Console command in parentheses)
	1. Validate the manifest (`LexSnapshot /dryrun`)
		- `Validator.Validate`
	2. Document a new value (`LexAdd`)
		- verify the candidate entry with `SourceVerifier.Verify` before adding it
		- without quote: `Ambiguous`, the amount stands next to the previous year
		- with the statute text as `sourceQuote`: `Found`
		- `ManifestEditor.AddValue` keeps the order and the unknown properties of `lex.json`
	3. Verify the entries against the evidence (`LexVerifySource`)
		- `ManifestVerifier.VerifyManifest` reads the local evidence of each entry
		- a clear hit writes the source location and an `auto` verification record
		- the minimum wage stays `Ambiguous`
	4. Attest the ambiguous entry (`LexAttest`)
		- a person confirms the value against the evidence: `manual` verification record
	5. Freeze the snapshot (`LexSnapshot`, `LexVerify`)
		- `SnapshotWriter` with `keepLocalFiles`: the evidence is not downloaded again
		- `Verifier.Verify` compares manifest, snapshot and evidence
	6. Check the regulation lookups (`LexCheck`)
		- lex key `Lookup.Key` addresses the lookup value
		- difference: contribution ceiling
		- no lookup value: tax-free allowance
	7. Apply the lex values (`LexApply`)
		- dry run lists the changes, the second run writes the regulation file in place
	8. Release check (`LexCheck /release`)
		- `Checker.CheckSources` lists the entries that still need a manual verification
		- `CheckReport.Write` creates `LexCheck.md`
	9. Roll over to the next cycle (`LexRollover`)
		- dry run: new folder `Data.Demo.2027`, all entries open
		- lines with the previous year are listed for review (new evidence for the new cycle)
