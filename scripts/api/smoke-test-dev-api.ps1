param(
    [string]$ProjectId = "dva-sop-dev",
    [string]$Region = "australia-southeast1",
    [string]$ServiceName = "vapi-dev-api",
    [string]$ServiceUrl = ""
)

$ErrorActionPreference = "Stop"

function Get-ServiceUrl {
    param(
        [string]$ProjectId,
        [string]$Region,
        [string]$ServiceName
    )

    if (-not [string]::IsNullOrWhiteSpace($ServiceUrl)) {
        return $ServiceUrl.TrimEnd("/")
    }

    $url = gcloud run services describe $ServiceName `
        --region=$Region `
        --project=$ProjectId `
        --format="value(status.url)"

    if ([string]::IsNullOrWhiteSpace($url)) {
        throw "Could not find Cloud Run service URL for $ServiceName."
    }

    return $url.TrimEnd("/")
}

function Test-Get {
    param(
        [string]$Name,
        [string]$Url,
        [int]$ExpectedStatus
    )

    try {
        $response = Invoke-WebRequest -Uri $Url -Method Get -UseBasicParsing
        $actualStatus = [int]$response.StatusCode
    }
    catch {
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $actualStatus = [int]$_.Exception.Response.StatusCode
        }
        else {
            Write-Host "[FAIL] $Name - request failed without HTTP status"
            Write-Host $_.Exception.Message
            return $false
        }
    }

    if ($actualStatus -eq $ExpectedStatus) {
        Write-Host "[PASS] $Name - HTTP $actualStatus"
        return $true
    }

    Write-Host "[FAIL] $Name - expected HTTP $ExpectedStatus but got HTTP $actualStatus"
    return $false
}

function Test-PostJson {
    param(
        [string]$Name,
        [string]$Url,
        [string]$Body,
        [int]$ExpectedStatus
    )

    try {
        $response = Invoke-WebRequest `
            -Uri $Url `
            -Method Post `
            -ContentType "application/json" `
            -Body $Body `
            -UseBasicParsing

        $actualStatus = [int]$response.StatusCode
    }
    catch {
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $actualStatus = [int]$_.Exception.Response.StatusCode
        }
        else {
            Write-Host "[FAIL] $Name - request failed without HTTP status"
            Write-Host $_.Exception.Message
            return $false
        }
    }

    if ($actualStatus -eq $ExpectedStatus) {
        Write-Host "[PASS] $Name - HTTP $actualStatus"
        return $true
    }

    Write-Host "[FAIL] $Name - expected HTTP $ExpectedStatus but got HTTP $actualStatus"
    return $false
}

$baseUrl = Get-ServiceUrl -ProjectId $ProjectId -Region $Region -ServiceName $ServiceName

Write-Host ""
Write-Host "Virtual Advocate PI API smoke test"
Write-Host "Service URL: $baseUrl"
Write-Host ""

$workspaceId = "00000000-0000-0000-0000-000000000000"
$conditionId = "00000000-0000-0000-0000-000000000000"

$results = @()

$results += Test-Get -Name "Health" -Url "$baseUrl/health" -ExpectedStatus 200
$results += Test-Get -Name "Database schema health" -Url "$baseUrl/api/v1/db/schema-health" -ExpectedStatus 200

$results += Test-Get -Name "Me requires auth" -Url "$baseUrl/api/v1/me" -ExpectedStatus 401
$results += Test-Get -Name "Claim workspaces require auth" -Url "$baseUrl/api/v1/claim-workspaces" -ExpectedStatus 401
$results += Test-Get -Name "Conditions require auth" -Url "$baseUrl/api/v1/claim-workspaces/$workspaceId/conditions" -ExpectedStatus 401
$results += Test-Get -Name "Accepted history requires auth" -Url "$baseUrl/api/v1/claim-workspaces/$workspaceId/conditions/$conditionId/accepted-history" -ExpectedStatus 401
$results += Test-Get -Name "Question responses require auth" -Url "$baseUrl/api/v1/claim-workspaces/$workspaceId/conditions/$conditionId/question-responses" -ExpectedStatus 401
$results += Test-Get -Name "Evidence metadata requires auth" -Url "$baseUrl/api/v1/claim-workspaces/$workspaceId/evidence-items" -ExpectedStatus 401
$results += Test-Get -Name "Audit events require auth" -Url "$baseUrl/api/v1/claim-workspaces/$workspaceId/audit-events" -ExpectedStatus 401
$results += Test-Get -Name "Evidence gaps require auth" -Url "$baseUrl/api/v1/claim-workspaces/$workspaceId/evidence-gaps" -ExpectedStatus 401
$results += Test-Get -Name "AI drafts require auth" -Url "$baseUrl/api/v1/claim-workspaces/$workspaceId/ai-drafts" -ExpectedStatus 401
$results += Test-Get -Name "Generated documents require auth" -Url "$baseUrl/api/v1/claim-workspaces/$workspaceId/generated-documents" -ExpectedStatus 401

$uploadBody = '{"evidenceType":"MEDICAL_REPORT","originalFileName":"test.pdf","fileType":"application/pdf"}'
$results += Test-PostJson -Name "Evidence upload URL requires auth" -Url "$baseUrl/api/v1/claim-workspaces/$workspaceId/conditions/$conditionId/evidence-upload-url" -Body $uploadBody -ExpectedStatus 401

Write-Host ""

$passed = ($results | Where-Object { $_ -eq $true }).Count
$total = $results.Count
$failed = $total - $passed

Write-Host "Smoke test result: $passed / $total passed"

if ($failed -gt 0) {
    Write-Host "$failed test(s) failed."
    exit 1
}

Write-Host "All smoke tests passed."
exit 0
