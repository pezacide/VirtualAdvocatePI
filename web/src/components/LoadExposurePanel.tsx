"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import { DatePickerInput } from "@/components/DatePickerInput";
import { LoadReferenceLookupPanel } from "@/components/LoadReferenceLookupPanel";
import {
  ClaimCondition,
  CreateLoadExposureRecordInput,
  LoadExposureRecord,
  LoadExposureRecordType,
  archiveLoadExposureRecord,
  createLoadExposureRecord,
  getClaimConditions,
  getLoadExposureRecords,
  loadExposureBodyAreas,
  loadExposureFrequencies,
  loadExposureHazardTypes,
  loadExposureRecordTypeLabels,
  loadExposureRecordTypes,
} from "@/lib/api";

type LoadExposurePanelProps = {
  workspaceId: string;
};

const hazardTypeLabels: Record<string, string> = {
  WHOLE_BODY_VIBRATION: "Whole-body vibration",
  AWKWARD_SUSTAINED_POSTURE: "Awkward or sustained posture",
  SUDDEN_UNEXPECTED_LOAD: "Sudden or unexpected load",
  SLIP_TRIP_FALL: "Slip, trip or fall risk",
  CONFINED_SPACE: "Confined space working",
  REPETITIVE_STRAIN: "Repetitive strain",
  OTHER: "Other",
};

function labelFromEnum(value: string) {
  return value
    .toLowerCase()
    .split("_")
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(" ");
}

const emptyForm = {
  recordType: "LIFTING_CARRYING" as LoadExposureRecordType,
  conditionId: "",
  activityDescription: "",
  bodyAreaAffected: "MULTIPLE",
  typicalWeightKg: "",
  maxWeightKg: "",
  frequency: "UNSURE",
  durationPerOccasion: "",
  repetitionsDescription: "",
  servicePeriodFrom: "",
  servicePeriodTo: "",
  yearsExposed: "",
  equipmentOrContext: "",
  hazardType: "OTHER",
  notes: "",
};

export function LoadExposurePanel({ workspaceId }: LoadExposurePanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [records, setRecords] = useState<LoadExposureRecord[]>([]);
  const [form, setForm] = useState(emptyForm);

  const [isLoading, setIsLoading] = useState(false);
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

  async function loadAll() {
    if (loading || !user) {
      return;
    }

    setIsLoading(true);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const [conditionRows, recordRows] = await Promise.all([
        getClaimConditions(token, workspaceId),
        getLoadExposureRecords(token, workspaceId),
      ]);

      setConditions(conditionRows);
      setRecords(recordRows);
    } catch (error) {
      setErrorMessage(
        error instanceof Error ? error.message : "Could not load load exposure records.",
      );
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadAll();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  function updateForm<K extends keyof typeof emptyForm>(key: K, value: (typeof emptyForm)[K]) {
    setForm((current) => ({ ...current, [key]: value }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setStatusMessage("");
    setErrorMessage("");

    if (!form.activityDescription.trim()) {
      setErrorMessage("Describe the activity before saving.");
      return;
    }

    setIsSubmitting(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const input: CreateLoadExposureRecordInput = {
        recordType: form.recordType,
        activityDescription: form.activityDescription.trim(),
        bodyAreaAffected: form.bodyAreaAffected,
        frequency: form.frequency,
        conditionId: form.conditionId || undefined,
        typicalWeightKg: form.typicalWeightKg ? Number(form.typicalWeightKg) : undefined,
        maxWeightKg: form.maxWeightKg ? Number(form.maxWeightKg) : undefined,
        durationPerOccasion: form.durationPerOccasion || undefined,
        repetitionsDescription: form.repetitionsDescription || undefined,
        servicePeriodFrom: form.servicePeriodFrom || undefined,
        servicePeriodTo: form.servicePeriodTo || undefined,
        yearsExposed: form.yearsExposed ? Number(form.yearsExposed) : undefined,
        equipmentOrContext: form.equipmentOrContext || undefined,
        hazardType: form.recordType === "HAZARD_EXPOSURE" ? form.hazardType : undefined,
        notes: form.notes || undefined,
      };

      await createLoadExposureRecord(token, workspaceId, input);

      setForm({ ...emptyForm, recordType: form.recordType, conditionId: form.conditionId });
      setStatusMessage("Load exposure record added.");
      await loadAll();
    } catch (error) {
      setErrorMessage(
        error instanceof Error ? error.message : "Could not add load exposure record.",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleArchive(recordId: string) {
    if (!window.confirm("Remove this load exposure record from the workspace?")) {
      return;
    }

    setArchivingId(recordId);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      await archiveLoadExposureRecord(token, workspaceId, recordId);
      await loadAll();
    } catch (error) {
      setErrorMessage(
        error instanceof Error ? error.message : "Could not remove load exposure record.",
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
        <p className="mt-2 text-sm">Sign in before recording load exposure.</p>
        <Link
          href="/login"
          className="mt-5 inline-flex rounded-xl bg-cyan-300 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-cyan-200"
        >
          Go to login
        </Link>
      </div>
    );
  }

  const isHazard = form.recordType === "HAZARD_EXPOSURE";

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Load exposure builder
        </p>

        <h1 className="mt-4 text-3xl font-bold">Record physically demanding service</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          Add one record per kind of physically demanding activity. Choose the record type, then
          describe what you did, how heavy it was and how often, over which period of service.
        </p>

        <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm leading-6 text-yellow-100">
          Preparation support only. This does not calculate impairment points, assess capacity,
          estimate compensation, provide legal or medical advice, make a DVA decision, or
          guarantee an outcome.
        </div>

        <form onSubmit={handleSubmit} className="mt-8 space-y-6">
          <div className="grid gap-5 md:grid-cols-2">
            <div>
              <label htmlFor="recordType" className="text-sm font-medium text-slate-200">
                Record type
              </label>
              <select
                id="recordType"
                value={form.recordType}
                onChange={(event) =>
                  updateForm("recordType", event.target.value as LoadExposureRecordType)
                }
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              >
                {loadExposureRecordTypes.map((type) => (
                  <option key={type} value={type}>
                    {loadExposureRecordTypeLabels[type]}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="conditionId" className="text-sm font-medium text-slate-200">
                Linked condition (optional)
              </label>
              <select
                id="conditionId"
                value={form.conditionId}
                onChange={(event) => updateForm("conditionId", event.target.value)}
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              >
                <option value="">Not linked to a specific condition</option>
                {conditions.map((condition) => (
                  <option key={condition.id} value={condition.id}>
                    {condition.conditionName}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <label htmlFor="activityDescription" className="text-sm font-medium text-slate-200">
              Activity description
            </label>
            <textarea
              id="activityDescription"
              value={form.activityDescription}
              onChange={(event) => updateForm("activityDescription", event.target.value)}
              rows={3}
              placeholder="Example: loaded and unloaded ammunition crates from vehicles during resupply."
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            />
          </div>

          <div className="grid gap-5 md:grid-cols-2">
            <div>
              <label htmlFor="bodyAreaAffected" className="text-sm font-medium text-slate-200">
                Body area affected
              </label>
              <select
                id="bodyAreaAffected"
                value={form.bodyAreaAffected}
                onChange={(event) => updateForm("bodyAreaAffected", event.target.value)}
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              >
                {loadExposureBodyAreas.map((area) => (
                  <option key={area} value={area}>
                    {labelFromEnum(area)}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="frequency" className="text-sm font-medium text-slate-200">
                Frequency
              </label>
              <select
                id="frequency"
                value={form.frequency}
                onChange={(event) => updateForm("frequency", event.target.value)}
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              >
                {loadExposureFrequencies.map((frequency) => (
                  <option key={frequency} value={frequency}>
                    {labelFromEnum(frequency)}
                  </option>
                ))}
              </select>
            </div>

            {isHazard ? (
              <div>
                <label htmlFor="hazardType" className="text-sm font-medium text-slate-200">
                  Hazard type
                </label>
                <select
                  id="hazardType"
                  value={form.hazardType}
                  onChange={(event) => updateForm("hazardType", event.target.value)}
                  className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
                >
                  {loadExposureHazardTypes.map((hazard) => (
                    <option key={hazard} value={hazard}>
                      {hazardTypeLabels[hazard] ?? labelFromEnum(hazard)}
                    </option>
                  ))}
                </select>
              </div>
            ) : (
              <>
                <div>
                  <label htmlFor="typicalWeightKg" className="text-sm font-medium text-slate-200">
                    Typical weight (kg)
                  </label>
                  <input
                    id="typicalWeightKg"
                    type="number"
                    min="0"
                    step="0.5"
                    value={form.typicalWeightKg}
                    onChange={(event) => updateForm("typicalWeightKg", event.target.value)}
                    className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
                  />
                </div>
                <div>
                  <label htmlFor="maxWeightKg" className="text-sm font-medium text-slate-200">
                    Heaviest weight (kg)
                  </label>
                  <input
                    id="maxWeightKg"
                    type="number"
                    min="0"
                    step="0.5"
                    value={form.maxWeightKg}
                    onChange={(event) => updateForm("maxWeightKg", event.target.value)}
                    className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
                  />
                </div>
              </>
            )}

            <div>
              <label htmlFor="durationPerOccasion" className="text-sm font-medium text-slate-200">
                Duration per occasion
              </label>
              <input
                id="durationPerOccasion"
                type="text"
                value={form.durationPerOccasion}
                onChange={(event) => updateForm("durationPerOccasion", event.target.value)}
                placeholder="Example: 2-3 hours"
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              />
            </div>

            <div>
              <label htmlFor="yearsExposed" className="text-sm font-medium text-slate-200">
                Years exposed
              </label>
              <input
                id="yearsExposed"
                type="number"
                min="0"
                step="0.5"
                value={form.yearsExposed}
                onChange={(event) => updateForm("yearsExposed", event.target.value)}
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              />
            </div>

            <div>
              <label htmlFor="servicePeriodFrom" className="text-sm font-medium text-slate-200">
                Service period from
              </label>
              <DatePickerInput
                id="servicePeriodFrom"
                value={form.servicePeriodFrom}
                onChange={(value) => updateForm("servicePeriodFrom", value)}
              />
            </div>

            <div>
              <label htmlFor="servicePeriodTo" className="text-sm font-medium text-slate-200">
                Service period to
              </label>
              <DatePickerInput
                id="servicePeriodTo"
                value={form.servicePeriodTo}
                onChange={(value) => updateForm("servicePeriodTo", value)}
              />
            </div>
          </div>

          {!isHazard && (
            <div>
              <label htmlFor="repetitionsDescription" className="text-sm font-medium text-slate-200">
                Repetitions / volume
              </label>
              <input
                id="repetitionsDescription"
                type="text"
                value={form.repetitionsDescription}
                onChange={(event) => updateForm("repetitionsDescription", event.target.value)}
                placeholder="Example: about 40 crates per resupply, several times a week"
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              />
            </div>
          )}

          <div>
            <label htmlFor="equipmentOrContext" className="text-sm font-medium text-slate-200">
              Equipment or context
            </label>
            <input
              id="equipmentOrContext"
              type="text"
              value={form.equipmentOrContext}
              onChange={(event) => updateForm("equipmentOrContext", event.target.value)}
              placeholder="Example: patrol pack, jerry cans, damage control pump"
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
              rows={3}
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
            {isSubmitting ? "Adding record..." : "Add load exposure record"}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Saved records
        </p>

        <h2 className="mt-4 text-2xl font-bold">Load exposure records</h2>

        {isLoading ? (
          <p className="mt-6 text-slate-300">Loading records...</p>
        ) : records.length === 0 ? (
          <p className="mt-6 text-slate-300">No load exposure records have been added yet.</p>
        ) : (
          <div className="mt-6 grid gap-4">
            {records.map((record) => {
              const condition = conditions.find((item) => item.id === record.conditionId);

              return (
                <div key={record.id} className="rounded-xl border border-white/10 bg-slate-900 p-5">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <p className="text-xs font-semibold uppercase tracking-wide text-cyan-300">
                        {loadExposureRecordTypeLabels[record.recordType]}
                        {record.hazardType ? ` — ${labelFromEnum(record.hazardType)}` : ""}
                      </p>
                      <p className="mt-1 font-medium text-white">{record.activityDescription}</p>
                    </div>
                    <button
                      type="button"
                      onClick={() => handleArchive(record.id)}
                      disabled={archivingId === record.id}
                      className="rounded-lg border border-red-300/30 bg-red-300/10 px-3 py-1 text-xs font-semibold text-red-100 hover:bg-red-300/20 disabled:opacity-60"
                    >
                      {archivingId === record.id ? "Removing..." : "Remove"}
                    </button>
                  </div>

                  <div className="mt-3 grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                    <p>Body area: {labelFromEnum(record.bodyAreaAffected)}</p>
                    <p>Frequency: {labelFromEnum(record.frequency)}</p>
                    {(record.typicalWeightKg ?? null) !== null && (
                      <p>Typical weight: {record.typicalWeightKg} kg</p>
                    )}
                    {(record.maxWeightKg ?? null) !== null && (
                      <p>Heaviest weight: {record.maxWeightKg} kg</p>
                    )}
                    {record.durationPerOccasion && <p>Duration: {record.durationPerOccasion}</p>}
                    {(record.yearsExposed ?? null) !== null && (
                      <p>Years exposed: {record.yearsExposed}</p>
                    )}
                    {record.equipmentOrContext && <p>Context: {record.equipmentOrContext}</p>}
                    {condition && <p>Condition: {condition.conditionName}</p>}
                  </div>

                  {record.repetitionsDescription && (
                    <p className="mt-3 text-sm text-slate-400">
                      Repetitions: {record.repetitionsDescription}
                    </p>
                  )}

                  {record.notes && (
                    <p className="mt-2 text-sm leading-6 text-slate-400">{record.notes}</p>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </section>

      <LoadReferenceLookupPanel />

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. This builder records user-provided history for organisation. It
        does not verify service, calculate impairment, estimate compensation, or guarantee any
        outcome.
      </section>
    </div>
  );
}
