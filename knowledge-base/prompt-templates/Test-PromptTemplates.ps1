param(
    [string]$RepoRoot = "C:\Projects\VirtualAdvocatePI"
)

$ErrorActionPreference = "Stop"

$ManifestPath = Join-Path $RepoRoot "knowledge-base\prompt-templates\prompt-template-manifest.json"

if (-not (Test-Path $ManifestPath)) {
    throw "Prompt manifest not found: $ManifestPath"
}

$manifest = Get-Content $ManifestPath -Raw | ConvertFrom-Json

$errors = New-Object System.Collections.Generic.List[string]

$sharedPath = Join-Path $RepoRoot ($manifest.sharedGuardrails -replace "/", "\")
if (-not (Test-Path $sharedPath)) {
    $errors.Add("Shared guardrails file missing: $sharedPath")
}

foreach ($template in $manifest.templates) {
    $templatePath = Join-Path $RepoRoot ($template.path -replace "/", "\")

    if (-not (Test-Path $templatePath)) {
        $errors.Add("Template file missing: $templatePath")
        continue
    }

    $content = Get-Content $templatePath -Raw

    foreach ($requiredPhrase in @("Preparation-only", "Review note", "Do not", "Source")) {
        if ($content -notmatch [regex]::Escape($requiredPhrase)) {
            $errors.Add("Template missing required phrase '$requiredPhrase': $($template.draftTaskType)")
        }
    }
}

if ($errors.Count -gt 0) {
    Write-Host "Prompt template validation failed." -ForegroundColor Red
    $errors | ForEach-Object { Write-Host "- $_" -ForegroundColor Red }
    exit 1
}

Write-Host "Prompt template validation passed." -ForegroundColor Green
Write-Host "Template count: $($manifest.templates.Count)"