"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  AdminKnowledgeAuditEvent,
  AdminKnowledgeAuditEventTypeSummary,
  AdminMeResponse,
  getAdminKnowledgeAuditEvents,
  getAdminMe,
} from "@/lib/api";

export function AdminKnowledgeAuditReviewPanel() {
  const { user, loading, getIdToken } = useAuth();

  const [adminStatus, setAdminStatus] = useState<AdminMeResponse | null>(null);
  const [events, setEvents] = useState<AdminKnowledgeAuditEvent[]>([]);
  const [eventTypeSummary, setEventTypeSummary] = useState<AdminKnowledgeAuditEventTypeSummary[]>([]);
  const [selectedEventId, setSelectedEventId] = useState("");

  const [search, setSearch] = useState("");
  const [eventType, setEventType] = useState("");
  const [workspaceId, setWorkspaceId] = useState("");
  const [userId, setUserId] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [knowledgeOnly, setKnowledgeOnly] = useState(true);

  const [isLoading, setIsLoading] = useState(false);
  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  const selectedEvent = useMemo(
    () => events.find((event) => event.id === selectedEventId) ?? events[0] ?? null,
    [events, selectedEventId],
  );

  const summary = useMemo(
    () => ({
      total: events.length,
      aiEvents: events.filter((event) => event.eventType.startsWith("AI_")).length,
      documentEvents: events.filter(
        (event) =>
          event.eventType.includes("DOCUMENT") ||
          event.eventType.includes("CLAIM_STARTER_PACK") ||
          event.eventType.includes("DOCTOR_GUIDANCE_PACK"),
      ).length,
      sourceEvents: events.filter(
        (event) =>
          event.eventType.includes("SOURCE") ||
          event.eventType.includes("TEMPLATE") ||
          event.eventType.includes("PROMPT") ||
          event.eventType.includes("DISCLAIMER") ||
          event.eventType.includes("KNOWLEDGE"),
      ).length,
    }),
    [events],
  );

  async function getTokenOrSetError() {
    const token = await getIdToken();

    if (!token) {
      setErrorMessage("No Firebase ID token is available. Please sign in again.");
      return null;
    }

    return token;
  }

  async function loadAdminStatus() {
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
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

  async function loadEvents() {
    setIsLoading(true);
    setStatusMessage("");
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const response = await getAdminKnowledgeAuditEvents(token, {
        search,
        eventType,
        workspaceId,
        userId,
        from: from ? new Date(from).toISOString() : "",
        to: to ? new Date(to).toISOString() : "",
        knowledgeOnly: knowledgeOnly ? "true" : "false",
      });

      setEvents(response.rows);
      setEventTypeSummary(response.eventTypeSummary);

      if (response.rows.length > 0) {
        setSelectedEventId(response.rows[0].id);
      } else {
        setSelectedEventId("");
      }

      setStatusMessage(`Loaded ${response.totalReturned} audit event${response.totalReturned === 1 ? "" : "s"}.`);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load knowledge base audit events.";
      setErrorMessage(message);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    if (!loading && user) {
      loadAdminStatus();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user]);

  useEffect(() => {
    if (adminStatus?.isAdmin) {
      loadEvents();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [adminStatus?.isAdmin]);

  async function handleFilterSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await loadEvents();
  }

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
        <p className="mt-4 text-sm text-slate-300">Sign in before opening admin audit tools.</p>
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
          Knowledge base audit review
        </p>

        <h1 className="mt-4 text-3xl font-bold text-white">
          Audit event review
        </h1>

        <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
          Review AI, RAG, source, template, prompt, disclaimer and generated document audit events.
          This view reads the existing application audit trail and is admin-only.
        </p>

        <div className="mt-6 grid gap-4 md:grid-cols-4">
          <SummaryCard label="Loaded" value={summary.total} />
          <SummaryCard label="AI events" value={summary.aiEvents} />
          <SummaryCard label="Document events" value={summary.documentEvents} />
          <SummaryCard label="Source/template events" value={summary.sourceEvents} />
        </div>

        {statusMessage && (
          <div className="mt-6 rounded-xl border border-emerald-300/30 bg-emerald-300/10 p-4 text-sm text-emerald-100">
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
          Filters
        </p>

        <form onSubmit={handleFilterSubmit} className="mt-6 grid gap-4 md:grid-cols-3">
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search event type, detail, IP or client"
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
          />

          <input
            value={eventType}
            onChange={(event) => setEventType(event.target.value)}
            placeholder="Exact event type"
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
          />

          <input
            value={workspaceId}
            onChange={(event) => setWorkspaceId(event.target.value)}
            placeholder="Workspace ID"
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
          />

          <input
            value={userId}
            onChange={(event) => setUserId(event.target.value)}
            placeholder="User ID"
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
          />

          <input
            type="datetime-local"
            value={from}
            onChange={(event) => setFrom(event.target.value)}
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
          />

          <input
            type="datetime-local"
            value={to}
            onChange={(event) => setTo(event.target.value)}
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
          />

          <label className="flex items-center gap-3 rounded-xl border border-white/10 bg-slate-950 p-4 text-sm text-slate-200 md:col-span-3">
            <input
              type="checkbox"
              checked={knowledgeOnly}
              onChange={(event) => setKnowledgeOnly(event.target.checked)}
              className="h-4 w-4"
            />
            Show knowledge/admin-relevant event types only
          </label>

          <button
            type="submit"
            disabled={isLoading}
            className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60 md:col-span-3"
          >
            {isLoading ? "Loading..." : "Apply filters"}
          </button>
        </form>
      </section>

      <section className="grid gap-8 xl:grid-cols-[1fr_1.1fr]">
        <div className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Event type summary
          </p>

          {eventTypeSummary.length === 0 ? (
            <p className="mt-6 text-sm text-slate-300">
              No event type summary is available for the current filter.
            </p>
          ) : (
            <div className="mt-6 grid gap-3">
              {eventTypeSummary.map((item) => (
                <button
                  key={item.eventType}
                  type="button"
                  onClick={() => setEventType(item.eventType)}
                  className="rounded-xl border border-white/10 bg-slate-900 p-4 text-left transition hover:border-cyan-300/60 hover:bg-cyan-300/10"
                >
                  <p className="font-mono text-xs text-cyan-200">{item.eventType}</p>
                  <p className="mt-2 text-sm text-slate-300">{item.count} event{item.count === 1 ? "" : "s"}</p>
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Selected event
          </p>

          {!selectedEvent ? (
            <p className="mt-6 text-sm text-slate-300">
              Select an event to review its details.
            </p>
          ) : (
            <div className="mt-6 rounded-xl border border-white/10 bg-slate-950 p-5 text-sm leading-6 text-slate-300">
              <p><span className="text-slate-500">Event ID:</span> {selectedEvent.id}</p>
              <p><span className="text-slate-500">Event type:</span> {selectedEvent.eventType}</p>
              <p><span className="text-slate-500">Created:</span> {formatDate(selectedEvent.createdAt)}</p>
              <p><span className="text-slate-500">User ID:</span> {selectedEvent.userId}</p>
              <p><span className="text-slate-500">Workspace ID:</span> {selectedEvent.claimWorkspaceId}</p>
              <p><span className="text-slate-500">IP address:</span> {selectedEvent.ipAddress || "Not recorded"}</p>
              <p className="mt-4 text-slate-500">Event detail:</p>
              <pre className="mt-2 whitespace-pre-wrap rounded-xl border border-white/10 bg-black/30 p-4 text-xs text-slate-200">
                {selectedEvent.eventDetail || "No detail recorded."}
              </pre>
              <p className="mt-4 text-slate-500">Client:</p>
              <pre className="mt-2 whitespace-pre-wrap rounded-xl border border-white/10 bg-black/30 p-4 text-xs text-slate-200">
                {selectedEvent.clientType || "No client recorded."}
              </pre>
            </div>
          )}
        </div>
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Audit events
        </p>

        {events.length === 0 ? (
          <p className="mt-6 text-sm text-slate-300">
            No audit events match the current filter.
          </p>
        ) : (
          <div className="mt-6 grid gap-4">
            {events.map((event) => (
              <button
                key={event.id}
                type="button"
                onClick={() => setSelectedEventId(event.id)}
                className={`rounded-xl border p-5 text-left transition ${
                  selectedEvent?.id === event.id
                    ? "border-cyan-300 bg-cyan-300/10"
                    : "border-white/10 bg-slate-900 hover:border-cyan-300/60"
                }`}
              >
                <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                  <div>
                    <p className="font-mono text-xs text-cyan-200">{event.eventType}</p>
                    <h2 className="mt-2 text-base font-bold text-white">
                      {formatAuditEventType(event.eventType)}
                    </h2>
                  </div>

                  <span className="rounded-full border border-white/10 px-3 py-1 text-xs text-slate-300">
                    {formatDate(event.createdAt)}
                  </span>
                </div>

                <p className="mt-4 line-clamp-2 text-sm leading-6 text-slate-300">
                  {event.eventDetail || "No event detail recorded."}
                </p>
              </button>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}

function SummaryCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl border border-white/10 bg-slate-900 p-4">
      <p className="text-xs uppercase tracking-[0.2em] text-slate-400">{label}</p>
      <p className="mt-2 text-2xl font-bold text-white">{value}</p>
    </div>
  );
}

function formatAuditEventType(eventType: string) {
  return eventType
    .toLowerCase()
    .split("_")
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(" ");
}

function formatDate(value: string) {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleString();
}