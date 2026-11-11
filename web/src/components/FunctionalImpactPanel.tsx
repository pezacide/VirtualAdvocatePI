"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  ClaimCondition,
  CreateFunctionalImpactEntryInput,
  FunctionalImpactEntry,
  archiveFunctionalImpactEntry,
  createFunctionalImpactEntry,
  functionalImpactActivityDomains,
  functionalImpactFrequencies,
  getClaimConditions,
  getFunctionalImpactEntries,
} from "@/lib/api";

type FunctionalImpactPanelProps = {
  workspaceId: string;
};

function labelFromEnum(value: string) {
  return value
    .toLowerCase()
    .split("_")
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(" ");
}

const emptyForm = {
  activityDomain: "SELF_CARE",
  goodDayDescription: "",
  badDayDescription: "",
  badDayFrequency: "WEEKLY",
  aidsOrHelpUsed: "",
  notes: "",
};

export function FunctionalImpactPanel({ workspaceId }: FunctionalImpactPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [selectedConditionId, setSelectedConditionId] = useState("");
  const [entries, setEntries] = useState<FunctionalImpactEntry[]>([]);
  const [form, setForm] = useState(emptyForm);

  const [isLoadingConditions, setIsLoadingConditions] = useState(false);
  const [isLoadingEntries, setIsLoadingEntries] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [archivingId, setArchivingId] = useState("");
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
      setErrorMessage(
        error instanceof Error ? error.message : "Could not load conditions.",
      );
    } finally {
      setIsLoadingConditions(false);
    }
  }

  async function loadEntries(conditionId: string) {
    if (loading || !user || !conditionId) {
      return;
    }

    setIsLoadingEntries(true);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = await getFunctionalImpactEntries(token, workspaceId, conditionId);
      setEntries(rows);
    } catch (error) {
      setErrorMessage(
        error instanceof Error ? error.message : "Could not load functional impact entries.",
      );
    } finally {
      setIsLoadingEntries(false);
    }
  }

  useEffect(() => {
    loadConditions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  useEffect(() => {
    if (selectedConditionId) {
      loadEntries(selectedConditionId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedConditionId]);

  function updateForm<K extends keyof typeof emptyForm>(key: K, value: string) {
    setForm((current) => ({ ...current, [key]: value }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setStatusMessage("");
    setErrorMessage("");

    if (!selectedConditionId) {
      setErrorMessage("Select a condition first.");
      return;
    }

    if (!form.goodDayDescription.trim() && !form.badDayDescription.trim()) {
      setErrorMessage("Describe a good day, a bad day, or both.");
      return;
    }

    setIsSubmitting(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const input: CreateFunctionalImpactEntryInput = {
        activityDomain: form.activityDomain,
        goodDayDescription: form.goodDayDescription || undefined,
        badDayDescription: form.badDayDescription || undefined,
        badDayFrequency: form.badDayFrequency,
        aidsOrHelpUsed: form.aidsOrHelpUsed || undefined,
        notes: form.notes || undefined,
      };

      await createFunctionalImpactEntry(token, workspaceId, selectedConditionId, input);

      setForm({ ...emptyForm, activityDomain: form.activityDomain });
      setStatusMessage("Functional impact entry added.");
      await loadEntries(selectedConditionId);
    } catch (error) {
      setErrorMessage(
        error instanceof Error ? error.message : "Could not add functional impact entry.",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleArchive(entryId: string) {
    if (!window.confirm("Remove this functional impact entry?")) {
      return;
    }

    setArchivingId(entryId);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      await archiveFunctionalImpactEntry(token, workspaceId, selectedConditionId, entryId);
      await loadEntries(selectedConditionId);
    } catch (error) {
      setErrorMessage(
        error instanceof Error ? error.message : "Could not remove functional impact entry.",
      );
    } finally {
      setArchivingId("");
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
        <p className="mt-2 text-sm">Sign in before recording functional impact.</p>
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
          Good day / bad day entries are recorded against a specific condition.
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
          Good day / bad day builder
        </p>

        <h1 className="mt-4 text-3xl font-bold">Describe functional impact</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          For each activity area, describe what a typical good day looks like and what a bad day
          looks like, and how often bad days happen. Use plain, specific examples.
        </p>

        <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm leading-6 text-yellow-100">
          Preparation support only. This does not assess capacity or impairment, provide medical
          advice, or guarantee an outcome.
        </div>

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
              <label htmlFor="activityDomain" className="text-sm font-medium text-slate-200">
                Activity area
              </label>
              <select
                id="activityDomain"
                value={form.activityDomain}
                onChange={(event) => updateForm("activityDomain", event.target.value)}
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              >
                {functionalImpactActivityDomains.map((domain) => (
                  <option key={domain} value={domain}>
                    {labelFromEnum(domain)}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="badDayFrequency" className="text-sm font-medium text-slate-200">
                How often bad days happen
              </label>
              <select
                id="badDayFrequency"
                value={form.badDayFrequency}
                onChange={(event) => updateForm("badDayFrequency", event.target.value)}
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              >
                {functionalImpactFrequencies.map((frequency) => (
                  <option key={frequency} value={frequency}>
                    {labelFromEnum(frequency)}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <label htmlFor="goodDayDescription" className="text-sm font-medium text-slate-200">
              On a good day
            </label>
            <textarea
              id="goodDayDescription"
              value={form.goodDayDescription}
              onChange={(event) => updateForm("goodDayDescription", event.target.value)}
              rows={3}
              placeholder="What you can manage on a good day, and any cost afterwards."
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            />
          </div>

          <div>
            <label htmlFor="badDayDescription" className="text-sm font-medium text-slate-200">
              On a bad day
            </label>
            <textarea
              id="badDayDescription"
              value={form.badDayDescription}
              onChange={(event) => updateForm("badDayDescription", event.target.value)}
              rows={3}
              placeholder="What you cannot do, what you avoid, and what help you need."
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            />
          </div>

          <div>
            <label htmlFor="aidsOrHelpUsed" className="text-sm font-medium text-slate-200">
              Aids, equipment or help used
            </label>
            <textarea
              id="aidsOrHelpUsed"
              value={form.aidsOrHelpUsed}
              onChange={(event) => updateForm("aidsOrHelpUsed", event.target.value)}
              rows={2}
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            />
          </div>

          <div>
            <label htmlFor="notes" className="text-sm font-medium text-slate-200">
              Notes
            </label>
            <textarea
              id="notes"
              value={form.notes}
              onChange={(event) => updateForm("notes", event.target.value)}
              rows={2}
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
            {isSubmitting ? "Adding entry..." : "Add good day / bad day entry"}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Saved entries
        </p>

        <h2 className="mt-4 text-2xl font-bold">
          {selectedCondition?.conditionName ?? "Selected condition"}
        </h2>

        {isLoadingEntries ? (
          <p className="mt-6 text-slate-300">Loading entries...</p>
        ) : entries.length === 0 ? (
          <p className="mt-6 text-slate-300">
            No good day / bad day entries have been recorded for this condition yet.
          </p>
        ) : (
          <div className="mt-6 grid gap-4">
            {entries.map((entry) => (
              <div key={entry.id} className="rounded-xl border border-white/10 bg-slate-900 p-5">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <p className="text-xs font-semibold uppercase tracking-wide text-cyan-300">
                    {labelFromEnum(entry.activityDomain)} — bad days {labelFromEnum(entry.badDayFrequency)}
                  </p>
                  <button
                    type="button"
                    onClick={() => handleArchive(entry.id)}
                    disabled={archivingId === entry.id}
                    className="rounded-lg border border-red-300/30 bg-red-300/10 px-3 py-1 text-xs font-semibold text-red-100 hover:bg-red-300/20 disabled:opacity-60"
                  >
                    {archivingId === entry.id ? "Removing..." : "Remove"}
                  </button>
                </div>

                {entry.goodDayDescription && (
                  <p className="mt-3 text-sm leading-6 text-slate-300">
                    <span className="font-semibold text-slate-200">Good day: </span>
                    {entry.goodDayDescription}
                  </p>
                )}

                {entry.badDayDescription && (
                  <p className="mt-2 text-sm leading-6 text-slate-300">
                    <span className="font-semibold text-slate-200">Bad day: </span>
                    {entry.badDayDescription}
                  </p>
                )}

                {entry.aidsOrHelpUsed && (
                  <p className="mt-2 text-sm leading-6 text-slate-400">
                    Aids / help: {entry.aidsOrHelpUsed}
                  </p>
                )}

                {entry.notes && (
                  <p className="mt-2 text-sm leading-6 text-slate-400">{entry.notes}</p>
                )}
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. These entries record user-provided information for organisation.
        They do not assess capacity or impairment, provide medical advice, or guarantee an
        outcome.
      </section>
    </div>
  );
}
