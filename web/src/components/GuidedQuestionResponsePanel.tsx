"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  ClaimCondition,
  QuestionResponse,
  createQuestionResponse,
  getClaimConditions,
  getQuestionResponses,
} from "@/lib/apiClient";

type GuidedQuestionResponsePanelProps = {
  workspaceId: string;
};

const questionGroups = [
  "CONDITION_DETAILS",
  "SYMPTOMS",
  "TREATMENT",
  "MEDICATION",
  "FUNCTIONAL_IMPACT",
  "LIFESTYLE_IMPACT",
  "WORK_IMPACT",
  "STABILITY",
  "WORSENING",
  "SERVICE_CONNECTION",
  "EVIDENCE",
  "APPOINTMENT_PREP",
  "OTHER",
];

const answerTypes = ["TEXT", "YES_NO", "YES_NO_UNSURE", "DATE", "NUMBER"];

export function GuidedQuestionResponsePanel({
  workspaceId,
}: GuidedQuestionResponsePanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [selectedConditionId, setSelectedConditionId] = useState("");
  const [responses, setResponses] = useState<QuestionResponse[]>([]);

  const [questionGroup, setQuestionGroup] = useState("SYMPTOMS");
  const [questionKey, setQuestionKey] = useState("symptoms_impact");
  const [questionText, setQuestionText] = useState(
    "How does this condition affect daily life?",
  );
  const [answerType, setAnswerType] = useState("TEXT");
  const [answerText, setAnswerText] = useState("");

  const [isLoadingConditions, setIsLoadingConditions] = useState(false);
  const [isLoadingResponses, setIsLoadingResponses] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

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

  async function loadResponses(conditionId: string) {
    if (loading || !user || !conditionId) {
      return;
    }

    setIsLoadingResponses(true);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = await getQuestionResponses(token, workspaceId, conditionId);
      setResponses(rows);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Could not load guided question responses.";
      setErrorMessage(message);
    } finally {
      setIsLoadingResponses(false);
    }
  }

  useEffect(() => {
    loadConditions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  useEffect(() => {
    if (selectedConditionId) {
      loadResponses(selectedConditionId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedConditionId]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setStatusMessage("");
    setErrorMessage("");
    setIsSubmitting(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      if (!selectedConditionId) {
        setErrorMessage("Select a condition before saving a guided question response.");
        return;
      }

      await createQuestionResponse(token, workspaceId, selectedConditionId, {
        questionGroup,
        questionKey,
        questionText,
        answerText: answerText || undefined,
        answerType,
      });

      setAnswerText("");
      setStatusMessage("Guided question response saved.");
      await loadResponses(selectedConditionId);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Could not save guided question response.";

      setErrorMessage(message);
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
          Sign in before adding guided question responses.
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
          Guided question responses are recorded against a specific condition.
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

  const selectedCondition = conditions.find((condition) => condition.id === selectedConditionId);

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Guided questions
        </p>

        <h1 className="mt-4 text-3xl font-bold">Add a guided question response</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          Capture structured answers that can later support evidence checks, doctor questions,
          AI draft metadata and generated preparation documents.
        </p>

        <form onSubmit={handleSubmit} className="mt-8 space-y-6">
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
              {conditions.map((condition) => (
                <option key={condition.id} value={condition.id}>
                  {condition.conditionName}
                </option>
              ))}
            </select>
          </div>

          <div className="grid gap-5 md:grid-cols-2">
            <div>
              <label htmlFor="questionGroup" className="text-sm font-medium text-slate-200">
                Question group
              </label>
              <select
                id="questionGroup"
                value={questionGroup}
                onChange={(event) => setQuestionGroup(event.target.value)}
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              >
                {questionGroups.map((group) => (
                  <option key={group} value={group}>
                    {group}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="answerType" className="text-sm font-medium text-slate-200">
                Answer type
              </label>
              <select
                id="answerType"
                value={answerType}
                onChange={(event) => setAnswerType(event.target.value)}
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              >
                {answerTypes.map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <label htmlFor="questionKey" className="text-sm font-medium text-slate-200">
              Question key
            </label>
            <input
              id="questionKey"
              type="text"
              value={questionKey}
              onChange={(event) => setQuestionKey(event.target.value)}
              required
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            />
          </div>

          <div>
            <label htmlFor="questionText" className="text-sm font-medium text-slate-200">
              Question text
            </label>
            <textarea
              id="questionText"
              value={questionText}
              onChange={(event) => setQuestionText(event.target.value)}
              required
              rows={3}
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            />
          </div>

          <div>
            <label htmlFor="answerText" className="text-sm font-medium text-slate-200">
              Answer
            </label>
            <textarea
              id="answerText"
              value={answerText}
              onChange={(event) => setAnswerText(event.target.value)}
              rows={5}
              placeholder="Write the plain-English answer here."
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            />
          </div>

          {statusMessage && (
            <div className="rounded-xl border border-green-300/30 bg-green-300/10 p-4 text-sm text-green-100">
              {statusMessage}
            </div>
          )}

          {errorMessage && (
            <div className="rounded-xl border border-red-300/30 bg-red-300/10 p-4 text-sm text-red-100">
              {errorMessage}
            </div>
          )}

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
          >
            {isSubmitting ? "Saving response..." : "Save guided question response"}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Saved responses
        </p>

        <h2 className="mt-4 text-2xl font-bold">
          {selectedCondition?.conditionName ?? "Selected condition"}
        </h2>

        {isLoadingResponses ? (
          <p className="mt-6 text-slate-300">Loading responses...</p>
        ) : responses.length === 0 ? (
          <p className="mt-6 text-slate-300">
            No guided question responses have been saved for this condition yet.
          </p>
        ) : (
          <div className="mt-6 grid gap-4">
            {responses.map((response) => (
              <div key={response.id} className="rounded-xl border border-white/10 bg-slate-900 p-5">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <p className="font-mono text-xs text-cyan-200">
                      {response.questionGroup} · {response.questionKey}
                    </p>
                    <h3 className="mt-2 font-semibold">{response.questionText}</h3>
                  </div>

                  <span className="rounded-xl border border-white/10 px-3 py-2 text-xs text-slate-300">
                    {response.answerType}
                  </span>
                </div>

                {response.answerText && (
                  <p className="mt-4 text-sm leading-6 text-slate-400">
                    {response.answerText}
                  </p>
                )}
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. Guided questions help organise user-provided information.
        They do not provide legal advice, medical advice, a DVA decision, a compensation estimate,
        or a guarantee of claim success.
      </section>
    </div>
  );
}