"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  AiDraft,
  ClaimCondition,
  generateAiDraft,
  getClaimConditions,
  getConditionAiDrafts,
  updateAiDraft,
} from "@/lib/api";

type DoctorGuidanceQuestionPanelProps = {
  workspaceId: string;
};

const doctorDraftTypes = [
  {
    value: "DOCTOR_QUESTIONS",
    label: "Doctor appointment questions",
    description: "Generate respectful questions to ask a doctor, specialist or treating provider.",
  },
  {
    value: "EVIDENCE_GAP_SUMMARY",
    label: "Evidence gap discussion points",
    description: "Generate practical follow-up points about missing or unclear evidence.",
  },
  {
    value: "DOCTOR_REQUEST_LETTER",
    label: "Doctor request letter",
    description: "Generate a polite letter asking for clinically appropriate information.",
  },
];

export function DoctorGuidanceQuestionPanel({
  workspaceId,
}: DoctorGuidanceQuestionPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [selectedConditionId, setSelectedConditionId] = useState("");
  const [draftType, setDraftType] = useState("DOCTOR_QUESTIONS");
  const [appointmentFocus, setAppointmentFocus] = useState("");
  const [additionalContext, setAdditionalContext] = useState("");

  const [drafts, setDrafts] = useState<AiDraft[]>([]);
  const [selectedDraftId, setSelectedDraftId] = useState("");
  const [reviewText, setReviewText] = useState("");

  const [isLoading, setIsLoading] = useState(false);
  const [isGenerating, setIsGenerating] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [copyMessage, setCopyMessage] = useState("");

  const selectedCondition = useMemo(
    () => conditions.find((condition) => condition.id === selectedConditionId) ?? null,
    [conditions, selectedConditionId],
  );

  const selectedDraft = useMemo(
    () => drafts.find((draft) => draft.id === selectedDraftId) ?? null,
    [drafts, selectedDraftId],
  );

  const doctorDrafts = useMemo(
    () =>
      drafts.filter((draft) =>
        ["DOCTOR_QUESTIONS", "EVIDENCE_GAP_SUMMARY", "DOCTOR_REQUEST_LETTER"].includes(
          draft.draftType,
        ),
      ),
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

  async function loadDrafts(conditionId = selectedConditionId) {
    if (!conditionId) {
      setDrafts([]);
      setSelectedDraftId("");
      setReviewText("");
      return;
    }

    setIsLoading(true);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = await getConditionAiDrafts(token, workspaceId, conditionId);
      const filteredRows = rows.filter((draft) =>
        ["DOCTOR_QUESTIONS", "EVIDENCE_GAP_SUMMARY", "DOCTOR_REQUEST_LETTER"].includes(
          draft.draftType,
        ),
      );

      setDrafts(filteredRows);

      if (filteredRows.length > 0) {
        const existing = filteredRows.find((draft) => draft.id === selectedDraftId);
        const nextDraft = existing ?? filteredRows[0];

        setSelectedDraftId(nextDraft.id);
        setReviewText(nextDraft.userEditedText ?? nextDraft.draftText);
      } else {
        setSelectedDraftId("");
        setReviewText("");
      }
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load doctor guidance drafts.";
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
    if (!loading && user && selectedConditionId) {
      loadDrafts(selectedConditionId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId, selectedConditionId]);

  function selectDraft(draft: AiDraft) {
    setSelectedDraftId(draft.id);
    setReviewText(draft.userEditedText ?? draft.draftText);
    setCopyMessage("");
    setStatusMessage(`Selected ${formatLabel(draft.draftType)}.`);
  }

  async function handleGenerate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setStatusMessage("");
    setErrorMessage("");
    setCopyMessage("");

    if (!selectedConditionId) {
      setErrorMessage("Select a condition before generating doctor guidance questions.");
      return;
    }

    setIsGenerating(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const focus = appointmentFocus || doctorDraftTypes.find((item) => item.value === draftType)?.label;
      const instructionParts = [
        "Create doctor guidance material that is respectful, neutral and non-pressuring.",
        "Do not tell the doctor what opinion to provide.",
        "Do not ask the doctor to make a DVA decision.",
        "Focus on clinically appropriate appointment preparation.",
        additionalContext,
      ].filter(Boolean);

      const response = await generateAiDraft(token, workspaceId, {
        conditionId: selectedConditionId,
        draftType,
        query: focus ?? draftType,
        maxSources: 8,
        userInstruction: instructionParts.join("\n"),
      });

      await loadDrafts(selectedConditionId);

      setSelectedDraftId(response.aiDraft.id);
      setReviewText(response.aiDraft.userEditedText ?? response.aiDraft.draftText);

      setStatusMessage(
        `${formatLabel(response.aiDraft.draftType)} generated. Review and edit it before use.`,
      );
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not generate doctor guidance material.";
      setErrorMessage(message);
    } finally {
      setIsGenerating(false);
    }
  }

  async function handleCopy() {
    setCopyMessage("");
    setErrorMessage("");

    if (!selectedDraft) {
      setErrorMessage("Select a draft before copying.");
      return;
    }

    try {
      await navigator.clipboard.writeText(reviewText || selectedDraft.draftText);
      setCopyMessage("Copied to clipboard.");
    } catch {
      setCopyMessage("Automatic copy failed. Select the text and copy it manually.");
    }
  }

  async function handleSave(status: "USER_EDITED" | "APPROVED" | "REJECTED") {
    setIsSaving(true);
    setStatusMessage("");
    setErrorMessage("");
    setCopyMessage("");

    if (!selectedDraft) {
      setErrorMessage("Select a draft before saving.");
      setIsSaving(false);
      return;
    }

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const updated = await updateAiDraft(token, workspaceId, selectedDraft.id, {
        userEditedText: reviewText,
        reviewStatus: status,
      });

      setDrafts((current) =>
        current.map((draft) => (draft.id === updated.id ? updated : draft)),
      );

      setSelectedDraftId(updated.id);
      setReviewText(updated.userEditedText ?? updated.draftText);
      setStatusMessage(`Saved as ${updated.reviewStatus}.`);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not save doctor guidance draft.";
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
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Doctor guidance
        </p>
        <h2 className="mt-4 text-2xl font-bold text-white">Sign in required</h2>
        <p className="mt-2 text-sm text-slate-300">
          Sign in before generating doctor guidance material.
        </p>
      </section>
    );
  }

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Doctor guidance
        </p>

        <h2 className="mt-4 text-2xl font-bold text-white">
          Build clinical appointment questions
        </h2>

        <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
          Generate doctor guidance material for appointment preparation. This does not provide
          medical advice, legal advice, diagnosis, DVA decisions, impairment points,
          compensation estimates or claim outcome guarantees. It must not tell a doctor what
          opinion to provide or pressure a doctor to support a claim.
        </p>

        {statusMessage && (
          <div className="mt-6 rounded-xl border border-emerald-300/30 bg-emerald-300/10 p-4 text-sm text-emerald-100">
            {statusMessage}
          </div>
        )}

        {copyMessage && (
          <div className="mt-6 rounded-xl border border-cyan-300/30 bg-cyan-300/10 p-4 text-sm text-cyan-100">
            {copyMessage}
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
          Generate
        </p>

        <form onSubmit={handleGenerate} className="mt-6 grid gap-5">
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
            <span className="text-sm font-medium text-slate-200">Doctor guidance type</span>
            <select
              value={draftType}
              onChange={(event) => setDraftType(event.target.value)}
              className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
            >
              {doctorDraftTypes.map((item) => (
                <option key={item.value} value={item.value}>
                  {item.label}
                </option>
              ))}
            </select>
          </label>

          <div className="rounded-xl border border-cyan-300/20 bg-cyan-300/5 p-4 text-sm leading-6 text-cyan-50">
            {doctorDraftTypes.find((item) => item.value === draftType)?.description}
          </div>

          <label className="grid gap-2">
            <span className="text-sm font-medium text-slate-200">Appointment focus</span>
            <input
              value={appointmentFocus}
              onChange={(event) => setAppointmentFocus(event.target.value)}
              placeholder="Example: medication side effects, worsening symptoms, work impact, specialist referral"
              className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
            />
          </label>

          <label className="grid gap-2">
            <span className="text-sm font-medium text-slate-200">Extra context for this appointment</span>
            <textarea
              value={additionalContext}
              onChange={(event) => setAdditionalContext(event.target.value)}
              rows={4}
              placeholder="Example: I want to ask about sleep, pain flare-ups, medication side effects and what reports I should request."
              className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
            />
          </label>

          <button
            type="submit"
            disabled={isGenerating || !selectedConditionId}
            className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-cyan-200 disabled:opacity-60"
          >
            {isGenerating ? "Generating..." : "Generate doctor guidance"}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
              Saved doctor guidance drafts
            </p>
            <p className="mt-3 text-sm text-slate-300">
              {selectedCondition
                ? `Showing doctor guidance drafts for ${selectedCondition.conditionName}.`
                : "Select a condition to load doctor guidance drafts."}
            </p>
          </div>

          <button
            type="button"
            onClick={() => loadDrafts()}
            disabled={isLoading || !selectedConditionId}
            className="rounded-xl border border-cyan-300/40 px-5 py-3 text-sm font-semibold text-cyan-100 hover:bg-cyan-300/10 disabled:opacity-60"
          >
            {isLoading ? "Refreshing..." : "Refresh"}
          </button>
        </div>

        {doctorDrafts.length === 0 ? (
          <p className="mt-6 text-sm text-slate-300">
            No doctor guidance drafts are available for this condition yet.
          </p>
        ) : (
          <div className="mt-6 grid gap-4">
            {doctorDrafts.map((draft) => (
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
                      {formatLabel(draft.draftType)}
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
            Review doctor guidance
          </p>

          <h3 className="mt-4 text-xl font-bold text-white">
            {formatLabel(selectedDraft.draftType)}
          </h3>

          <p className="mt-2 text-sm text-slate-400">
            Review status: {selectedDraft.reviewStatus}
          </p>

          <label className="mt-6 grid gap-2">
            <span className="text-sm font-medium text-slate-200">
              Reviewed guidance text
            </span>
            <textarea
              value={reviewText}
              onChange={(event) => setReviewText(event.target.value)}
              rows={20}
              className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 font-mono text-sm leading-6 text-white"
            />
          </label>

          <div className="mt-5 flex flex-wrap gap-3">
            <button
              type="button"
              onClick={handleCopy}
              className="rounded-xl border border-cyan-300/40 px-5 py-3 text-sm font-semibold text-cyan-100 hover:bg-cyan-300/10"
            >
              Copy text
            </button>

            <button
              type="button"
              onClick={() => handleSave("USER_EDITED")}
              disabled={isSaving}
              className="rounded-xl border border-cyan-300/40 px-5 py-3 text-sm font-semibold text-cyan-100 hover:bg-cyan-300/10 disabled:opacity-60"
            >
              Save review
            </button>

            <button
              type="button"
              onClick={() => handleSave("APPROVED")}
              disabled={isSaving}
              className="rounded-xl border border-emerald-300/40 px-5 py-3 text-sm font-semibold text-emerald-100 hover:bg-emerald-300/10 disabled:opacity-60"
            >
              Approve for pack
            </button>

            <button
              type="button"
              onClick={() => handleSave("REJECTED")}
              disabled={isSaving}
              className="rounded-xl border border-yellow-300/40 px-5 py-3 text-sm font-semibold text-yellow-100 hover:bg-yellow-300/10 disabled:opacity-60"
            >
              Reject
            </button>
          </div>
        </section>
      )}
    </div>
  );
}

function formatLabel(value: string) {
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