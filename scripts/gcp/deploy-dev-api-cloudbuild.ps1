$ProjectId = "dva-sop-dev"
$Region = "australia-southeast1"

gcloud config set project $ProjectId

gcloud builds submit `
    --config=cloudbuild.yaml `
    --project=$ProjectId `
    --substitutions=_REGION=$Region
