"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  AdminMeResponse,
  AdminPromptDisclaimerVersionEntry,
  createAdminPromptDisclaimerVersion,
  getAdminMe,
  getAdminPromptDisclaimerVersions,
  updateAdminPromptDisclaimerVersion,
} from "@/lib/api";

const versionTypes = ["", "PROMPT", "DISCLAIMER"];
const approvalStatuses = ["", "DRAFT", "PENDING_REVIEW", "APPROVED", "REJECTED"];
const statusOptions = ["", "ACTIVE", "ARCHIVED"];

export function AdminPromptDisclaimerVersionPanel() {
  const { user, loading, getIdToken } = useAuth();

  const [adminStatus, setAdminStatus] = useState<AdminMeResponse | null>(null);
  const [entries, setEntries] = useState<AdminPromptDisclaimerVersionEntry[]>([]);
  const [selectedEntryId, setSelectedEntryId] = useState("");

  const [search, setSearch] = useState("");
  const [versionTypeFilter, setVersionTypeFilter] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [appliesToFilter, setAppliesToFilter] = useState("");
  const [approvalStatusFilter, setApprovalStatusFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");

  const [versionKey, setVersionKey] = useState("");
  const [versionType, setVersionType] = useState<"PROMPT" | "DISCLAIMER">("PROMPT");
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [category, setCategory] = useState("GENERAL");
  const [versionLabel, setVersionLabel] = useState("v1");
  const [appliesTo, setAppliesTo] = useState("GENERAL");
  const [content, setContent] = useState("");
  const [approvalStatus, setApprovalStatus] = useState("DRAFT");
  const [approvedBy, setApprovedBy] = useState("");
  const [reviewNotes, setReviewNotes] = useState("");
  const [effectiveFrom, setEffectiveFrom] = useState("");
  const [retiredAt, setRetiredAt] = useState("");
  const [isActive, setIsActive] = useState(true);
  const [status, setStatus] = useState("ACTIVE");

  const [isCreateMode, setIsCreateMode] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  const selectedEntry = useMemo(
    () => entries.find((entry) => entry.id === selectedEntryId) ?? null,
    [entries, selectedEntryId],
  );

  const summary = useMemo(
    () => ({
      total: entries.length,
      prompts: entries.filter((entry) => entry.versionType === "PROMPT").length,
      disclaimers: entries.filter((entry) => entry.versionType === "DISCLAIMER").length,
      approved: entries.filter((entry) => entry.approvalStatus === "APPROVED").length,
    }),
    [entries],
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

  async function loadEntries() {
    setIsLoading(true);
    setStatusMessage("");
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = await getAdminPromptDisclaimerVersions(token, {
        search,
        versionType: versionTypeFilter,
        category: categoryFilter,
        appliesTo: appliesToFilter,
        approvalStatus: approvalStatusFilter,
        status: statusFilter,
      });

      setEntries(rows);
      setStatusMessage(`Loaded ${rows.length} prompt/disclaimer version${rows.length === 1 ? "" : "s"}.`);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load prompt/disclaimer versions.";
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
      loadEntries();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [adminStatus?.isAdmin]);

  function startCreateMode(nextType: "PROMPT" | "DISCLAIMER") {
    setIsCreateMode(true);
    setSelectedEntryId("");
    setVersionKey("");
    setVersionType(nextType);
    setTitle("");
    setDescription("");
    setCategory("GENERAL");
    setVersionLabel("v1");
    setAppliesTo("GENERAL");
    setContent("");
    setApprovalStatus("DRAFT");
    setApprovedBy("");
    setReviewNotes("");
    setEffectiveFrom("");
    setRetiredAt("");
    setIsActive(true);
    setStatus("ACTIVE");
    setStatusMessage(`Create ${nextType.toLowerCase()} mode started.`);
  }

  function selectEntry(entry: AdminPromptDisclaimerVersionEntry) {
    setIsCreateMode(false);
    setSelectedEntryId(entry.id);
    setVersionKey(entry.versionKey);
    setVersionType(entry.versionType);
    setTitle(entry.title);
    setDescription(entry.description);
    setCategory(entry.category);
    setVersionLabel(entry.versionLabel);
    setAppliesTo(entry.appliesTo);
    setContent(entry.content);
    setApprovalStatus(entry.approvalStatus);
    setApprovedBy(entry.approvedBy ?? "");
    setReviewNotes(entry.reviewNotes ?? "");
    setEffectiveFrom(toDateTimeLocal(entry.effectiveFrom));
    setRetiredAt(toDateTimeLocal(entry.retiredAt));
    setIsActive(entry.isActive);
    setStatus(entry.status);
    setStatusMessage(`Selected ${entry.versionKey}.`);
  }

  async function handleFilterSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await loadEntries();
  }

  async function handleSave(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setIsSaving(true);
    setStatusMessage("");
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const effectiveFromValue = effectiveFrom ? new Date(effectiveFrom).toISOString() : null;
      const retiredAtValue = retiredAt ? new Date(retiredAt).toISOString() : null;

      if (isCreateMode) {
        const created = await createAdminPromptDisclaimerVersion(token, {
          versionKey,
          versionType,
          title,
          description,
          category,
          versionLabel,
          appliesTo,
          content,
          approvalStatus,
          approvedBy,
          reviewNotes,
          effectiveFrom: effectiveFromValue,
          retiredAt: retiredAtValue,
          isActive,
          status,
        });

        setEntries((current) => [created, ...current]);
        selectEntry(created);
        setStatusMessage(`Created ${created.versionKey}.`);
      } else {
        if (!selectedEntry) {
          setErrorMessage("Select a prompt/disclaimer version before saving.");
          return;
        }

        const updated = await updateAdminPromptDisclaimerVersion(token, selectedEntry.id, {
          title,
          description,
          category,
          versionLabel,
          appliesTo,
          content,
          approvalStatus,
          approvedBy,
          reviewNotes,
          effectiveFrom: effectiveFromValue,
          effectiveFromSet: true,
          retiredAt: retiredAtValue,
          retiredAtSet: true,
          isActive,
          status,
        });

        setEntries((current) =>
          current.map((entry) => (entry.id === updated.id ? updated : entry)),
        );

        selectEntry(updated);
        setStatusMessage(`Saved ${updated.versionKey}.`);
      }
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not save prompt/disclaimer version.";
      setErrorMessage(message);
    } finally {
      setIsSaving(false);
    }
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
        <p className="mt-4 text-sm text-slate-300">Sign in before opening admin tools.</p>
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
          Prompt and disclaimer versioning
        </p>

        <h1 className="mt-4 text-3xl font-bold text-white">
          Controlled wording registry
        </h1>

        <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
          Create and manage prompt versions, disclaimer versions, safety wording and
          workflow-specific controlled text. This foundation does not yet wire approved
          versions into generation workflows.
        </p>

        <div className="mt-6 grid gap-4 md:grid-cols-4">
          <SummaryCard label="Loaded" value={summary.total} />
          <SummaryCard label="Prompts" value={summary.prompts} />
          <SummaryCard label="Disclaimers" value={summary.disclaimers} />
          <SummaryCard label="Approved" value={summary.approved} />
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

        <form onSubmit={handleFilterSubmit} className="mt-6 grid gap-4 md:grid-cols-6">
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search versions"
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
          />

          <select
            value={versionTypeFilter}
            onChange={(event) => setVersionTypeFilter(event.target.value)}
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
          >
            {versionTypes.map((value) => (
              <option key={value || "any"} value={value}>
                {value || "Any type"}
              </option>
            ))}
          </select>

          <input
            value={categoryFilter}
            onChange={(event) => setCategoryFilter(event.target.value)}
            placeholder="Category"
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
          />

          <input
            value={appliesToFilter}
            onChange={(event) => setAppliesToFilter(event.target.value)}
            placeholder="Applies to"
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
          />

          <select
            value={approvalStatusFilter}
            onChange={(event) => setApprovalStatusFilter(event.target.value)}
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
          >
            {approvalStatuses.map((value) => (
              <option key={value || "any"} value={value}>
                {value || "Any approval"}
              </option>
            ))}
          </select>

          <select
            value={statusFilter}
            onChange={(event) => setStatusFilter(event.target.value)}
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
          >
            {statusOptions.map((value) => (
              <option key={value || "any"} value={value}>
                {value || "Any status"}
              </option>
            ))}
          </select>

          <button
            type="submit"
            disabled={isLoading}
            className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60 md:col-span-6"
          >
            {isLoading ? "Loading..." : "Apply filters"}
          </button>
        </form>
      </section>

      <section className="grid gap-8 xl:grid-cols-[1fr_1.2fr]">
        <div className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
            <div>
              <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
                Versions
              </p>
              <p className="mt-3 text-sm text-slate-300">
                Select an existing prompt/disclaimer version, or create a new controlled wording entry.
              </p>
            </div>

            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                onClick={() => startCreateMode("PROMPT")}
                className="rounded-xl border border-cyan-300/40 px-4 py-2 text-xs font-semibold text-cyan-100 hover:bg-cyan-300/10"
              >
                New prompt
              </button>

              <button
                type="button"
                onClick={() => startCreateMode("DISCLAIMER")}
                className="rounded-xl border border-cyan-300/40 px-4 py-2 text-xs font-semibold text-cyan-100 hover:bg-cyan-300/10"
              >
                New disclaimer
              </button>
            </div>
          </div>

          {entries.length === 0 ? (
            <p className="mt-6 text-sm text-slate-300">
              No prompt/disclaimer versions match the current filter.
            </p>
          ) : (
            <div className="mt-6 grid gap-4">
              {entries.map((entry) => (
                <button
                  key={entry.id}
                  type="button"
                  onClick={() => selectEntry(entry)}
                  className={`rounded-xl border p-5 text-left transition ${
                    selectedEntryId === entry.id
                      ? "border-cyan-300 bg-cyan-300/10"
                      : "border-white/10 bg-slate-900 hover:border-cyan-300/60"
                  }`}
                >
                  <p className="font-mono text-xs text-cyan-200">{entry.versionKey}</p>
                  <h2 className="mt-2 text-base font-bold text-white">{entry.title}</h2>
                  <p className="mt-2 line-clamp-2 text-sm leading-6 text-slate-300">
                    {entry.description || "No description recorded."}
                  </p>

                  <div className="mt-3 flex flex-wrap gap-2 text-xs">
                    <span className="rounded-full border border-white/10 px-3 py-1 text-slate-300">
                      {entry.versionType}
                    </span>
                    <span className="rounded-full border border-white/10 px-3 py-1 text-slate-300">
                      {entry.category}
                    </span>
                    <span className="rounded-full border border-white/10 px-3 py-1 text-slate-300">
                      {entry.appliesTo}
                    </span>
                    <span className="rounded-full border border-cyan-300/30 bg-cyan-300/10 px-3 py-1 text-cyan-100">
                      {entry.approvalStatus}
                    </span>
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            {isCreateMode ? "Create version" : "Edit version"}
          </p>

          <form onSubmit={handleSave} className="mt-6 grid gap-5">
            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">Version key</span>
              <input
                value={versionKey}
                onChange={(event) => setVersionKey(event.target.value)}
                disabled={!isCreateMode}
                placeholder="claim-starter-pack-disclaimer-v1"
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500 disabled:opacity-60"
              />
            </label>

            <div className="grid gap-5 md:grid-cols-2">
              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Version type</span>
                <select
                  value={versionType}
                  onChange={(event) => setVersionType(event.target.value as "PROMPT" | "DISCLAIMER")}
                  disabled={!isCreateMode}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white disabled:opacity-60"
                >
                  <option value="PROMPT">PROMPT</option>
                  <option value="DISCLAIMER">DISCLAIMER</option>
                </select>
              </label>

              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Category</span>
                <input
                  value={category}
                  onChange={(event) => setCategory(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                />
              </label>
            </div>

            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">Title</span>
              <input
                value={title}
                onChange={(event) => setTitle(event.target.value)}
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
              />
            </label>

            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">Description</span>
              <textarea
                value={description}
                onChange={(event) => setDescription(event.target.value)}
                rows={3}
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
              />
            </label>

            <div className="grid gap-5 md:grid-cols-2">
              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Version label</span>
                <input
                  value={versionLabel}
                  onChange={(event) => setVersionLabel(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                />
              </label>

              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Applies to</span>
                <input
                  value={appliesTo}
                  onChange={(event) => setAppliesTo(event.target.value)}
                  placeholder="CLAIM_STARTER_PACK, DOCTOR_GUIDANCE_PACK, AI_DRAFTS"
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
                />
              </label>
            </div>

            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">Content</span>
              <textarea
                value={content}
                onChange={(event) => setContent(event.target.value)}
                rows={18}
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 font-mono text-sm leading-6 text-white"
              />
            </label>

            <div className="grid gap-5 md:grid-cols-2">
              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Approval status</span>
                <select
                  value={approvalStatus}
                  onChange={(event) => setApprovalStatus(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                >
                  {approvalStatuses.filter(Boolean).map((value) => (
                    <option key={value} value={value}>
                      {value}
                    </option>
                  ))}
                </select>
              </label>

              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Status</span>
                <select
                  value={status}
                  onChange={(event) => setStatus(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                >
                  {statusOptions.filter(Boolean).map((value) => (
                    <option key={value} value={value}>
                      {value}
                    </option>
                  ))}
                </select>
              </label>
            </div>

            <div className="grid gap-5 md:grid-cols-2">
              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Effective from</span>
                <input
                  type="datetime-local"
                  value={effectiveFrom}
                  onChange={(event) => setEffectiveFrom(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                />
              </label>

              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Retired at</span>
                <input
                  type="datetime-local"
                  value={retiredAt}
                  onChange={(event) => setRetiredAt(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                />
              </label>
            </div>

            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">Approved by</span>
              <input
                value={approvedBy}
                onChange={(event) => setApprovedBy(event.target.value)}
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
              />
            </label>

            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">Review notes</span>
              <textarea
                value={reviewNotes}
                onChange={(event) => setReviewNotes(event.target.value)}
                rows={4}
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
              />
            </label>

            <label className="flex items-center gap-3 rounded-xl border border-white/10 bg-slate-950 p-4 text-sm text-slate-200">
              <input
                type="checkbox"
                checked={isActive}
                onChange={(event) => setIsActive(event.target.checked)}
                className="h-4 w-4"
              />
              Version is active
            </label>

            <button
              type="submit"
              disabled={isSaving}
              className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
            >
              {isSaving ? "Saving..." : isCreateMode ? "Create version" : "Save version"}
            </button>
          </form>
        </div>
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

function toDateTimeLocal(value?: string | null) {
  if (!value) {
    return "";
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "";
  }

  const offset = date.getTimezoneOffset();
  const localDate = new Date(date.getTime() - offset * 60 * 1000);

  return localDate.toISOString().slice(0, 16);
}