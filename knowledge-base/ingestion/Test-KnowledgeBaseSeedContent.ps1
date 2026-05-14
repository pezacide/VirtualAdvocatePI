param(
    [string]$RepoRoot = "C:\Projects\VirtualAdvocatePI"
)

$ErrorActionPreference = "Stop"

$SeedPath = Join-Path $RepoRoot "knowledge-base\seed-content\knowledge-base-chunks.seed.jsonl"
$TaxonomyPath = Join-Path $RepoRoot "knowledge-base\source-registry\source-category-taxonomy.json"
$RegistryPath = Join-Path $RepoRoot "knowledge-base\source-registry\approved-source-registry.loaded.seed.json"

if (-not (Test-Path $SeedPath)) {
    throw "Seed file not found: $SeedPath"
}

if (-not (Test-Path $TaxonomyPath)) {
    throw "Taxonomy file not found: $TaxonomyPath"
}

if (-not (Test-Path $RegistryPath)) {
    throw "Loaded registry file not found: $RegistryPath"
}

$taxonomy = Get-Content $TaxonomyPath -Raw | ConvertFrom-Json
$registry = Get-Content $RegistryPath -Raw | ConvertFrom-Json
$chunks = Get-Content $SeedPath | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object {
    $_ | ConvertFrom-Json
}

$categoryKeys = @($taxonomy.categories | ForEach-Object { $_.categoryKey })
$sourceKeys = @($registry.entries | ForEach-Object { $_.sourceKey })

$requiredChunkFields = @(
    "chunkKey",
    "sourceKey",
    "category",
    "sourceType",
    "citationLabel",
    "chunkTitle",
    "retrievalUse",
    "content",
    "safetyNotes",
    "approvalStatus",
    "isActive",
    "status"
)

$errors = New-Object System.Collections.Generic.List[string]

foreach ($chunk in $chunks) {
    foreach ($field in $requiredChunkFields) {
        if (-not $chunk.PSObject.Properties.Name.Contains($field)) {
            $errors.Add("Chunk missing field '$field': $($chunk.chunkKey)")
        }
    }

    if ($categoryKeys -notcontains $chunk.category) {
        $errors.Add("Chunk category is not in taxonomy: $($chunk.chunkKey) => $($chunk.category)")
    }

    if ($sourceKeys -notcontains $chunk.sourceKey) {
        $errors.Add("Chunk sourceKey is not in loaded source registry: $($chunk.chunkKey) => $($chunk.sourceKey)")
    }

    if ($chunk.approvalStatus -ne "APPROVED") {
        $errors.Add("Chunk is not approved: $($chunk.chunkKey)")
    }

    if ($chunk.isActive -ne $true) {
        $errors.Add("Chunk is not active: $($chunk.chunkKey)")
    }

    if ($chunk.status -ne "ACTIVE") {
        $errors.Add("Chunk status is not ACTIVE: $($chunk.chunkKey)")
    }

    if ([string]::IsNullOrWhiteSpace($chunk.citationLabel)) {
        $errors.Add("Chunk missing citationLabel: $($chunk.chunkKey)")
    }

    if ([string]::IsNullOrWhiteSpace($chunk.content)) {
        $errors.Add("Chunk missing content: $($chunk.chunkKey)")
    }

    if ([string]::IsNullOrWhiteSpace($chunk.safetyNotes)) {
        $errors.Add("Chunk missing safetyNotes: $($chunk.chunkKey)")
    }
}

if ($errors.Count -gt 0) {
    Write-Host "Knowledge base seed validation failed." -ForegroundColor Red
    $errors | ForEach-Object { Write-Host "- $_" -ForegroundColor Red }
    exit 1
}

Write-Host "Knowledge base seed validation passed." -ForegroundColor Green
Write-Host "Chunk count: $($chunks.Count)"