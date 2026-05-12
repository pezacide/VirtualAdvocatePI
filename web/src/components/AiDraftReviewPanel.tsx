"use client";

import Link from "next/link";
import { FormEvent, useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  AiDraft,
  ClaimCondition,
  createAiDraft,
  getClaimConditions,
  getConditionAiDrafts,
  getWorkspaceAiDrafts,
  updateAiDraft,
} from "@/lib/apiClient";

type AiDraftReviewPanelProps = {
  workspaceId: string;
};

const draftTypes = [
  "VETERAN_STATEMENT",
  "WORSENING_SUMMARY",
  "EVIDENCE_GAP_SUMMARY",
  "DOCTOR_APPOINTMENT_QUESTIONS",
  "DOCTOR_REQUEST_LETTER",
  "CLAIM_PACK_COVER_NOTE",
];

const reviewStatuses = [
  "DRAFT_CREATED",
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
  const [showWorkspaceDrafts, setShowWorkspaceDrafts] = useState(true);
  const [drafts, setDrafts] = useState<AiDraft[]>([]);
  const [selectedDraftId, setSelectedDraftId] = useState("");

  const [draftType, setDraftType] = useState("VETERAN_STATEMENT");
  const [promptVersion, setPromptVersion] = useState("manual-web-v1");
  const [sourceReferences, setSourceReferences] = useState("");
  const [draftText, setDraftText] = useState("");
  const [userEditedText, setUserEditedText] = useState("");
  const [reviewStatus, setReviewStatus] = useState("USER_REVIEW_REQUIRED");

  const [isLoadingConditions, setIsLoadingConditions] = useState(false);
  const [isLoadingDrafts, setIsLoadingDrafts] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);

  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  const selectedCondition = conditions.find(
    (condition) => condition.id === selectedConditionId,
  );

  const selectedDraft = useMemo(
    () => drafts.find((draft) => draft.id === selectedDraftId) ?? null,
    [drafts, selectedDraftId],
  );

  const summary = useMemo(() => {
    return {
      total: drafts.length,
      approved: drafts.filter((draft) => draft.reviewStatus === "APPROVED").length,
      needsReview: drafts.filter(
        (draft) =>
          draft.reviewStatus === "USER_REVIEW_REQUIRED" ||
          draft.reviewStatus === "DRAFT_CREATED",
      ).length,
      edited: drafts.filter((draft) => draft.reviewStatus === "USER_EDITED").length,
      rejected: drafts.filter((draft) => draft.reviewStatus === "REJECTED").length,
    };
  }, [drafts]);

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

  async function loadDrafts() {
    if (loading || !user) {
      return;
    }

    setIsLoadingDrafts(true);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows =
        showWorkspaceDrafts || !selectedConditionId
          ? await getWorkspaceAiDrafts(token, workspaceId)
          : await getConditionAiDrafts(token, workspaceId, selectedConditionId);

      setDrafts(rows);

      if (rows.length > 0 && !rows.some((draft) => draft.id === selectedDraftId)) {
        setSelectedDraftId(rows[0].id);
      }

      if (rows.length === 0) {
        setSelectedDraftId("");
      }
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load AI drafts.";
      setErrorMessage(message);
    } finally {
      setIsLoadingDrafts(false);
    }
  }

  useEffect(() => {
    loadConditions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  useEffect(() => {
    loadDrafts();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId, selectedConditionId, showWorkspaceDrafts]);

  useEffect(() => {
    if (!selectedDraft) {
      return;
    }

    setDraftType(selectedDraft.draftType);
    setPromptVersion(selectedDraft.promptVersion);
    setSourceReferences(selectedDraft.sourceReferences ?? "");
    setDraftText(selectedDraft.draftText);
    setUserEditedText(selectedDraft.userEditedText ?? selectedDraft.draftText);
    setReviewStatus(selectedDraft.reviewStatus);
  }, [selectedDraft]);

  async function handleCreateDraft(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setStatusMessage("");
    setErrorMessage("");
    setIsCreating(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      if (!draftText.trim()) {
        setErrorMessage("Draft text is required.");
        return;
      }

      await createAiDraft(token, workspaceId, {
        conditionId: selectedConditionId || undefined,
        draftType,
        promptVersion: promptVersion || "manual-web-v1",
        sourceReferences: sourceReferences || undefined,
        draftText,
        userEditedText: userEditedText || undefined,
        reviewStatus,
      });

      setDraftText("");
      setUserEditedText("");
      setSourceReferences("");
      setReviewStatus("USER_REVIEW_REQUIRED");
      setStatusMessage("AI draft metadata created.");
      await loadDrafts();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not create AI draft.";
      setErrorMessage(message);
    } finally {
      setIsCreating(false);
    }
  }

  async function handleUpdateDraft(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setStatusMessage("");
    setErrorMessage("");
    setIsUpdating(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      if (!selectedDraft) {
        setErrorMessage("Select a draft before saving changes.");
        return;
      }

      await updateAiDraft(token, workspaceId, selectedDraft.id, {
        draftType,
        promptVersion,
        sourceReferences,
        draftText,
        userEditedText,
        reviewStatus,
      });

      setStatusMessage("AI draft review updated.");
      await loadDrafts();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not update AI draft.";
      setErrorMessage(message);
    } finally {
      setIsUpdating(false);
    }
  }

  function handleUseTemplate(templateType: string) {
    setDraftType(templateType);
    setPromptVersion("manual-web-v1");
    setReviewStatus("USER_REVIEW_REQUIRED");

    if (templateType === "VETERAN_STATEMENT") {
      setDraftText(
        "Draft veteran statement placeholder. Summarise the condition, current symptoms, treatment, functional impact, and what evidence is available. This must be reviewed and edited by the user before use.",
      );
      setSourceReferences("Condition intake, guided questions, evidence metadata");
      return;
    }

    if (templateType === "DOCTOR_APPOINTMENT_QUESTIONS") {
      setDraftText(
        "Draft doctor appointment questions placeholder:\n\n1. Can you confirm the current diagnosis?\n2. Can you describe current severity and treatment?\n3. Can you comment on functional impact?\n4. Are there any records or reports that should be gathered?",
      );
      setSourceReferences("Condition intake, evidence gaps");
      return;
    }

    if (templateType === "EVIDENCE_GAP_SUMMARY") {
      setDraftText(
        "Draft evidence gap summary placeholder. Summarise missing or incomplete evidence in plain English and list possible next steps for the user to discuss with a doctor, advocate, lawyer or support person.",
      );
      setSourceReferences("Evidence gap tracker");
      return;
    }

    setDraftText(
      "Draft placeholder. This is preparation text only and must be reviewed before being used in any document.",
    );
    setSourceReferences("Manual web draft");
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
        <p className="mt-2 text-sm">Sign in before reviewing AI drafts.</p>
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

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          AI draft review
        </p>

        <h1 className="mt-4 text-3xl font-bold">Review preparation drafts</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          Create and review draft preparation text. This is not live AI generation yet. The
          current UI stores draft metadata, edited text and review status using the backend.
        </p>

        <div className="mt-8 grid gap-4 md:grid-cols-5">
          <SummaryCard label="Total" value={summary.total} />
          <SummaryCard label="Needs review" value={summary.needsReview} />
          <SummaryCard label="Edited" value={summary.edited} />
          <SummaryCard label="Approved" value={summary.approved} />
          <SummaryCard label="Rejected" value={summary.rejected} />
        </div>

        <div className="mt-8 grid gap-5 md:grid-cols-2">
          <div>
            <label htmlFor="condition" className="text-sm font-medium text-slate-200">
              Condition
            </label>

            <select
              id="condition"
              value={selectedConditionId}
              onChange={(event) => setSelectedConditionId(event.target.value)}
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            >
              <option value="">Workspace-level draft</option>
              {conditions.map((condition) => (
                <option key={condition.id} value={condition.id}>
                  {condition.conditionName}
                </option>
              ))}
            </select>

            {selectedCondition && (
              <p className="mt-2 text-sm text-slate-400">
                Selected condition: {selectedCondition.conditionName}
              </p>
            )}
          </div>

          <div>
            <label className="text-sm font-medium text-slate-200">Draft list scope</label>

            <div className="mt-2 flex rounded-xl border border-white/10 bg-slate-900 p-1">
              <button
                type="button"
                onClick={() => setShowWorkspaceDrafts(true)}
                className={
                  showWorkspaceDrafts
                    ? "flex-1 rounded-lg bg-cyan-300 px-4 py-2 text-sm font-semibold text-slate-950"
                    : "flex-1 rounded-lg px-4 py-2 text-sm font-semibold text-slate-300"
                }
              >
                Workspace drafts
              </button>

              <button
                type="button"
                onClick={() => setShowWorkspaceDrafts(false)}
                className={
                  !showWorkspaceDrafts
                    ? "flex-1 rounded-lg bg-cyan-300 px-4 py-2 text-sm font-semibold text-slate-950"
                    : "flex-1 rounded-lg px-4 py-2 text-sm font-semibold text-slate-300"
                }
              >
                Selected condition
              </button>
            </div>
          </div>
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
          Create draft metadata
        </p>

        <h2 className="mt-4 text-2xl font-bold">Create a draft record</h2>

        <div className="mt-6 flex flex-wrap gap-3">
          {["VETERAN_STATEMENT", "DOCTOR_APPOINTMENT_QUESTIONS", "EVIDENCE_GAP_SUMMARY"].map(
            (templateType) => (
              <button
                key={templateType}
                type="button"
                onClick={() => handleUseTemplate(templateType)}
                className="rounded-xl border border-white/10 bg-slate-900 px-4 py-2 text-sm text-slate-200 hover:bg-white/10"
              >
                Use {templateType}
              </button>
            ),
          )}
        </div>

        <form onSubmit={handleCreateDraft} className="mt-8 space-y-6">
          <DraftFields
            draftType={draftType}
            setDraftType={setDraftType}
            promptVersion={promptVersion}
            setPromptVersion={setPromptVersion}
            sourceReferences={sourceReferences}
            setSourceReferences={setSourceReferences}
            draftText={draftText}
            setDraftText={setDraftText}
            userEditedText={userEditedText}
            setUserEditedText={setUserEditedText}
            reviewStatus={reviewStatus}
            setReviewStatus={setReviewStatus}
          />

          <button
            type="submit"
            disabled={isCreating}
            className="w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
          >
            {isCreating ? "Creating draft..." : "Create draft metadata"}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Existing drafts
        </p>

        {isLoadingDrafts ? (
          <p className="mt-6 text-slate-300">Loading drafts...</p>
        ) : drafts.length === 0 ? (
          <p className="mt-6 text-slate-300">No AI draft metadata has been created yet.</p>
        ) : (
          <div className="mt-6 grid gap-4">
            {drafts.map((draft) => (
              <button
                key={draft.id}
                type="button"
                onClick={() => setSelectedDraftId(draft.id)}
                className={
                  selectedDraftId === draft.id
                    ? "rounded-xl border border-cyan-300 bg-cyan-300/10 p-5 text-left"
                    : "rounded-xl border border-white/10 bg-slate-900 p-5 text-left hover:bg-white/5"
                }
              >
                <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <p className="font-mono text-xs text-cyan-200">{draft.draftType}</p>
                    <h3 className="mt-2 font-semibold text-white">
                      {draft.userEditedText || draft.draftText}
                    </h3>
                  </div>

                  <span className="rounded-xl border border-white/10 px-3 py-2 text-xs text-slate-300">
                    {draft.reviewStatus}
                  </span>
                </div>

                <p className="mt-3 line-clamp-3 text-sm leading-6 text-slate-400">
                  {draft.draftText}
                </p>
              </button>
            ))}
          </div>
        )}
      </section>

      {selectedDraft && (
        <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Review selected draft
          </p>

          <h2 className="mt-4 text-2xl font-bold">Edit and set review status</h2>

          <form onSubmit={handleUpdateDraft} className="mt-8 space-y-6">
            <DraftFields
              draftType={draftType}
              setDraftType={setDraftType}
              promptVersion={promptVersion}
              setPromptVersion={setPromptVersion}
              sourceReferences={sourceReferences}
              setSourceReferences={setSourceReferences}
              draftText={draftText}
              setDraftText={setDraftText}
              userEditedText={userEditedText}
              setUserEditedText={setUserEditedText}
              reviewStatus={reviewStatus}
              setReviewStatus={setReviewStatus}
            />

            <button
              type="submit"
              disabled={isUpdating}
              className="w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
            >
              {isUpdating ? "Saving review..." : "Save draft review"}
            </button>
          </form>
        </section>
      )}

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. Drafts must be reviewed by the user before use. This page does
        not provide legal advice, medical advice, a DVA decision, a compensation estimate, or a
        guarantee of claim success.
      </section>
    </div>
  );
}

type DraftFieldsProps = {
  draftType: string;
  setDraftType: (value: string) => void;
  promptVersion: string;
  setPromptVersion: (value: string) => void;
  sourceReferences: string;
  setSourceReferences: (value: string) => void;
  draftText: string;
  setDraftText: (value: string) => void;
  userEditedText: string;
  setUserEditedText: (value: string) => void;
  reviewStatus: string;
  setReviewStatus: (value: string) => void;
};

function DraftFields({
  draftType,
  setDraftType,
  promptVersion,
  setPromptVersion,
  sourceReferences,
  setSourceReferences,
  draftText,
  setDraftText,
  userEditedText,
  setUserEditedText,
  reviewStatus,
  setReviewStatus,
}: DraftFieldsProps) {
  return (
    <>
      <div className="grid gap-5 md:grid-cols-2">
        <div>
          <label htmlFor="draftType" className="text-sm font-medium text-slate-200">
            Draft type
          </label>

          <select
            id="draftType"
            value={draftType}
            onChange={(event) => setDraftType(event.target.value)}
            className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
          >
            {draftTypes.map((type) => (
              <option key={type} value={type}>
                {type}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="reviewStatus" className="text-sm font-medium text-slate-200">
            Review status
          </label>

          <select
            id="reviewStatus"
            value={reviewStatus}
            onChange={(event) => setReviewStatus(event.target.value)}
            className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
          >
            {reviewStatuses.map((status) => (
              <option key={status} value={status}>
                {status}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div>
        <label htmlFor="promptVersion" className="text-sm font-medium text-slate-200">
          Prompt or template version
        </label>

        <input
          id="promptVersion"
          type="text"
          value={promptVersion}
          onChange={(event) => setPromptVersion(event.target.value)}
          className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
        />
      </div>

      <div>
        <label htmlFor="sourceReferences" className="text-sm font-medium text-slate-200">
          Source references
        </label>

        <textarea
          id="sourceReferences"
          value={sourceReferences}
          onChange={(event) => setSourceReferences(event.target.value)}
          rows={3}
          placeholder="Example: condition intake, guided questions, evidence gaps"
          className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
        />
      </div>

      <div>
        <label htmlFor="draftText" className="text-sm font-medium text-slate-200">
          Draft text
        </label>

        <textarea
          id="draftText"
          value={draftText}
          onChange={(event) => setDraftText(event.target.value)}
          rows={7}
          required
          className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
        />
      </div>

      <div>
        <label htmlFor="userEditedText" className="text-sm font-medium text-slate-200">
          User edited text
        </label>

        <textarea
          id="userEditedText"
          value={userEditedText}
          onChange={(event) => setUserEditedText(event.target.value)}
          rows={7}
          placeholder="Edit the draft here before approving or using it."
          className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
        />
      </div>
    </>
  );
}

function SummaryCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl border border-white/10 bg-slate-900 p-5">
      <p className="text-sm text-slate-400">{label}</p>
      <p className="mt-2 text-2xl font-bold text-white">{value}</p>
    </div>
  );
}