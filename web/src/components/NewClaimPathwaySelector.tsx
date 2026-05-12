"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/AuthProvider";
import { createClaimWorkspace } from "@/lib/apiClient";

const claimScenarios = [
  {
    value: "NEW_CONDITION",
    title: "New condition",
    description:
      "Use this when preparing information for a condition that has not previously been accepted.",
  },
  {
    value: "WORSENING_EXISTING_CONDITION",
    title: "Worsening existing condition",
    description:
      "Use this when an existing accepted condition may have worsened and needs evidence organised.",
  },
  {
    value: "NEW_PLUS_EXISTING",
    title: "New plus existing conditions",
    description:
      "Use this when the preparation pack may involve both new and previously accepted conditions.",
  },
  {
    value: "EVIDENCE_PREP_ONLY",
    title: "Evidence preparation only",
    description:
      "Use this when the main goal is organising documents, notes, gaps and questions before speaking with support.",
  },
  {
    value: "UNSURE",
    title: "Not sure yet",
    description:
      "Use this when the pathway is unclear and the workspace should stay flexible.",
  },
];

export function NewClaimPathwaySelector() {
  const router = useRouter();
  const { user, loading, getIdToken } = useAuth();

  const [workspaceTitle, setWorkspaceTitle] = useState(
    "Post-2026 PI Claim Starter Pack",
  );
  const [claimScenario, setClaimScenario] = useState("UNSURE");
  const [statusMessage, setStatusMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setStatusMessage("");
    setIsSubmitting(true);

    try {
      const token = await getIdToken();

      if (!token) {
        setStatusMessage("You need to sign in before creating a claim workspace.");
        return;
      }

      const workspace = await createClaimWorkspace(token, {
        workspaceTitle,
        claimScenario,
      });

      router.push(`/claim-workspaces/${workspace.id}`);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Could not create the claim workspace.";

      setStatusMessage(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  if (loading) {
    return (
      <div className="rounded-2xl border border-white/10 bg-white/5 p-6 text-slate-300">
        Checking session...
      </div>
    );
  }

  if (!user) {
    return (
      <div className="rounded-2xl border border-yellow-300/30 bg-yellow-300/10 p-6 text-yellow-100">
        <h2 className="text-xl font-semibold">Sign in required</h2>
        <p className="mt-2 text-sm">
          You need to sign in before creating a claim preparation workspace.
        </p>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-8">
      <div>
        <label htmlFor="workspaceTitle" className="text-sm font-medium text-slate-200">
          Workspace title
        </label>

        <input
          id="workspaceTitle"
          type="text"
          value={workspaceTitle}
          onChange={(event) => setWorkspaceTitle(event.target.value)}
          required
          className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
        />
      </div>

      <div>
        <p className="text-sm font-medium text-slate-200">Claim preparation pathway</p>

        <div className="mt-4 grid gap-4">
          {claimScenarios.map((scenario) => (
            <label
              key={scenario.value}
              className={
                claimScenario === scenario.value
                  ? "cursor-pointer rounded-2xl border border-cyan-300 bg-cyan-300/10 p-5"
                  : "cursor-pointer rounded-2xl border border-white/10 bg-slate-900 p-5 hover:bg-white/5"
              }
            >
              <div className="flex gap-3">
                <input
                  type="radio"
                  name="claimScenario"
                  value={scenario.value}
                  checked={claimScenario === scenario.value}
                  onChange={(event) => setClaimScenario(event.target.value)}
                  className="mt-1"
                />

                <div>
                  <p className="font-semibold text-white">{scenario.title}</p>
                  <p className="mt-2 text-sm leading-6 text-slate-300">
                    {scenario.description}
                  </p>
                  <p className="mt-2 font-mono text-xs text-cyan-200">
                    {scenario.value}
                  </p>
                </div>
              </div>
            </label>
          ))}
        </div>
      </div>

      {statusMessage && (
        <div className="rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm text-yellow-100">
          {statusMessage}
        </div>
      )}

      <button
        type="submit"
        disabled={isSubmitting}
        className="w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
      >
        {isSubmitting ? "Creating workspace..." : "Create claim workspace"}
      </button>

      <p className="text-sm leading-6 text-slate-400">
        This creates a preparation workspace only. It does not submit anything to DVA,
        provide legal advice, provide medical advice, estimate compensation, or guarantee
        any outcome.
      </p>
    </form>
  );
}