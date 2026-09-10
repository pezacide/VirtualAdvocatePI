"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  ClaimCondition,
  createQuestionResponse,
  getClaimConditions,
  getQuestionResponses,
  QuestionResponse,
} from "@/lib/api";
import {
  FleQuestion,
  fleAllQuestions,
  fleQuestionGroups,
  fleSafetyBoundary,
} from "@/lib/functionalLoadEvidence/questions";

type FunctionalLoadQuestionPanelProps = {
  workspaceId: string;
};

const KEY_PREFIX = "fle:";

function backendKey(question: FleQuestion) {
  return `${KEY_PREFIX}${question.id}`;
}

function backendQuestionGroup(groupKey: string) {
  if (groupKey === "CURRENT_FUNCTIONAL_CAPACITY") {
    return "FUNCTIONAL_IMPACT";
  }

  if (groupKey === "LOAD_EVIDENCE_PREP") {
    return "EVIDENCE_MISSING";
  }

  return "CLAIM_CONTEXT";
}

function backendAnswerType(question: FleQuestion) {
  // The generic question-responses API does not accept NUMBER; store it as text.
  return question.answerType === "NUMBER" ? "TEXT" : question.answerType;
}

function latestAnswersByKey(responses: QuestionResponse[]) {
  const map: Record<string, string> = {};

  responses
    .filter((response) => response.questionKey.startsWith(KEY_PREFIX))
    .slice()
    .sort((a, b) => {
      const aTime = new Date(a.updatedAt || a.createdAt).getTime();
      const bTime = new Date(b.updatedAt || b.createdAt).getTime();
      return aTime - bTime;
    })
    .forEach((response) => {
      map[response.questionKey.slice(KEY_PREFIX.length)] = response.answerText ?? "";
    });

  return map;
}

export function FunctionalLoadQuestionPanel({ workspaceId }: FunctionalLoadQuestionPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [selectedConditionId, setSelectedConditionId] = useState("");
  const [answers, setAnswers] = useState<Record<string, string>>({});
  const [savedAnswers, setSavedAnswers] = useState<Record<string, string>>({});

  const [isLoadingConditions, setIsLoadingConditions] = useState(false);
  const [isLoadingAnswers, setIsLoadingAnswers] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
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
      setErrorMessage(error instanceof Error ? error.message : "Could not load conditions.");
    } finally {
      setIsLoadingConditions(false);
    }
  }

  async function loadAnswers(conditionId: string) {
    if (loading || !user || !conditionId) {
      return;
    }

    setIsLoadingAnswers(true);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const responses = await getQuestionResponses(token, workspaceId, conditionId);
      const map = latestAnswersByKey(responses);
      setAnswers(map);
      setSavedAnswers(map);
    } catch (error) {
      setErrorMessage(
        error instanceof Error ? error.message : "Could not load saved answers.",
      );
    } finally {
      setIsLoadingAnswers(false);
    }
  }

  useEffect(() => {
    loadConditions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  useEffect(() => {
    if (selectedConditionId) {
      loadAnswers(selectedConditionId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedConditionId]);

  function setAnswer(questionId: string, value: string) {
    setAnswers((current) => ({ ...current, [questionId]: value }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setStatusMessage("");
    setErrorMessage("");

    if (!selectedConditionId) {
      setErrorMessage("Select a condition first.");
      return;
    }

    const changed = fleAllQuestions.filter((question) => {
      const value = (answers[question.id] ?? "").trim();
      return value.length > 0 && value !== (savedAnswers[question.id] ?? "");
    });

    if (changed.length === 0) {
      setStatusMessage("No changes to save.");
      return;
    }

    setIsSaving(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const groupOfQuestion = new Map<string, string>();
      fleQuestionGroups.forEach((group) => {
        group.questions.forEach((question) => groupOfQuestion.set(question.id, group.groupKey));
      });

      for (const question of changed) {
        await createQuestionResponse(token, workspaceId, selectedConditionId, {
          questionGroup: backendQuestionGroup(groupOfQuestion.get(question.id) ?? ""),
          questionKey: backendKey(question),
          questionText: question.questionText,
          answerText: (answers[question.id] ?? "").trim(),
          answerType: backendAnswerType(question),
        });
      }

      setStatusMessage(`Saved ${changed.length} answer${changed.length === 1 ? "" : "s"}.`);
      await loadAnswers(selectedConditionId);
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : "Could not save answers.");
    } finally {
      setIsSaving(false);
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
        <p className="mt-2 text-sm">Sign in before answering the question bank.</p>
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
        <p className="mt-2 text-sm">Question bank answers are saved against a specific condition.</p>
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
    <form onSubmit={handleSubmit} className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Functional and load evidence question bank
        </p>

        <h1 className="mt-4 text-3xl font-bold">Answer the preparation questions</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          These questions help you describe physically demanding service and current function in
          your own words. Answer what you can; you can come back and add more later.
        </p>

        <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm leading-6 text-yellow-100">
          {fleSafetyBoundary}
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
      </section>

      {isLoadingAnswers ? (
        <div className="rounded-2xl border border-white/10 bg-white/5 p-6 text-slate-300">
          Loading saved answers...
        </div>
      ) : (
        fleQuestionGroups.map((group) => (
          <section key={group.groupKey} className="rounded-2xl border border-white/10 bg-white/5 p-8">
            <h2 className="text-2xl font-bold">{group.title}</h2>
            <p className="mt-3 max-w-3xl text-slate-300">{group.description}</p>

            <div className="mt-6 space-y-5">
              {group.questions.map((question) => (
                <div key={question.id}>
                  <label htmlFor={question.id} className="text-sm font-medium text-slate-200">
                    {question.questionText}
                    {question.required && <span className="text-cyan-300"> *</span>}
                  </label>

                  {question.helperText && (
                    <p className="mt-1 text-xs text-slate-400">{question.helperText}</p>
                  )}

                  {question.answerType === "LONG_TEXT" ? (
                    <textarea
                      id={question.id}
                      value={answers[question.id] ?? ""}
                      onChange={(event) => setAnswer(question.id, event.target.value)}
                      rows={3}
                      className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
                    />
                  ) : question.answerType === "SINGLE_SELECT" || question.answerType === "YES_NO_UNSURE" ? (
                    <select
                      id={question.id}
                      value={answers[question.id] ?? ""}
                      onChange={(event) => setAnswer(question.id, event.target.value)}
                      className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
                    >
                      <option value="">Select an answer</option>
                      {(question.options ?? []).map((option) => (
                        <option key={option.value} value={option.value}>
                          {option.label}
                        </option>
                      ))}
                    </select>
                  ) : (
                    <input
                      id={question.id}
                      type={
                        question.answerType === "DATE"
                          ? "date"
                          : question.answerType === "NUMBER"
                            ? "number"
                            : "text"
                      }
                      value={answers[question.id] ?? ""}
                      onChange={(event) => setAnswer(question.id, event.target.value)}
                      className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
                    />
                  )}
                </div>
              ))}
            </div>
          </section>
        ))
      )}

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
        disabled={isSaving}
        className="w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
      >
        {isSaving ? "Saving answers..." : "Save answers"}
      </button>

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. These answers record user-provided information for organisation.
        They do not calculate impairment, assess capacity, provide medical advice, or guarantee an
        outcome.
      </section>
    </form>
  );
}
