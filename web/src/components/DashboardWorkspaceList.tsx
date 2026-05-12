"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import { ClaimWorkspace, getClaimWorkspaces } from "@/lib/api";

export function DashboardWorkspaceList() {
  const { user, loading, getIdToken } = useAuth();
  const [workspaces, setWorkspaces] = useState<ClaimWorkspace[]>([]);
  const [isLoadingWorkspaces, setIsLoadingWorkspaces] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    async function loadWorkspaces() {
      if (loading) {
        return;
      }

      if (!user) {
        setWorkspaces([]);
        setErrorMessage("");
        return;
      }

      setIsLoadingWorkspaces(true);
      setErrorMessage("");

      try {
        const token = await getIdToken();

        if (!token) {
          setErrorMessage("No Firebase ID token is available. Please sign in again.");
          return;
        }

        const rows = await getClaimWorkspaces(token);
        setWorkspaces(rows);
      } catch (error) {
        const message =
          error instanceof Error ? error.message : "Could not load claim workspaces.";
        setErrorMessage(message);
      } finally {
        setIsLoadingWorkspaces(false);
      }
    }

    loadWorkspaces();
  }, [getIdToken, loading, user]);

  if (loading || isLoadingWorkspaces) {
    return (
      <div className="rounded-2xl border border-white/10 bg-white/5 p-6 text-slate-300">
        Loading claim workspaces...
      </div>
    );
  }

  if (!user) {
    return (
      <div className="rounded-2xl border border-yellow-300/30 bg-yellow-300/10 p-6 text-yellow-100">
        <h2 className="text-lg font-semibold">Sign in required</h2>
        <p className="mt-2 text-sm">
          Sign in before loading your claim workspaces from the backend.
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
        <h2 className="text-lg font-semibold">Could not load workspaces</h2>
        <p className="mt-2 text-sm">{errorMessage}</p>
      </div>
    );
  }

  if (workspaces.length === 0) {
    return (
      <div className="rounded-2xl border border-white/10 bg-white/5 p-6">
        <h2 className="text-xl font-semibold">No claim workspaces yet</h2>
        <p className="mt-2 text-sm text-slate-300">
          Create your first claim workspace to start organising conditions and evidence.
        </p>
        <Link
          href="/claim-workspaces/new"
          className="mt-5 inline-flex rounded-xl bg-cyan-300 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-cyan-200"
        >
          Create workspace
        </Link>
      </div>
    );
  }

  return (
    <div className="grid gap-4">
      {workspaces.map((workspace) => (
        <Link
          key={workspace.id}
          href={`/claim-workspaces/${workspace.id}`}
          className="rounded-2xl border border-white/10 bg-white/5 p-6 hover:bg-white/10"
        >
          <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <h2 className="text-xl font-semibold">{workspace.workspaceTitle}</h2>
              <p className="mt-2 text-sm text-slate-300">
                Scenario: {workspace.claimScenario}
              </p>
              <p className="mt-1 text-sm text-slate-400">
                Framework: {workspace.claimFramework}
              </p>
            </div>

            <div className="rounded-xl border border-cyan-300/30 bg-cyan-300/10 px-3 py-2 text-sm text-cyan-100">
              {workspace.status}
            </div>
          </div>

          <p className="mt-4 text-xs text-slate-500">
            Generated pack: {workspace.generatedPackStatus}
          </p>
        </Link>
      ))}
    </div>
  );
}