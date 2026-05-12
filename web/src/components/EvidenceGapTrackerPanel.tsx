"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  ClaimCondition,
  EvidenceGap,
  getClaimConditions,
  getConditionEvidenceGaps,
  getWorkspaceEvidenceGaps,
  recalculateEvidenceGaps,
  updateEvidenceGap,
} from "@/lib/api";

type EvidenceGapTrackerPanelProps = {
  workspaceId: string;
};

const gapStatusOptions = [
  "OPEN",
  "IN_PROGRESS",
  "RESOLVED",
  "USER_MARKED_NOT_APPLICABLE",
];

const severityOrder: Record<string, number> = {
  HIGH: 3,
  MEDIUM: 2,
  LOW: 1,
};

export function EvidenceGapTrackerPanel({ workspaceId }: EvidenceGapTrackerPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [selectedConditionId, setSelectedConditionId] = useState("");
  const [workspaceGaps, setWorkspaceGaps] = useState<EvidenceGap[]>([]);
  const [conditionGaps, setConditionGaps] = useState<EvidenceGap[]>([]);

  const [isLoadingConditions, setIsLoadingConditions] = useState(false);
  const [isLoadingGaps, setIsLoadingGaps] = useState(false);
  const [isRecalculating, setIsRecalculating] = useState(false);
  const [updatingGapId, setUpdatingGapId] = useState("");

  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  const selectedCondition = conditions.find(
    (condition) => condition.id === selectedConditionId,
  );

  const summary = useMemo(() => {
    const activeGaps = workspaceGaps.filter((gap) => gap.status !== "ARCHIVED");

    return {
      total: activeGaps.length,
      high: activeGaps.filter((gap) => gap.severity === "HIGH").length,
      medium: activeGaps.filter((gap) => gap.severity === "MEDIUM").length,
      low: activeGaps.filter((gap) => gap.severity === "LOW").length,
      open: activeGaps.filter((gap) => gap.gapStatus === "OPEN").length,
      resolved: activeGaps.filter((gap) => gap.gapStatus === "RESOLVED").length,
    };
  }, [workspaceGaps]);

  async function getTokenOrSetError() {
    const token = await getIdToken();

    if (!token) {
      setErrorMessage("No Firebase ID token is available. Please sign in again.");
      return null;
    }

    return token;
  }

  async function loadConditions() {
    if (loading || !user) {
      return;
    }

    setIsLoadingConditions(true);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = await getClaimConditions(token, workspaceId);
      setConditions(rows);

      if (!selectedConditionId && rows.length > 0) {
        setSelectedConditionId(rows[0].id);
      }
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load conditions.";
      setErrorMessage(message);
    } finally {
      setIsLoadingConditions(false);
    }
  }

  async function loadWorkspaceGaps() {
    if (loading || !user) {
      return;
    }

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = await getWorkspaceEvidenceGaps(token, workspaceId);
      setWorkspaceGaps(sortGaps(rows));
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load workspace evidence gaps.";
      setErrorMessage(message);
    }
  }

  async function loadConditionGaps(conditionId: string) {
    if (loading || !user || !conditionId) {
      return;
    }

    setIsLoadingGaps(true);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = await getConditionEvidenceGaps(token, workspaceId, conditionId);
      setConditionGaps(sortGaps(rows));
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load condition evidence gaps.";
      setErrorMessage(message);
    } finally {
      setIsLoadingGaps(false);
    }
  }

  useEffect(() => {
    loadConditions();
    loadWorkspaceGaps();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  useEffect(() => {
    if (selectedConditionId) {
      loadConditionGaps(selectedConditionId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedConditionId]);

  async function handleRecalculate() {
    setStatusMessage("");
    setErrorMessage("");
    setIsRecalculating(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      if (!selectedConditionId) {
        setErrorMessage("Select a condition before recalculating evidence gaps.");
        return;
      }

      const result = await recalculateEvidenceGaps(
        token,
        workspaceId,
        selectedConditionId,
      );

      setStatusMessage(`Evidence gaps recalculated. ${result.createdCount} gap(s) created.`);
      await loadConditionGaps(selectedConditionId);
      await loadWorkspaceGaps();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not recalculate evidence gaps.";
      setErrorMessage(message);
    } finally {
      setIsRecalculating(false);
    }
  }

  async function handleStatusChange(gap: EvidenceGap, nextStatus: string) {
    setStatusMessage("");
    setErrorMessage("");
    setUpdatingGapId(gap.id);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      await updateEvidenceGap(token, workspaceId, gap.conditionId, gap.id, {
        gapStatus: nextStatus,
      });

      setStatusMessage("Evidence gap status updated.");
      await loadConditionGaps(selectedConditionId);
      await loadWorkspaceGaps();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not update evidence gap.";
      setErrorMessage(message);
    } finally {
      setUpdatingGapId("");
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
          Sign in before using the evidence gap tracker.
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

  if (isLoadingConditions) {
    return (
      <div className="rounded-2xl border border-white/10 bg-white/5 p-6 text-slate-300">
        Loading conditions...
      </div>
    );
  }

  if (conditions.length === 0) {
    return (
      <div className="rounded-2xl border border-yellow-300/30 bg-yellow-300/10 p-6 text-yellow-100">
        <h2 className="text-xl font-semibold">Add a condition first</h2>
        <p className="mt-2 text-sm">
          Evidence gaps are calculated against a specific condition.
        </p>
        <Link
          href={`/claim-workspaces/${workspaceId}/conditions`}
          className="mt-5 inline-flex rounded-xl bg-cyan-300 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-cyan-200"
        >
          Go to condition intake
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Evidence gap tracker
        </p>

        <h1 className="mt-4 text-3xl font-bold">Review evidence gaps</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          Recalculate plain-English evidence preparation prompts based on the selected condition,
          accepted-condition history and listed evidence.
        </p>

        <div className="mt-8 grid gap-4 md:grid-cols-5">
          <SummaryCard label="Total gaps" value={summary.total} />
          <SummaryCard label="High" value={summary.high} />
          <SummaryCard label="Medium" value={summary.medium} />
          <SummaryCard label="Low" value={summary.low} />
          <SummaryCard label="Open" value={summary.open} />
        </div>

        <div className="mt-8">
          <label htmlFor="condition" className="text-sm font-medium text-slate-200">
            Condition
          </label>

          <select
            id="condition"
            value={selectedConditionId}
            onChange={(event) => setSelectedConditionId(event.target.value)}
            className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
          >
            {conditions.map((condition) => (
              <option key={condition.id} value={condition.id}>
                {condition.conditionName}
              </option>
            ))}
          </select>
        </div>

        <button
          type="button"
          onClick={handleRecalculate}
          disabled={isRecalculating}
          className="mt-6 w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
        >
          {isRecalculating ? "Recalculating gaps..." : "Recalculate evidence gaps"}
        </button>

        {statusMessage && (
          <div className="mt-6 rounded-xl border border-green-300/30 bg-green-300/10 p-4 text-sm text-green-100">
            {statusMessage}
          </div>
        )}

        {errorMessage && (
          <div className="mt-6 rounded-xl border border-red-300/30 bg-red-300/10 p-4 text-sm text-red-100">
            {errorMessage}
          </div>
        )}
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Condition gaps
        </p>

        <h2 className="mt-4 text-2xl font-bold">
          {selectedCondition?.conditionName ?? "Selected condition"}
        </h2>

        {isLoadingGaps ? (
          <p className="mt-6 text-slate-300">Loading evidence gaps...</p>
        ) : conditionGaps.length === 0 ? (
          <div className="mt-6 rounded-xl border border-white/10 bg-slate-900 p-5 text-slate-300">
            No evidence gaps have been calculated for this condition yet. Use the recalculate
            button above to create preparation prompts.
          </div>
        ) : (
          <div className="mt-6 grid gap-4">
            {conditionGaps.map((gap) => (
              <article
                key={gap.id}
                className="rounded-xl border border-white/10 bg-slate-900 p-5"
              >
                <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                  <div>
                    <div className="flex flex-wrap gap-2">
                      <span className="rounded-full border border-cyan-300/30 bg-cyan-300/10 px-3 py-1 text-xs text-cyan-100">
                        {gap.gapType}
                      </span>

                      <span className="rounded-full border border-white/10 px-3 py-1 text-xs text-slate-300">
                        {gap.severity}
                      </span>

                      <span className="rounded-full border border-white/10 px-3 py-1 text-xs text-slate-300">
                        {gap.gapStatus}
                      </span>
                    </div>

                    <p className="mt-4 text-sm leading-6 text-slate-300">
                      {gap.plainEnglishExplanation}
                    </p>

                    {gap.suggestedNextStep && (
                      <p className="mt-4 text-sm leading-6 text-slate-400">
                        Suggested next step: {gap.suggestedNextStep}
                      </p>
                    )}
                  </div>

                  <div className="min-w-56">
                    <label
                      htmlFor={`gap-status-${gap.id}`}
                      className="text-xs font-medium text-slate-400"
                    >
                      Gap status
                    </label>

                    <select
                      id={`gap-status-${gap.id}`}
                      value={gap.gapStatus}
                      disabled={updatingGapId === gap.id}
                      onChange={(event) => handleStatusChange(gap, event.target.value)}
                      className="mt-2 w-full rounded-xl border border-white/10 bg-slate-950 px-3 py-2 text-sm text-white outline-none focus:border-cyan-300"
                    >
                      {gapStatusOptions.map((status) => (
                        <option key={status} value={status}>
                          {status}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. Evidence gaps are plain-English prompts to help organise
        evidence. They do not tell the user what DVA will require, provide legal advice,
        provide medical advice, estimate compensation, or guarantee claim success.
      </section>
    </div>
  );
}

function sortGaps(gaps: EvidenceGap[]) {
  return [...gaps].sort((a, b) => {
    const severityDifference =
      (severityOrder[b.severity] ?? 0) - (severityOrder[a.severity] ?? 0);

    if (severityDifference !== 0) {
      return severityDifference;
    }

    return a.gapType.localeCompare(b.gapType);
  });
}

function SummaryCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl border border-white/10 bg-slate-900 p-5">
      <p className="text-sm text-slate-400">{label}</p>
      <p className="mt-2 text-2xl font-bold text-white">{value}</p>
    </div>
  );
}