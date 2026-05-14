"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import { AuditEvent, getWorkspaceAuditEvents } from "@/lib/api";

type EvidenceAuditTrailPanelProps = {
  workspaceId: string;
};

const evidenceEventPrefixes = [
  "EVIDENCE",
  "EVIDENCE_GAP",
  "EVIDENCE_UPLOAD",
];

export function EvidenceAuditTrailPanel({ workspaceId }: EvidenceAuditTrailPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [auditEvents, setAuditEvents] = useState<AuditEvent[]>([]);
  const [showEvidenceOnly, setShowEvidenceOnly] = useState(false);
  const [isLoadingAudit, setIsLoadingAudit] = useState(false);
  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  async function loadAuditEvents() {
    if (loading || !user) {
      return;
    }

    setIsLoadingAudit(true);
    setStatusMessage("");
    setErrorMessage("");

    try {
      const token = await getIdToken();

      if (!token) {
        setErrorMessage("No Firebase ID token is available. Please sign in again.");
        return;
      }

      const rows = await getWorkspaceAuditEvents(token, workspaceId);
      setAuditEvents(rows);
      setStatusMessage(`Loaded ${rows.length} audit event(s).`);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Could not load audit events.";
      setErrorMessage(message);
    } finally {
      setIsLoadingAudit(false);
    }
  }

  useEffect(() => {
    loadAuditEvents();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  const visibleEvents = useMemo(() => {
    if (!showEvidenceOnly) {
      return auditEvents;
    }

    return auditEvents.filter((event) =>
      evidenceEventPrefixes.some((prefix) => event.eventType.startsWith(prefix)),
    );
  }, [auditEvents, showEvidenceOnly]);

  const evidenceEventCount = useMemo(
    () =>
      auditEvents.filter((event) =>
        evidenceEventPrefixes.some((prefix) => event.eventType.startsWith(prefix)),
      ).length,
    [auditEvents],
  );

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
        <p className="mt-2 text-sm">Sign in before viewing the audit trail.</p>
        <Link
          href="/login"
          className="mt-5 inline-flex rounded-xl bg-cyan-300 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-cyan-200"
        >
          Go to login
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Workspace audit trail
        </p>

        <h1 className="mt-4 text-3xl font-bold">Workspace activity log</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          Review recorded workspace activity such as evidence upload URL creation, upload
          confirmation, evidence metadata changes, evidence gap recalculation and related actions.
          This is a preparation activity log only.
        </p>

        <div className="mt-6 grid gap-4 md:grid-cols-3">
          <AuditSummaryCard label="Total events" value={auditEvents.length} />
          <AuditSummaryCard label="Evidence events" value={evidenceEventCount} />
          <AuditSummaryCard label="Visible events" value={visibleEvents.length} />
        </div>

        <div className="mt-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <label className="inline-flex items-center gap-3 text-sm text-slate-300">
            <input
              type="checkbox"
              checked={showEvidenceOnly}
              onChange={(event) => setShowEvidenceOnly(event.target.checked)}
              className="h-4 w-4 rounded border-white/20 bg-slate-900"
            />
            Show evidence-related events only
          </label>

          <button
            type="button"
            onClick={loadAuditEvents}
            disabled={isLoadingAudit}
            className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
          >
            {isLoadingAudit ? "Refreshing audit trail..." : "Refresh audit trail"}
          </button>
        </div>

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
          Audit events
        </p>

        {isLoadingAudit ? (
          <p className="mt-6 text-slate-300">Loading audit trail...</p>
        ) : visibleEvents.length === 0 ? (
          <p className="mt-6 text-slate-300">
            No audit events are currently visible for this filter.
          </p>
        ) : (
          <div className="mt-6 grid gap-4">
            {visibleEvents.map((event) => (
              <article
                key={event.id}
                className="rounded-xl border border-white/10 bg-slate-900 p-5"
              >
                <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <p className="font-mono text-xs text-cyan-200">{event.eventType}</p>
                    <h2 className="mt-2 text-lg font-semibold text-white">
                      {formatAuditEventType(event.eventType)}
                    </h2>
                  </div>

                  <span className="rounded-xl border border-white/10 px-3 py-2 text-xs text-slate-300">
                    {formatDateTime(event.createdAt)}
                  </span>
                </div>

                {event.eventDetail && (
                  <p className="mt-4 text-sm leading-6 text-slate-300">{event.eventDetail}</p>
                )}

                <div className="mt-4 grid gap-2 text-xs text-slate-400 md:grid-cols-2">
                  <p>Audit event ID: {event.id}</p>
                  <p>Workspace ID: {event.claimWorkspaceId || "Not recorded"}</p>
                  <p>IP address: {event.ipAddress || "Not recorded"}</p>
                  <p>Client: {event.clientType || "Not recorded"}</p>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. The audit trail records activity inside this app. It does not
        submit evidence to DVA, confirm DVA has received anything, provide legal advice, provide
        medical advice, make a DVA decision, or guarantee any outcome.
      </section>
    </div>
  );
}

function AuditSummaryCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl border border-white/10 bg-slate-900 p-4">
      <p className="text-xs text-slate-400">{label}</p>
      <p className="mt-2 text-2xl font-bold text-white">{value}</p>
    </div>
  );
}

function formatAuditEventType(eventType: string) {
  return eventType
    .toLowerCase()
    .split("_")
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(" ");
}

function formatDateTime(value: string) {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleString();
}