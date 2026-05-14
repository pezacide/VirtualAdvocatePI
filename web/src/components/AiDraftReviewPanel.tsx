"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  AiDraft,
  ClaimCondition,
  archiveAiDraft,
  generateAiDraft,
  getClaimConditions,
  getConditionAiDrafts,
  getWorkspaceAiDrafts,
  updateAiDraft,
} from "@/lib/api";

type AiDraftReviewPanelProps = {
  workspaceId: string;
};

const draftTypes = [
  "VETERAN_STATEMENT",
  "WORSENING_SUMMARY",
  "DOCTOR_QUESTIONS",
  "EVIDENCE_GAP_SUMMARY",
  "DOCTOR_REQUEST_LETTER",
];

const reviewStatuses = [
  "USER_REVIEW_REQUIRED",
  "USER_EDITED",
  "APPROVED",
  "REJECTED",
  "REGENERATED",
];

export function AiDraftReviewPanel({ workspaceId }: AiDraftReviewPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [selectedConditionId, setSelectedConditionId] = useState("");
  const [selectedDraftType, setSelectedDraftType] = useState("VETERAN_STATEMENT");
  const [query, setQuery] = useState("");
  const [userInstruction, setUserInstruction] = useState("");

  const [drafts, setDrafts] = useState<AiDraft[]>([]);
  const [selectedDraftId, setSelectedDraftId] = useState("");
  const [editedDraftText, setEditedDraftText] = useState("");
  const [reviewStatus, setReviewStatus] = useState("USER_REVIEW_REQUIRED");

  const [isLoading, setIsLoading] = useState(false);
  const [isGenerating, setIsGenerating] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isArchiving, setIsArchiving] = useState(false);

  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [copyStatusMessage, setCopyStatusMessage] = useState("");

  const selectedDraft = useMemo(
    () => drafts.find((draft) => draft.id === selectedDraftId) ?? null,
    [drafts, selectedDraftId],
  );

  const selectedCondition = useMemo(
    () => conditions.find((condition) => condition.id === selectedConditionId) ?? null,
    [conditions, selectedConditionId],
  );

  const summary = useMemo(
    () => ({
      total: drafts.length,
      reviewRequired: drafts.filter(
        (draft) =>
          draft.reviewStatus === "USER_REVIEW_REQUIRED" ||
          draft.reviewStatus === "DRAFT_CREATED",
      ).length,
      edited: drafts.filter((draft) => draft.reviewStatus === "USER_EDITED").length,
      approved: drafts.filter((draft) => draft.reviewStatus === "APPROVED").length,
      rejected: drafts.filter((draft) => draft.reviewStatus === "REJECTED").length,
    }),
    [drafts],
  );

  async function getTokenOrSetError() {
    const token = await getIdToken();

    if (!token) {
      setErrorMessage("No Firebase ID token is available. Please sign in again.");
      return null;
    }

    return token;
  }

  async function loadConditions() {
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
    }
  }

  async function loadDrafts() {
    setErrorMessage("");
    setStatusMessage("");
    setIsLoading(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = selectedConditionId
        ? await getConditionAiDrafts(token, workspaceId, selectedConditionId)
        : await getWorkspaceAiDrafts(token, workspaceId);

      setDrafts(rows);

      if (rows.length > 0 && !rows.some((draft) => draft.id === selectedDraftId)) {
        selectDraft(rows[0]);
      }

      if (rows.length === 0) {
        setSelectedDraftId("");
        setEditedDraftText("");
        setReviewStatus("USER_REVIEW_REQUIRED");
      }

      setStatusMessage(`Loaded ${rows.length} AI draft(s).`);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load AI drafts.";
      setErrorMessage(message);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    if (!loading && user) {
      loadConditions();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  useEffect(() => {
    if (!loading && user) {
      loadDrafts();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId, selectedConditionId]);

  function selectDraft(draft: AiDraft) {
    setSelectedDraftId(draft.id);
    setEditedDraftText(draft.userEditedText ?? draft.draftText ?? "");
    setReviewStatus(draft.reviewStatus);
    setCopyStatusMessage("");
    setStatusMessage(`Selected ${formatDraftType(draft.draftType)} draft.`);
  }

  async function handleGenerateDraft(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setStatusMessage("");
    setErrorMessage("");
    setCopyStatusMessage("");

    if (!selectedConditionId) {
      setErrorMessage("Select a condition before generating a draft.");
      return;
    }

    setIsGenerating(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const response = await generateAiDraft(token, workspaceId, {
        conditionId: selectedConditionId,
        draftType: selectedDraftType,
        query: query || selectedDraftType,
        maxSources: 8,
        userInstruction: userInstruction || undefined,
      });

      await loadDrafts();

      setSelectedDraftId(response.aiDraft.id);
      setEditedDraftText(response.aiDraft.userEditedText ?? response.aiDraft.draftText);
      setReviewStatus(response.aiDraft.reviewStatus);

      setStatusMessage(
        `Generated ${formatDraftType(response.aiDraft.draftType)} draft. Review it before use.`,
      );
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not generate AI draft.";
      setErrorMessage(message);
    } finally {
      setIsGenerating(false);
    }
  }

  async function handleCopyDraft() {
    setCopyStatusMessage("");
    setErrorMessage("");

    if (!selectedDraft) {
      setErrorMessage("Select a draft before copying.");
      return;
    }

    const textToCopy = editedDraftText || selectedDraft.userEditedText || selectedDraft.draftText;

    try {
      await navigator.clipboard.writeText(textToCopy);
      setCopyStatusMessage("Copied to clipboard.");
    } catch {
      setCopyStatusMessage("Automatic copy failed. You can still select and copy the text manually.");
    }
  }

  async function handleSaveReview(statusOverride?: string) {
    setStatusMessage("");
    setErrorMessage("");
    setCopyStatusMessage("");

    if (!selectedDraft) {
      setErrorMessage("Select a draft before saving.");
      return;
    }

    setIsSaving(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const nextStatus = statusOverride ?? reviewStatus;

      const updated = await updateAiDraft(token, workspaceId, selectedDraft.id, {
        userEditedText: editedDraftText,
        reviewStatus: nextStatus,
      });

      setDrafts((current) =>
        current.map((draft) => (draft.id === updated.id ? updated : draft)),
      );

      setSelectedDraftId(updated.id);
      setEditedDraftText(updated.userEditedText ?? updated.draftText);
      setReviewStatus(updated.reviewStatus);

      setStatusMessage(`Draft saved with status ${updated.reviewStatus}.`);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not save AI draft review.";
      setErrorMessage(message);
    } finally {
      setIsSaving(false);
    }
  }

  async function handleArchiveDraft() {
    setStatusMessage("");
    setErrorMessage("");
    setCopyStatusMessage("");

    if (!selectedDraft) {
      setErrorMessage("Select a draft before archiving.");
      return;
    }

    const confirmed = window.confirm(
      "Archive this AI draft from the active workspace?\n\nIt will no longer appear in the active draft list. This does not contact DVA and does not submit anything.",
    );

    if (!confirmed) {
      return;
    }

    setIsArchiving(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      await archiveAiDraft(token, workspaceId, selectedDraft.id);

      setDrafts((current) => current.filter((draft) => draft.id !== selectedDraft.id));
      setSelectedDraftId("");
      setEditedDraftText("");
      setReviewStatus("USER_REVIEW_REQUIRED");

      setStatusMessage("AI draft archived.");
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not archive AI draft.";
      setErrorMessage(message);
    } finally {
      setIsArchiving(false);
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
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          AI draft review
        </p>
        <h2 className="mt-4 text-2xl font-bold text-white">Sign in required</h2>
        <p className="mt-2 text-sm text-slate-300">Sign in before reviewing AI drafts.</p>
      </section>
    );
  }

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          AI draft review
        </p>

        <h2 className="mt-4 text-2xl font-bold text-white">
          Generate, review, copy and save preparation drafts
        </h2>

        <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
          These drafts are preparation support only. Review and edit every draft before
          using it. This tool does not provide legal advice, medical advice, DVA decisions,
          impairment points, compensation estimates or claim outcome guarantees.
        </p>

        <div className="mt-6 grid gap-4 md:grid-cols-5">
          <SummaryCard label="Total drafts" value={summary.total} />
          <SummaryCard label="Needs review" value={summary.reviewRequired} />
          <SummaryCard label="Edited" value={summary.edited} />
          <SummaryCard label="Approved" value={summary.approved} />
          <SummaryCard label="Rejected" value={summary.rejected} />
        </div>

        {statusMessage && (
          <div className="mt-6 rounded-xl border border-emerald-300/30 bg-emerald-300/10 p-4 text-sm text-emerald-100">
            {statusMessage}
          </div>
        )}

        {copyStatusMessage && (
          <div className="mt-6 rounded-xl border border-cyan-300/30 bg-cyan-300/10 p-4 text-sm text-cyan-100">
            {copyStatusMessage}
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
          Generate draft
        </p>

        <form onSubmit={handleGenerateDraft} className="mt-6 grid gap-5">
          <label className="grid gap-2">
            <span className="text-sm font-medium text-slate-200">Condition</span>
            <select
              value={selectedConditionId}
              onChange={(event) => setSelectedConditionId(event.target.value)}
              className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
            >
              <option value="">Select a condition</option>
              {conditions.map((condition) => (
                <option key={condition.id} value={condition.id}>
                  {condition.conditionName}
                </option>
              ))}
            </select>
          </label>

          <label className="grid gap-2">
            <span className="text-sm font-medium text-slate-200">Draft type</span>
            <select
              value={selectedDraftType}
              onChange={(event) => setSelectedDraftType(event.target.value)}
              className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
            >
              {draftTypes.map((type) => (
                <option key={type} value={type}>
                  {formatDraftType(type)}
                </option>
              ))}
            </select>
          </label>

          <label className="grid gap-2">
            <span className="text-sm font-medium text-slate-200">Optional focus</span>
            <input
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder="Example: worsening, daily impact, doctor questions"
              className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
            />
          </label>

          <label className="grid gap-2">
            <span className="text-sm font-medium text-slate-200">Optional instruction</span>
            <textarea
              value={userInstruction}
              onChange={(event) => setUserInstruction(event.target.value)}
              rows={4}
              placeholder="Example: Keep this short and focus on sleep, work and medication side effects."
              className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
            />
          </label>

          <button
            type="submit"
            disabled={isGenerating || !selectedConditionId}
            className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-cyan-200 disabled:opacity-60"
          >
            {isGenerating ? "Generating draft..." : "Generate reviewable draft"}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
              Saved drafts
            </p>

            <h3 className="mt-4 text-xl font-bold text-white">
              Review and select a draft
            </h3>

            {selectedCondition && (
              <p className="mt-2 text-sm text-slate-400">
                Filtering by condition: {selectedCondition.conditionName}
              </p>
            )}
          </div>

          <button
            type="button"
            onClick={loadDrafts}
            disabled={isLoading}
            className="rounded-xl border border-cyan-300/40 px-5 py-3 text-sm font-semibold text-cyan-100 hover:bg-cyan-300/10 disabled:opacity-60"
          >
            {isLoading ? "Refreshing..." : "Refresh drafts"}
          </button>
        </div>

        {drafts.length === 0 ? (
          <p className="mt-6 text-sm text-slate-300">
            No active AI drafts are available for this filter yet.
          </p>
        ) : (
          <div className="mt-6 grid gap-4">
            {drafts.map((draft) => (
              <button
                key={draft.id}
                type="button"
                onClick={() => selectDraft(draft)}
                className={`rounded-xl border p-5 text-left transition ${
                  selectedDraftId === draft.id
                    ? "border-cyan-300 bg-cyan-300/10"
                    : "border-white/10 bg-slate-900 hover:border-cyan-300/60"
                }`}
              >
                <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                  <div>
                    <p className="text-lg font-semibold text-white">
                      {formatDraftType(draft.draftType)}
                    </p>
                    <p className="mt-1 text-xs text-slate-400">
                      Created {formatDate(draft.createdAt)}
                    </p>
                  </div>

                  <span className="rounded-full border border-cyan-300/30 bg-cyan-300/10 px-3 py-1 text-xs text-cyan-100">
                    {draft.reviewStatus}
                  </span>
                </div>

                <p className="mt-4 line-clamp-3 text-sm leading-6 text-slate-300">
                  {(draft.userEditedText || draft.draftText).slice(0, 260)}
                </p>
              </button>
            ))}
          </div>
        )}
      </section>

      {selectedDraft && (
        <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Draft review
          </p>

          <div className="mt-4 flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
            <div>
              <h3 className="text-xl font-bold text-white">
                {formatDraftType(selectedDraft.draftType)}
              </h3>
              <p className="mt-2 text-sm text-slate-400">
                Review status: {selectedDraft.reviewStatus}
              </p>
            </div>

            <div className="flex flex-wrap gap-3">
              <button
                type="button"
                onClick={handleCopyDraft}
                className="rounded-xl border border-cyan-300/40 px-4 py-2 text-sm font-semibold text-cyan-100 hover:bg-cyan-300/10"
              >
                Copy draft
              </button>

              <button
                type="button"
                onClick={() => handleSaveReview("APPROVED")}
                disabled={isSaving}
                className="rounded-xl border border-emerald-300/40 px-4 py-2 text-sm font-semibold text-emerald-100 hover:bg-emerald-300/10 disabled:opacity-60"
              >
                Approve
              </button>

              <button
                type="button"
                onClick={() => handleSaveReview("REJECTED")}
                disabled={isSaving}
                className="rounded-xl border border-yellow-300/40 px-4 py-2 text-sm font-semibold text-yellow-100 hover:bg-yellow-300/10 disabled:opacity-60"
              >
                Reject
              </button>

              <button
                type="button"
                onClick={handleArchiveDraft}
                disabled={isArchiving}
                className="rounded-xl border border-red-300/40 px-4 py-2 text-sm font-semibold text-red-100 hover:bg-red-300/10 disabled:opacity-60"
              >
                {isArchiving ? "Archiving..." : "Archive"}
              </button>
            </div>
          </div>

          <div className="mt-6 grid gap-5">
            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">Review status</span>
              <select
                value={reviewStatus}
                onChange={(event) => setReviewStatus(event.target.value)}
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
              >
                {reviewStatuses.map((status) => (
                  <option key={status} value={status}>
                    {status}
                  </option>
                ))}
              </select>
            </label>

            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">
                Reviewed draft text
              </span>
              <textarea
                value={editedDraftText}
                onChange={(event) => {
                  setEditedDraftText(event.target.value);
                  if (reviewStatus === "USER_REVIEW_REQUIRED") {
                    setReviewStatus("USER_EDITED");
                  }
                }}
                rows={20}
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 font-mono text-sm leading-6 text-white"
              />
            </label>

            <button
              type="button"
              onClick={() => handleSaveReview()}
              disabled={isSaving}
              className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-cyan-200 disabled:opacity-60"
            >
              {isSaving ? "Saving review..." : "Save draft review"}
            </button>

            <div className="rounded-xl border border-white/10 bg-slate-950 p-5">
              <p className="text-sm font-semibold text-white">Source references</p>
              <pre className="mt-3 max-h-64 overflow-auto whitespace-pre-wrap text-xs leading-5 text-slate-300">
                {formatSourceReferences(selectedDraft.sourceReferences)}
              </pre>
            </div>
          </div>
        </section>
      )}
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

function formatDraftType(value: string) {
  return value
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

function formatSourceReferences(value?: string | null) {
  if (!value) {
    return "No source references recorded.";
  }

  try {
    return JSON.stringify(JSON.parse(value), null, 2);
  } catch {
    return value;
  }
}