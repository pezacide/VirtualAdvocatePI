# Web Smoke Test Checklist

## App

Virtual Advocate PI

## Phase

Phase 4 - Web MVP shell

## Purpose

Use this checklist after each web change to confirm the main Phase 4 user journey still works.

## Checks

1. Run backend smoke test:

cd C:\Projects\VirtualAdvocatePI
.\scripts\api\smoke-test-dev-api.ps1

Expected: All smoke tests passed.

2. Build the web app:

cd C:\Projects\VirtualAdvocatePI\web
npm run build

Expected: build succeeds.

3. Start the web app:

npm run dev

Open http://localhost:3000

4. Public routes:

http://localhost:3000
http://localhost:3000/login
http://localhost:3000/env-check
http://localhost:3000/session-check

Expected: public routes load without sign-in.

5. Protected routes:

http://localhost:3000/dashboard
http://localhost:3000/claim-workspaces/demo-workspace-1

Expected: signed-out users are redirected to login.

6. Authentication:

Sign in with a Firebase test account.

Expected: header shows signed-in email and sign-out button.

7. Dashboard:

http://localhost:3000/dashboard

Expected: real claim workspaces load from the backend.

8. New workspace:

http://localhost:3000/claim-workspaces/new

Expected: creating a workspace succeeds and redirects to workspace detail.

9. Workspace detail:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID

Expected: workspace title, scenario, framework, status and section cards appear.

10. Condition intake:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/conditions

Expected: conditions load and a new condition can be added.

11. Accepted-condition history:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/accepted-history

Expected: accepted-condition history can be saved and displayed.

12. Guided questions:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/guided-questions

Expected: guided question response can be saved and displayed.

13. Evidence checklist:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/evidence-checklist

Expected: checklist shell loads and checkboxes can be ticked locally.

14. Evidence metadata:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/evidence-metadata

Expected: evidence metadata can be saved and displayed.

15. Evidence upload:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/evidence-upload

Expected: file upload creates signed URL, uploads to Cloud Storage and marks item uploaded.

16. Evidence gaps:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/evidence-gaps

Expected: gaps can be recalculated and statuses updated.

17. AI drafts:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/ai-drafts

Expected: draft metadata can be created, edited and review status updated.

18. Generated documents:

http://localhost:3000/claim-workspaces/YOUR_REAL_WORKSPACE_ID/generated-documents

Expected: document metadata can be created and status updated.

19. Sign out:

Use the header sign-out button.

Expected: protected routes redirect to login again.

20. Safety wording:

Confirm the app does not claim to provide legal advice, medical advice, DVA decision advice, compensation estimates or claim outcome guarantees.

## Pass criteria

Phase 4 smoke test passes when backend smoke test passes, web build succeeds, protected routes redirect, authentication works, dashboard loads backend data, core workspace pages load, create/read flows work, no secrets are committed, and preparation-only wording remains visible.
