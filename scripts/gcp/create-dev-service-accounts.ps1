$ProjectId = "dva-sop-dev"

$ServiceAccounts = @(
    @{
        Id = "vapi-dev-api"
        DisplayName = "Virtual Advocate PI Dev API"
        Description = "Main Cloud Run backend API runtime identity"
    },
    @{
        Id = "vapi-dev-aiworker"
        DisplayName = "Virtual Advocate PI Dev AI Worker"
        Description = "AI orchestration and drafting worker runtime identity"
    },
    @{
        Id = "vapi-dev-docgen"
        DisplayName = "Virtual Advocate PI Dev Document Generator"
        Description = "DOCX and PDF document generation runtime identity"
    },
    @{
        Id = "vapi-dev-build"
        DisplayName = "Virtual Advocate PI Dev Build Deploy"
        Description = "Build and deployment identity for Cloud Build or CI/CD"
    },
    @{
        Id = "vapi-dev-adminops"
        DisplayName = "Virtual Advocate PI Dev Admin Ops"
        Description = "Optional setup automation identity"
    }
)

foreach ($ServiceAccount in $ServiceAccounts) {
    Write-Host "Creating service account $($ServiceAccount.Id) ..."

    gcloud iam service-accounts create $ServiceAccount.Id `
        --display-name="$($ServiceAccount.DisplayName)" `
        --description="$($ServiceAccount.Description)" `
        --project=$ProjectId
}
