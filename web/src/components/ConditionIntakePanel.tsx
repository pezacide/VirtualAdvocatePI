"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  ClaimCondition,
  createClaimCondition,
  getClaimConditions,
} from "@/lib/apiClient";

type ConditionIntakePanelProps = {
  workspaceId: string;
};

const diagnosisOptions = [
  "DIAGNOSED",
  "PROVISIONAL_DIAGNOSIS",
  "SELF_REPORTED",
  "NOT_YET_DIAGNOSED",
  "UNSURE",
];

export function ConditionIntakePanel({ workspaceId }: ConditionIntakePanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [conditionName, setConditionName] = useState("");
  const [diagnosisStatus, setDiagnosisStatus] = useState("DIAGNOSED");
  const [dateDiagnosed, setDateDiagnosed] = useState("");
  const [currentSymptoms, setCurrentSymptoms] = useState("");
  const [treatmentSummary, setTreatmentSummary] = useState("");
  const [medicationSummary, setMedicationSummary] = useState("");
  const [functionalImpactSummary, setFunctionalImpactSummary] = useState("");
  const [isPrimaryCondition, setIsPrimaryCondition] = useState(true);

  const [isLoadingConditions, setIsLoadingConditions] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  async function loadConditions() {
    if (loading || !user) {
      return;
    }

    setIsLoadingConditions(true);
    setErrorMessage("");

    try {
      const token = await getIdToken();

      if (!token) {
        setErrorMessage("No Firebase ID token is available. Please sign in again.");
        return;
      }

      const rows = await getClaimConditions(token, workspaceId);
      setConditions(rows);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Could not load conditions.";
      setErrorMessage(message);
    } finally {
      setIsLoadingConditions(false);
    }
  }

  useEffect(() => {
    loadConditions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setStatusMessage("");
    setErrorMessage("");
    setIsSubmitting(true);

    try {
      const token = await getIdToken();

      if (!token) {
        setErrorMessage("You need to sign in before adding a condition.");
        return;
      }

      await createClaimCondition(token, workspaceId, {
        conditionName,
        diagnosisStatus,
        dateDiagnosed: dateDiagnosed || undefined,
        currentSymptoms: currentSymptoms || undefined,
        treatmentSummary: treatmentSummary || undefined,
        medicationSummary: medicationSummary || undefined,
        functionalImpactSummary: functionalImpactSummary || undefined,
        isPrimaryCondition,
      });

      setConditionName("");
      setDiagnosisStatus("DIAGNOSED");
      setDateDiagnosed("");
      setCurrentSymptoms("");
      setTreatmentSummary("");
      setMedicationSummary("");
      setFunctionalImpactSummary("");
      setIsPrimaryCondition(false);

      setStatusMessage("Condition added.");
      await loadConditions();
    } catch (error) {
      const message = error instanceof Error ? error.message : "Could not add condition.";
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
          Sign in before adding conditions to this claim preparation workspace.
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

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Condition intake
        </p>

        <h1 className="mt-4 text-3xl font-bold">Add a condition</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          Capture the plain-English details needed to organise evidence and prepare later
          sections of the claim starter pack.
        </p>

        <form onSubmit={handleSubmit} className="mt-8 space-y-6">
          <div>
            <label htmlFor="conditionName" className="text-sm font-medium text-slate-200">
              Condition name
            </label>
            <input
              id="conditionName"
              type="text"
              value={conditionName}
              onChange={(event) => setConditionName(event.target.value)}
              required
              placeholder="Example: tinnitus, PTSD, lumbar spine condition"
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            />
          </div>

          <div className="grid gap-5 md:grid-cols-2">
            <div>
              <label htmlFor="diagnosisStatus" className="text-sm font-medium text-slate-200">
                Diagnosis status
              </label>
              <select
                id="diagnosisStatus"
                value={diagnosisStatus}
                onChange={(event) => setDiagnosisStatus(event.target.value)}
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              >
                {diagnosisOptions.map((option) => (
                  <option key={option} value={option}>
                    {option}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="dateDiagnosed" className="text-sm font-medium text-slate-200">
                Date diagnosed
              </label>
              <input
                id="dateDiagnosed"
                type="date"
                value={dateDiagnosed}
                onChange={(event) => setDateDiagnosed(event.target.value)}
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              />
            </div>
          </div>

          <div>
            <label htmlFor="currentSymptoms" className="text-sm font-medium text-slate-200">
              Current symptoms
            </label>
            <textarea
              id="currentSymptoms"
              value={currentSymptoms}
              onChange={(event) => setCurrentSymptoms(event.target.value)}
              rows={4}
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            />
          </div>

          <div>
            <label htmlFor="treatmentSummary" className="text-sm font-medium text-slate-200">
              Treatment summary
            </label>
            <textarea
              id="treatmentSummary"
              value={treatmentSummary}
              onChange={(event) => setTreatmentSummary(event.target.value)}
              rows={3}
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            />
          </div>

          <div>
            <label htmlFor="medicationSummary" className="text-sm font-medium text-slate-200">
              Medication summary
            </label>
            <textarea
              id="medicationSummary"
              value={medicationSummary}
              onChange={(event) => setMedicationSummary(event.target.value)}
              rows={3}
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            />
          </div>

          <div>
            <label htmlFor="functionalImpactSummary" className="text-sm font-medium text-slate-200">
              Functional impact summary
            </label>
            <textarea
              id="functionalImpactSummary"
              value={functionalImpactSummary}
              onChange={(event) => setFunctionalImpactSummary(event.target.value)}
              rows={4}
              placeholder="Example: work, sleep, mobility, relationships, daily tasks, flare-ups"
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            />
          </div>

          <label className="flex items-center gap-3 rounded-xl border border-white/10 bg-slate-900 p-4 text-sm text-slate-200">
            <input
              type="checkbox"
              checked={isPrimaryCondition}
              onChange={(event) => setIsPrimaryCondition(event.target.checked)}
            />
            Mark as a primary condition for this workspace.
          </label>

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
            {isSubmitting ? "Adding condition..." : "Add condition"}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Conditions in this workspace
        </p>

        {isLoadingConditions ? (
          <p className="mt-6 text-slate-300">Loading conditions...</p>
        ) : conditions.length === 0 ? (
          <p className="mt-6 text-slate-300">No conditions have been added yet.</p>
        ) : (
          <div className="mt-6 grid gap-4">
            {conditions.map((condition) => (
              <div
                key={condition.id}
                className="rounded-xl border border-white/10 bg-slate-900 p-5"
              >
                <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <h2 className="text-xl font-semibold">{condition.conditionName}</h2>
                    <p className="mt-2 text-sm text-slate-300">
                      Diagnosis status: {condition.diagnosisStatus}
                    </p>
                    {condition.currentSymptoms && (
                      <p className="mt-3 text-sm leading-6 text-slate-400">
                        {condition.currentSymptoms}
                      </p>
                    )}
                  </div>

                  <div className="rounded-xl border border-cyan-300/30 bg-cyan-300/10 px-3 py-2 text-sm text-cyan-100">
                    {condition.isPrimaryCondition ? "PRIMARY" : "SECONDARY"}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. This condition intake page does not provide medical advice,
        diagnose a condition, submit material to DVA, or guarantee a claim outcome.
      </section>
    </div>
  );
}