"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  AdminMeResponse,
  AdminSourceRegistryEntry,
  getAdminMe,
  getAdminSourceRegistryEntries,
  updateAdminSourceRegistryEntry,
} from "@/lib/api";

const approvalStatuses = ["", "DRAFT", "PENDING_REVIEW", "APPROVED", "REJECTED"];
const sourceStatuses = ["", "ACTIVE", "ARCHIVED"];
const activeFilters = ["", "true", "false"];

export function AdminSourceMetadataManagerPanel() {
  const { user, loading, getIdToken } = useAuth();

  const [adminStatus, setAdminStatus] = useState<AdminMeResponse | null>(null);
  const [entries, setEntries] = useState<AdminSourceRegistryEntry[]>([]);
  const [selectedEntryId, setSelectedEntryId] = useState("");

  const [search, setSearch] = useState("");
  const [category, setCategory] = useState("");
  const [sourceType, setSourceType] = useState("");
  const [approvalStatusFilter, setApprovalStatusFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [isActiveFilter, setIsActiveFilter] = useState("");

  const [title, setTitle] = useState("");
  const [editCategory, setEditCategory] = useState("");
  const [editSourceType, setEditSourceType] = useState("");
  const [jurisdiction, setJurisdiction] = useState("");
  const [sourceVersion, setSourceVersion] = useState("");
  const [citationLabel, setCitationLabel] = useState("");
  const [sourceUrl, setSourceUrl] = useState("");
  const [storagePath, setStoragePath] = useState("");
  const [contentHash, setContentHash] = useState("");
  const [approvalStatus, setApprovalStatus] = useState("APPROVED");
  const [approvedBy, setApprovedBy] = useState("");
  const [reviewNotes, setReviewNotes] = useState("");
  const [isActive, setIsActive] = useState(true);
  const [status, setStatus] = useState("ACTIVE");

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
      approved: entries.filter((entry) => entry.approvalStatus === "APPROVED").length,
      active: entries.filter((entry) => entry.isActive && entry.status === "ACTIVE").length,
      archived: entries.filter((entry) => entry.status === "ARCHIVED").length,
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

      const rows = await getAdminSourceRegistryEntries(token, {
        search,
        category,
        sourceType,
        approvalStatus: approvalStatusFilter,
        status: statusFilter,
        isActive: isActiveFilter,
      });

      setEntries(rows);

      if (rows.length > 0 && !rows.some((entry) => entry.id === selectedEntryId)) {
        selectEntry(rows[0]);
      }

      if (rows.length === 0) {
        setSelectedEntryId("");
        clearForm();
      }

      setStatusMessage(`Loaded ${rows.length} source registry entr${rows.length === 1 ? "y" : "ies"}.`);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load source registry entries.";
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

  function clearForm() {
    setTitle("");
    setEditCategory("");
    setEditSourceType("");
    setJurisdiction("");
    setSourceVersion("");
    setCitationLabel("");
    setSourceUrl("");
    setStoragePath("");
    setContentHash("");
    setApprovalStatus("APPROVED");
    setApprovedBy("");
    setReviewNotes("");
    setIsActive(true);
    setStatus("ACTIVE");
  }

  function selectEntry(entry: AdminSourceRegistryEntry) {
    setSelectedEntryId(entry.id);
    setTitle(entry.title);
    setEditCategory(entry.category);
    setEditSourceType(entry.sourceType);
    setJurisdiction(entry.jurisdiction);
    setSourceVersion(entry.sourceVersion ?? "");
    setCitationLabel(entry.citationLabel);
    setSourceUrl(entry.sourceUrl ?? "");
    setStoragePath(entry.storagePath ?? "");
    setContentHash(entry.contentHash ?? "");
    setApprovalStatus(entry.approvalStatus);
    setApprovedBy(entry.approvedBy ?? "");
    setReviewNotes(entry.reviewNotes ?? "");
    setIsActive(entry.isActive);
    setStatus(entry.status);
    setStatusMessage(`Selected ${entry.sourceKey}.`);
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

    if (!selectedEntry) {
      setErrorMessage("Select a source registry entry before saving.");
      setIsSaving(false);
      return;
    }

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const updated = await updateAdminSourceRegistryEntry(token, selectedEntry.id, {
        title,
        category: editCategory,
        sourceType: editSourceType,
        jurisdiction,
        sourceVersion,
        citationLabel,
        sourceUrl,
        storagePath,
        contentHash,
        approvalStatus,
        approvedBy,
        reviewNotes,
        isActive,
        status,
      });

      setEntries((current) =>
        current.map((entry) => (entry.id === updated.id ? updated : entry)),
      );

      selectEntry(updated);
      setStatusMessage(`Saved source metadata for ${updated.sourceKey}.`);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not save source registry entry.";
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
          Source metadata manager
        </p>

        <h1 className="mt-4 text-3xl font-bold text-white">
          Approved source registry
        </h1>

        <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
          Review and update source registry metadata used by the AI/RAG knowledge base.
          Source key values are treated as stable identifiers and are not edited here.
        </p>

        <div className="mt-6 grid gap-4 md:grid-cols-4">
          <SummaryCard label="Loaded" value={summary.total} />
          <SummaryCard label="Approved" value={summary.approved} />
          <SummaryCard label="Active" value={summary.active} />
          <SummaryCard label="Archived" value={summary.archived} />
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
            placeholder="Search key, title or citation"
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
          />

          <input
            value={category}
            onChange={(event) => setCategory(event.target.value)}
            placeholder="Category"
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
          />

          <input
            value={sourceType}
            onChange={(event) => setSourceType(event.target.value)}
            placeholder="Source type"
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
          />

          <select
            value={approvalStatusFilter}
            onChange={(event) => setApprovalStatusFilter(event.target.value)}
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
          >
            {approvalStatuses.map((value) => (
              <option key={value || "any"} value={value}>
                {value || "Any approval status"}
              </option>
            ))}
          </select>

          <select
            value={statusFilter}
            onChange={(event) => setStatusFilter(event.target.value)}
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
          >
            {sourceStatuses.map((value) => (
              <option key={value || "any"} value={value}>
                {value || "Any status"}
              </option>
            ))}
          </select>

          <select
            value={isActiveFilter}
            onChange={(event) => setIsActiveFilter(event.target.value)}
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
          >
            {activeFilters.map((value) => (
              <option key={value || "any"} value={value}>
                {value === "true"
                  ? "Active only"
                  : value === "false"
                    ? "Inactive only"
                    : "Any active flag"}
              </option>
            ))}
          </select>

          <button
            type="submit"
            disabled={isLoading}
            className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-cyan-200 disabled:opacity-60 md:col-span-3"
          >
            {isLoading ? "Loading..." : "Apply filters"}
          </button>
        </form>
      </section>

      <section className="grid gap-8 xl:grid-cols-[1fr_1.2fr]">
        <div className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Source entries
          </p>

          {entries.length === 0 ? (
            <p className="mt-6 text-sm text-slate-300">
              No source registry entries match the current filter.
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
                  <p className="font-mono text-xs text-cyan-200">{entry.sourceKey}</p>
                  <h2 className="mt-2 text-base font-bold text-white">{entry.title}</h2>
                  <div className="mt-3 flex flex-wrap gap-2 text-xs">
                    <span className="rounded-full border border-white/10 px-3 py-1 text-slate-300">
                      {entry.category}
                    </span>
                    <span className="rounded-full border border-white/10 px-3 py-1 text-slate-300">
                      {entry.sourceType}
                    </span>
                    <span className="rounded-full border border-cyan-300/30 bg-cyan-300/10 px-3 py-1 text-cyan-100">
                      {entry.approvalStatus}
                    </span>
                    <span className="rounded-full border border-white/10 px-3 py-1 text-slate-300">
                      {entry.isActive ? "Active" : "Inactive"}
                    </span>
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Edit metadata
          </p>

          {!selectedEntry ? (
            <p className="mt-6 text-sm text-slate-300">
              Select a source entry to edit metadata.
            </p>
          ) : (
            <form onSubmit={handleSave} className="mt-6 grid gap-5">
              <div className="rounded-xl border border-white/10 bg-slate-950 p-4 text-sm leading-6 text-slate-300">
                <p>Source key: {selectedEntry.sourceKey}</p>
                <p>Created: {selectedEntry.createdAt}</p>
                <p>Updated: {selectedEntry.updatedAt}</p>
              </div>

              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Title</span>
                <input
                  value={title}
                  onChange={(event) => setTitle(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                />
              </label>

              <div className="grid gap-5 md:grid-cols-2">
                <label className="grid gap-2">
                  <span className="text-sm font-medium text-slate-200">Category</span>
                  <input
                    value={editCategory}
                    onChange={(event) => setEditCategory(event.target.value)}
                    className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                  />
                </label>

                <label className="grid gap-2">
                  <span className="text-sm font-medium text-slate-200">Source type</span>
                  <input
                    value={editSourceType}
                    onChange={(event) => setEditSourceType(event.target.value)}
                    className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                  />
                </label>
              </div>

              <div className="grid gap-5 md:grid-cols-2">
                <label className="grid gap-2">
                  <span className="text-sm font-medium text-slate-200">Jurisdiction</span>
                  <input
                    value={jurisdiction}
                    onChange={(event) => setJurisdiction(event.target.value)}
                    className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                  />
                </label>

                <label className="grid gap-2">
                  <span className="text-sm font-medium text-slate-200">Source version</span>
                  <input
                    value={sourceVersion}
                    onChange={(event) => setSourceVersion(event.target.value)}
                    className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                  />
                </label>
              </div>

              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Citation label</span>
                <input
                  value={citationLabel}
                  onChange={(event) => setCitationLabel(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                />
              </label>

              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Source URL</span>
                <input
                  value={sourceUrl}
                  onChange={(event) => setSourceUrl(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                />
              </label>

              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Storage path</span>
                <input
                  value={storagePath}
                  onChange={(event) => setStoragePath(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                />
              </label>

              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Content hash</span>
                <input
                  value={contentHash}
                  onChange={(event) => setContentHash(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
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
                    {sourceStatuses.filter(Boolean).map((value) => (
                      <option key={value} value={value}>
                        {value}
                      </option>
                    ))}
                  </select>
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
                  rows={5}
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
                Source is active
              </label>

              <button
                type="submit"
                disabled={isSaving}
                className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-cyan-200 disabled:opacity-60"
              >
                {isSaving ? "Saving..." : "Save source metadata"}
              </button>
            </form>
          )}
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