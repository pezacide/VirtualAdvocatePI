"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import { AdminMeResponse, getAdminMe } from "@/lib/api";

const sections = [
  {
    title: "Admin access",
    items: [
      "Sign in as configured admin.",
      "Open /admin/access-check.",
      "Confirm Role is ADMIN.",
      "Confirm Account status is ACTIVE.",
      "Confirm Is admin is Yes.",
      "Confirm Protected ping succeeds.",
      "Confirm normal users are denied admin access.",
    ],
  },
  {
    title: "Admin dashboard",
    items: [
      "Open /admin.",
      "Confirm admin dashboard loads.",
      "Confirm admin status card displays signed-in admin email.",
      "Confirm all Phase 10 admin tool cards are visible.",
      "Open each admin tool card.",
    ],
  },
  {
    title: "Source metadata manager",
    items: [
      "Open /admin/source-metadata.",
      "Confirm source registry entries load.",
      "Apply search filter.",
      "Select a source entry.",
      "Edit Review notes.",
      "Save source metadata.",
      "Refresh and confirm saved changes remain.",
      "Confirm source key is not editable.",
    ],
  },
  {
    title: "Question and document templates",
    items: [
      "Open /admin/templates/questions.",
      "Create and edit a QUESTION template.",
      "Open /admin/templates/documents.",
      "Create and edit a DOCUMENT template.",
      "Confirm TemplateKey is locked after creation.",
      "Refresh and confirm saved changes remain.",
    ],
  },
  {
    title: "Prompt and disclaimer versioning",
    items: [
      "Open /admin/prompts-disclaimers.",
      "Create a test PROMPT version.",
      "Create a test DISCLAIMER version.",
      "Edit review notes.",
      "Save changes.",
      "Confirm VersionKey is locked after creation.",
    ],
  },
  {
    title: "Knowledge audit and admin audit logging",
    items: [
      "Open /admin/knowledge-audit.",
      "Confirm audit events load.",
      "Confirm event type summary appears.",
      "Select an audit event and review its details.",
      "Make a source metadata change and confirm ADMIN_SOURCE_REGISTRY_UPDATED appears.",
      "Make a template change and confirm ADMIN_TEMPLATE_UPDATED appears.",
      "Make a prompt/disclaimer change and confirm ADMIN_PROMPT_DISCLAIMER_VERSION_UPDATED appears.",
      "Confirm request bodies are not logged.",
    ],
  },
  {
    title: "Safety boundary",
    items: [
      "Admin tools deny unauthenticated users.",
      "Admin tools deny non-admin users.",
      "Admin tools do not bypass veteran workspace ownership checks.",
      "Admin tools do not silently alter generated claim content.",
      "Admin write actions are auditable.",
    ],
  },
  {
    title: "Build and deploy checks",
    items: [
      "Backend build passes.",
      "Web build passes.",
      "Cloud Run backend deploy succeeds.",
      "Web app works against Cloud Run backend.",
    ],
  },
];

export function AdminSmokeTestChecklistPanel() {
  const { user, loading, getIdToken } = useAuth();

  const [adminStatus, setAdminStatus] = useState<AdminMeResponse | null>(null);
  const [errorMessage, setErrorMessage] = useState("");

  async function loadAdminStatus() {
    setErrorMessage("");

    try {
      const token = await getIdToken();

      if (!token) {
        setErrorMessage("No Firebase ID token is available. Please sign in again.");
        return;
      }

      const me = await getAdminMe(token);
      setAdminStatus(me);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load admin access status.";
      setErrorMessage(message);
    }
  }

  useEffect(() => {
    if (!loading && user) {
      loadAdminStatus();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user]);

  if (loading) {
    return (
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-slate-300">Checking sign-in status...</p>
      </section>
    );
  }

  if (!user) {
    return (
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <h1 className="text-3xl font-bold text-white">Sign in required</h1>
        <p className="mt-4 text-sm text-slate-300">
          Sign in before opening the admin smoke test checklist.
        </p>
      </section>
    );
  }

  if (adminStatus && !adminStatus.isAdmin) {
    return (
      <section className="rounded-2xl border border-yellow-300/30 bg-yellow-300/10 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-yellow-100">
          Admin access denied
        </p>

        <h1 className="mt-4 text-3xl font-bold text-white">
          This account is not an admin
        </h1>

        <p className="mt-4 text-sm leading-6 text-yellow-100">
          Role: {adminStatus.role}. Reason: {adminStatus.reason}.
        </p>
      </section>
    );
  }

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Admin smoke test checklist
        </p>

        <h1 className="mt-4 text-3xl font-bold text-white">
          Phase 10 verification checklist
        </h1>

        <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
          Use this page to manually verify admin access, source metadata, templates,
          prompt/disclaimer versioning, audit review, admin audit logging, safety
          boundaries, build checks and Cloud Run deployment.
        </p>

        {adminStatus?.isAdmin && (
          <div className="mt-6 rounded-xl border border-emerald-300/30 bg-emerald-300/10 p-4 text-sm leading-6 text-emerald-100">
            <p>Admin access confirmed.</p>
            <p>Email: {adminStatus.email}</p>
            <p>Role: {adminStatus.role}</p>
          </div>
        )}

        {errorMessage && (
          <div className="mt-6 rounded-xl border border-red-300/30 bg-red-300/10 p-4 text-sm text-red-100">
            {errorMessage}
          </div>
        )}
      </section>

      <section className="grid gap-5 md:grid-cols-2">
        {sections.map((section) => (
          <div
            key={section.title}
            className="rounded-2xl border border-white/10 bg-white/5 p-6"
          >
            <h2 className="text-xl font-bold text-white">{section.title}</h2>

            <div className="mt-5 grid gap-3">
              {section.items.map((item) => (
                <label key={item} className="flex gap-3 text-sm leading-6 text-slate-300">
                  <input
                    type="checkbox"
                    className="mt-1 h-4 w-4 rounded border-white/20 bg-slate-950"
                  />
                  <span>{item}</span>
                </label>
              ))}
            </div>
          </div>
        ))}
      </section>
    </div>
  );
}