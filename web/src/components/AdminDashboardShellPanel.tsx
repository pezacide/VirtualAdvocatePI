"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import { AdminMeResponse, getAdminMe } from "@/lib/api";

const adminTools = [
  {
    title: "Source metadata manager",
    href: "/admin/source-metadata",
    description: "Review and manage approved source registry metadata.",
    status: "Next",
  },
  {
    title: "Question templates",
    href: "/admin/templates/questions",
    description: "Manage guided question and intake template structures.",
    status: "Planned",
  },
  {
    title: "Document templates",
    href: "/admin/templates/documents",
    description: "Manage Claim Starter Pack and Doctor Guidance Pack template versions.",
    status: "Planned",
  },
  {
    title: "Prompts and disclaimers",
    href: "/admin/prompts-disclaimers",
    description: "Version prompt templates, safety wording and disclaimer text.",
    status: "Planned",
  },
  {
    title: "Knowledge base audit review",
    href: "/admin/knowledge-audit",
    description: "Review source, template and knowledge-base admin activity.",
    status: "Planned",
  },
  {
    title: "Admin access check",
    href: "/admin/access-check",
    description: "Confirm current account role and protected admin endpoint access.",
    status: "Active",
  },
];

export function AdminDashboardShellPanel() {
  const { user, loading, getIdToken } = useAuth();

  const [adminStatus, setAdminStatus] = useState<AdminMeResponse | null>(null);
  const [errorMessage, setErrorMessage] = useState("");
  const [isChecking, setIsChecking] = useState(false);

  async function checkAdminAccess() {
    setIsChecking(true);
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
    } finally {
      setIsChecking(false);
    }
  }

  useEffect(() => {
    if (!loading && user) {
      checkAdminAccess();
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
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Admin
        </p>

        <h1 className="mt-4 text-3xl font-bold text-white">
          Sign in required
        </h1>

        <p className="mt-4 text-sm leading-6 text-slate-300">
          Sign in before opening admin tools.
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

        <div className="mt-6 rounded-xl border border-white/10 bg-slate-950 p-5 text-sm leading-6 text-yellow-100">
          <p>Email: {adminStatus.email}</p>
          <p>Role: {adminStatus.role}</p>
          <p>Account status: {adminStatus.accountStatus}</p>
          <p>Reason: {adminStatus.reason}</p>
        </div>

        <Link
          href="/dashboard"
          className="mt-6 inline-flex rounded-xl border border-cyan-300/40 px-5 py-3 text-sm font-semibold text-cyan-100 hover:bg-cyan-300/10"
        >
          Back to dashboard
        </Link>
      </section>
    );
  }

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Admin
        </p>

        <h1 className="mt-4 text-3xl font-bold text-white">
          Knowledge and template manager
        </h1>

        <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
          Manage approved source metadata, question templates, document templates,
          prompt versions, disclaimers and knowledge-base review workflows. Admin
          tools are protected and should be used carefully.
        </p>

        <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm font-semibold leading-6 text-yellow-100">
          Admin actions must be deliberate, reviewable and auditable. Admin tools
          must not bypass veteran workspace ownership checks or silently alter generated
          claim content.
        </div>

        {adminStatus && (
          <div className="mt-6 rounded-xl border border-emerald-300/30 bg-emerald-300/10 p-4 text-sm leading-6 text-emerald-100">
            <p>Signed in as: {adminStatus.email}</p>
            <p>Role: {adminStatus.role}</p>
            <p>Admin access: {adminStatus.isAdmin ? "Granted" : "Denied"}</p>
          </div>
        )}

        {errorMessage && (
          <div className="mt-6 rounded-xl border border-red-300/30 bg-red-300/10 p-4 text-sm text-red-100">
            {errorMessage}
          </div>
        )}

        <button
          type="button"
          onClick={checkAdminAccess}
          disabled={isChecking}
          className="mt-6 rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-cyan-200 disabled:opacity-60"
        >
          {isChecking ? "Checking..." : "Refresh admin status"}
        </button>
      </section>

      {adminStatus?.isAdmin && (
        <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Admin tools
          </p>

          <div className="mt-6 grid gap-5 md:grid-cols-2 xl:grid-cols-3">
            {adminTools.map((tool) => (
              <Link
                key={tool.href}
                href={tool.href}
                className="group rounded-2xl border border-white/10 bg-slate-900 p-6 transition hover:border-cyan-300/70 hover:bg-cyan-300/10"
              >
                <div className="flex items-start justify-between gap-4">
                  <h2 className="text-lg font-bold text-white">{tool.title}</h2>

                  <span className="rounded-full border border-cyan-300/40 bg-cyan-300/10 px-3 py-1 text-xs font-semibold text-cyan-100">
                    {tool.status}
                  </span>
                </div>

                <p className="mt-4 text-sm leading-6 text-slate-300">
                  {tool.description}
                </p>

                <p className="mt-5 text-sm font-semibold text-cyan-300 group-hover:text-cyan-100">
                  Open →
                </p>
              </Link>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}