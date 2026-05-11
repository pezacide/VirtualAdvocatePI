$ProjectId = "dva-sop-dev"

$ApiServiceAccount = "vapi-dev-api@$ProjectId.iam.gserviceaccount.com"
$AiWorkerServiceAccount = "vapi-dev-aiworker@$ProjectId.iam.gserviceaccount.com"
$DocGenServiceAccount = "vapi-dev-docgen@$ProjectId.iam.gserviceaccount.com"

$Bindings = @(
    @{
        Secret = "vapi-dev-db-connection-string"
        Members = @($ApiServiceAccount, $DocGenServiceAccount)
    },
    @{
        Secret = "vapi-dev-app-settings"
        Members = @($ApiServiceAccount)
    },
    @{
        Secret = "vapi-dev-ai-settings"
        Members = @($AiWorkerServiceAccount)
    },
    @{
        Secret = "vapi-dev-docai-processor-id"
        Members = @($AiWorkerServiceAccount)
    },
    @{
        Secret = "vapi-dev-jwt-signing-key"
        Members = @($ApiServiceAccount)
    }
)

foreach ($Binding in $Bindings) {
    foreach ($Member in $Binding.Members) {
        Write-Host "Granting Secret Accessor on $($Binding.Secret) to $Member ..."

        gcloud secrets add-iam-policy-binding $Binding.Secret `
            --member="serviceAccount:$Member" `
            --role="roles/secretmanager.secretAccessor" `
            --project=$ProjectId
    }
}
