"use client";

import { DatePickerInput } from "@/components/DatePickerInput";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  AcceptedConditionHistory,
  ClaimCondition,
  createAcceptedConditionHistory,
  getAcceptedConditionHistory,
  getClaimConditions,
} from "@/lib/api";

type AcceptedConditionHistoryPanelProps = {
  workspaceId: string;
};

const yesNoUnsureOptions = ["YES", "NO", "UNSURE"];
const originalActOptions = ["MRCA", "DRCA", "VEA", "UNSURE"];

export function AcceptedConditionHistoryPanel({
  workspaceId,
}: AcceptedConditionHistoryPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [selectedConditionId, setSelectedConditionId] = useState("");
  const [historyRows, setHistoryRows] = useState<AcceptedConditionHistory[]>([]);

  const [previouslyAcceptedByDva, setPreviouslyAcceptedByDva] = useState("UNSURE");
  const [originalAct, setOriginalAct] = useState("UNSURE");
  const [previousCompensationReceived, setPreviousCompensationReceived] =
    useState("UNSURE");
  const [
    previousDvaDecisionLetterAvailable,
    setPreviousDvaDecisionLetterAvailable,
  ] = useState("UNSURE");
  const [
    previousAssessmentLetterAvailable,
    setPreviousAssessmentLetterAvailable,
  ] = useState("UNSURE");
  const [previousDecisionDate, setPreviousDecisionDate] = useState("");
  const [previousAssessmentDate, setPreviousAssessmentDate] = useState("");
  const [worseningClaimed, setWorseningClaimed] = useState("UNSURE");
  const [worseningSummary, setWorseningSummary] = useState("");

  const [isLoadingConditions, setIsLoadingConditions] = useState(false);
  const [isLoadingHistory, setIsLoadingHistory] = useState(false);
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

  async function loadHistory(conditionId: string) {
    if (loading || !user || !conditionId) {
      return;
    }

    setIsLoadingHistory(true);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = await getAcceptedConditionHistory(token, workspaceId, conditionId);
      setHistoryRows(rows);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Could not load accepted-condition history.";
      setErrorMessage(message);
    } finally {
      setIsLoadingHistory(false);
    }
  }

  useEffect(() => {
    loadConditions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  useEffect(() => {
    if (selectedConditionId) {
      loadHistory(selectedConditionId);
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
        setErrorMessage("Select a condition before saving accepted-condition history.");
        return;
      }

      await createAcceptedConditionHistory(token, workspaceId, selectedConditionId, {
        previouslyAcceptedByDva,
        originalAct,
        previousCompensationReceived,
        previousDvaDecisionLetterAvailable,
        previousAssessmentLetterAvailable,
        previousDecisionDate: previousDecisionDate || undefined,
        previousAssessmentDate: previousAssessmentDate || undefined,
        worseningClaimed,
        worseningSummary: worseningSummary || undefined,
      });

      setPreviousDecisionDate("");
      setPreviousAssessmentDate("");
      setWorseningSummary("");

      setStatusMessage("Accepted-condition history saved.");
      await loadHistory(selectedConditionId);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Could not save accepted-condition history.";

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
          Sign in before adding accepted-condition history.
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
          Accepted-condition history is recorded against a specific condition.
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
          Accepted-condition history
        </p>

        <h1 className="mt-4 text-3xl font-bold">Record previous DVA history</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          Capture whether a condition has previously been accepted or compensated, and whether
          previous DVA letters or assessment material are available.
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
            <SelectField
              id="previouslyAcceptedByDva"
              label="Previously accepted by DVA"
              value={previouslyAcceptedByDva}
              options={yesNoUnsureOptions}
              onChange={setPreviouslyAcceptedByDva}
            />

            <SelectField
              id="originalAct"
              label="Original Act"
              value={originalAct}
              options={originalActOptions}
              onChange={setOriginalAct}
            />

            <SelectField
              id="previousCompensationReceived"
              label="Previous compensation received"
              value={previousCompensationReceived}
              options={yesNoUnsureOptions}
              onChange={setPreviousCompensationReceived}
            />

            <SelectField
              id="previousDvaDecisionLetterAvailable"
              label="DVA decision letter available"
              value={previousDvaDecisionLetterAvailable}
              options={yesNoUnsureOptions}
              onChange={setPreviousDvaDecisionLetterAvailable}
            />

            <SelectField
              id="previousAssessmentLetterAvailable"
              label="Assessment letter available"
              value={previousAssessmentLetterAvailable}
              options={yesNoUnsureOptions}
              onChange={setPreviousAssessmentLetterAvailable}
            />

            <SelectField
              id="worseningClaimed"
              label="Worsening claimed"
              value={worseningClaimed}
              options={yesNoUnsureOptions}
              onChange={setWorseningClaimed}
            />
          </div>

          <div className="grid gap-5 md:grid-cols-2">
            <div>
              <label
                htmlFor="previousDecisionDate"
                className="text-sm font-medium text-slate-200"
              >
                Previous decision date
              </label>
              <DatePickerInput id="previousDecisionDate" value={previousDecisionDate} onChange={setPreviousDecisionDate} />
            </div>

            <div>
              <label
                htmlFor="previousAssessmentDate"
                className="text-sm font-medium text-slate-200"
              >
                Previous assessment date
              </label>
              <DatePickerInput id="previousAssessmentDate" value={previousAssessmentDate} onChange={setPreviousAssessmentDate} />
            </div>
          </div>

          <div>
            <label htmlFor="worseningSummary" className="text-sm font-medium text-slate-200">
              Worsening summary
            </label>
            <textarea
              id="worseningSummary"
              value={worseningSummary}
              onChange={(event) => setWorseningSummary(event.target.value)}
              rows={4}
              placeholder="Example: symptoms have become more frequent, treatment has increased, or functional impact has changed."
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
            {isSubmitting ? "Saving history..." : "Save accepted-condition history"}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Saved history
        </p>

        <h2 className="mt-4 text-2xl font-bold">
          {selectedCondition?.conditionName ?? "Selected condition"}
        </h2>

        {isLoadingHistory ? (
          <p className="mt-6 text-slate-300">Loading history...</p>
        ) : historyRows.length === 0 ? (
          <p className="mt-6 text-slate-300">
            No accepted-condition history has been recorded for this condition yet.
          </p>
        ) : (
          <div className="mt-6 grid gap-4">
            {historyRows.map((row) => (
              <div key={row.id} className="rounded-xl border border-white/10 bg-slate-900 p-5">
                <div className="grid gap-3 text-sm text-slate-300 md:grid-cols-2">
                  <p>Previously accepted: {row.previouslyAcceptedByDva}</p>
                  <p>Original Act: {row.originalAct}</p>
                  <p>Previous compensation: {row.previousCompensationReceived}</p>
                  <p>DVA decision letter: {row.previousDvaDecisionLetterAvailable}</p>
                  <p>Assessment letter: {row.previousAssessmentLetterAvailable}</p>
                  <p>Worsening claimed: {row.worseningClaimed}</p>
                </div>

                {row.worseningSummary && (
                  <p className="mt-4 text-sm leading-6 text-slate-400">
                    {row.worseningSummary}
                  </p>
                )}
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. This page records user-provided history for organisation only.
        It does not confirm DVA acceptance, provide legal advice, provide medical advice, estimate
        compensation, or guarantee any outcome.
      </section>
    </div>
  );
}

type SelectFieldProps = {
  id: string;
  label: string;
  value: string;
  options: string[];
  onChange: (value: string) => void;
};

function SelectField({ id, label, value, options, onChange }: SelectFieldProps) {
  return (
    <div>
      <label htmlFor={id} className="text-sm font-medium text-slate-200">
        {label}
      </label>
      <select
        id={id}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
      >
        {options.map((option) => (
          <option key={option} value={option}>
            {option}
          </option>
        ))}
      </select>
    </div>
  );
}