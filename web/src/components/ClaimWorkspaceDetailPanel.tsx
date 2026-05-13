"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import { ClaimWorkspace, getClaimWorkspace } from "@/lib/api";

type ClaimWorkspaceDetailPanelProps = {
  workspaceId: string;
};


export function ClaimWorkspaceDetailPanel({ workspaceId }: ClaimWorkspaceDetailPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [workspace, setWorkspace] = useState<ClaimWorkspace | null>(null);
  const [isLoadingWorkspace, setIsLoadingWorkspace] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    async function loadWorkspace() {
      if (loading) {
        return;
      }

      if (!user) {
        setWorkspace(null);
        setErrorMessage("");
        return;
      }

      setIsLoadingWorkspace(true);
      setErrorMessage("");

      try {
        const token = await getIdToken();

        if (!token) {
          setErrorMessage("No Firebase ID token is available. Please sign in again.");
          return;
        }

        const row = await getClaimWorkspace(token, workspaceId);
        setWorkspace(row);
      } catch (error) {
        const message =
          error instanceof Error ? error.message : "Could not load claim workspace.";

        setErrorMessage(message);
      } finally {
        setIsLoadingWorkspace(false);
      }
    }

    loadWorkspace();
  }, [getIdToken, loading, user, workspaceId]);

  if (loading || isLoadingWorkspace) {
    return (
      <div className="rounded-2xl border border-white/10 bg-white/5 p-6 text-slate-300">
        Loading claim workspace...
      </div>
    );
  }

  if (!user) {
    return (
      <div className="rounded-2xl border border-yellow-300/30 bg-yellow-300/10 p-6 text-yellow-100">
        <h2 className="text-xl font-semibold">Sign in required</h2>
        <p className="mt-2 text-sm">
          Sign in before opening this claim preparation workspace.
        </p>
        <Link
          href="/login"
          className="mt-5 inline-flex rounded-xl bg-cyan-300 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-cyan-200"
        >
          Go to login
        </Link>
      </div>
    );
  }

  if (errorMessage) {
    return (
      <div className="rounded-2xl border border-red-300/30 bg-red-300/10 p-6 text-red-100">
        <h2 className="text-xl font-semibold">Could not load workspace</h2>
        <p className="mt-2 text-sm">{errorMessage}</p>
        <Link
          href="/dashboard"
          className="mt-5 inline-flex rounded-xl bg-white px-4 py-2 text-sm font-semibold text-slate-950"
        >
          Back to dashboard
        </Link>
      </div>
    );
  }

  if (!workspace) {
    return (
      <div className="rounded-2xl border border-white/10 bg-white/5 p-6 text-slate-300">
        No workspace loaded.
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Claim workspace
        </p>

        <div className="mt-4 flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <h1 className="text-3xl font-bold">{workspace.workspaceTitle}</h1>

            <p className="mt-4 max-w-3xl text-slate-300">
              This is a preparation workspace for organising claim information, conditions,
              evidence, gaps and draft documents.
            </p>
          </div>

          <div className="rounded-xl border border-cyan-300/30 bg-cyan-300/10 px-4 py-3 text-sm text-cyan-100">
            {workspace.status}
          </div>
        </div>

        <div className="mt-8 grid gap-4 md:grid-cols-2">
          <div className="rounded-xl border border-white/10 bg-slate-900 p-5">
            <p className="text-sm text-slate-400">Scenario</p>
            <p className="mt-2 font-mono text-cyan-200">{workspace.claimScenario}</p>
          </div>

          <div className="rounded-xl border border-white/10 bg-slate-900 p-5">
            <p className="text-sm text-slate-400">Framework</p>
            <p className="mt-2 font-mono text-cyan-200">{workspace.claimFramework}</p>
          </div>

          <div className="rounded-xl border border-white/10 bg-slate-900 p-5">
            <p className="text-sm text-slate-400">Generated pack status</p>
            <p className="mt-2 font-mono text-cyan-200">{workspace.generatedPackStatus}</p>
          </div>

          <div className="rounded-xl border border-white/10 bg-slate-900 p-5">
            <p className="text-sm text-slate-400">Workspace ID</p>
            <p className="mt-2 break-all font-mono text-cyan-200">{workspace.id}</p>
          </div>
        </div>
      </section>

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. This workspace does not create a DVA claim, submit material
        to DVA, provide legal advice, provide medical advice, estimate compensation, or guarantee
        a claim outcome.
      </section>
    </div>
  );
}