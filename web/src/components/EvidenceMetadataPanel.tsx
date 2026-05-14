"use client";

import { DatePickerInput } from "@/components/DatePickerInput";
import { EvidenceListSummaryPanel } from "@/components/EvidenceListSummaryPanel";
import {
  evidenceSourceQuickTags,
  evidenceTypeOptions,
  evidenceStatusOptions,
  getEvidenceStatusLabel,
  getEvidenceTypeCategory,
  getEvidenceTypeLabel,
} from "@/lib/evidenceUi";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  ClaimCondition,
  EvidenceItem,
  createEvidenceItem,
  updateEvidenceStatus,
  archiveEvidenceItem,
  deleteEvidenceUploadedFile,
  getClaimConditions,
  getConditionEvidenceItems,
} from "@/lib/api";

type EvidenceMetadataPanelProps = {
  workspaceId: string;
};

const evidenceTypes = [
  "DVA_DECISION_LETTER",
  "PREVIOUS_PI_ASSESSMENT",
  "DCP_ASSESSMENT",
  "MEDICAL_REPORT",
  "SPECIALIST_REPORT",
  "IMAGING_REPORT",
  "MEDICATION_LIST",
  "TREATMENT_SUMMARY",
  "SERVICE_DOCUMENT",
  "PERSONAL_STATEMENT",
  "FUNCTIONAL_IMPACT_NOTES",
  "APPOINTMENT_NOTES",
  "OTHER",
];

const evidenceStatuses = [
  "MISSING",
  "LISTED_NOT_UPLOADED",
  "UPLOADED",
  "REVIEWED",
  "CONFIRMED",
  "NOT_APPLICABLE",
];

export function EvidenceMetadataPanel({ workspaceId }: EvidenceMetadataPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [selectedConditionId, setSelectedConditionId] = useState("");
  const [evidenceItems, setEvidenceItems] = useState<EvidenceItem[]>([]);

  const [evidenceType, setEvidenceType] = useState("MEDICAL_REPORT");
  const [evidenceStatus, setEvidenceStatus] = useState("LISTED_NOT_UPLOADED");
  const [originalFileName, setOriginalFileName] = useState("");
  const [fileType, setFileType] = useState("");
  const [documentDate, setDocumentDate] = useState("");
  const [providerName, setProviderName] = useState("");
  const [userNotes, setUserNotes] = useState("");
  const [usedInGeneratedPack, setUsedInGeneratedPack] = useState(false);

  const [isLoadingConditions, setIsLoadingConditions] = useState(false);
  const [isLoadingEvidence, setIsLoadingEvidence] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [updatingEvidenceStatusId, setUpdatingEvidenceStatusId] = useState<string | null>(null);
  const [removingEvidenceItemId, setRemovingEvidenceItemId] = useState<string | null>(null);
  const [deletingUploadedFileItemId, setDeletingUploadedFileItemId] = useState<string | null>(null);
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

  async function loadEvidenceItems(conditionId: string) {
    if (loading || !user || !conditionId) {
      return;
    }

    setIsLoadingEvidence(true);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = await getConditionEvidenceItems(token, workspaceId, conditionId);
      setEvidenceItems(rows);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load evidence items.";
      setErrorMessage(message);
    } finally {
      setIsLoadingEvidence(false);
    }
  }

  useEffect(() => {
    loadConditions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  useEffect(() => {
    if (selectedConditionId) {
      loadEvidenceItems(selectedConditionId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedConditionId]);


  async function handleUpdateEvidenceStatus(evidenceItem: EvidenceItem, evidenceStatus: string) {
    setStatusMessage("");
    setErrorMessage("");
    setUpdatingEvidenceStatusId(evidenceItem.id);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      await updateEvidenceStatus(token, workspaceId, evidenceItem.id, evidenceStatus);

      setStatusMessage(`Evidence status updated to ${getEvidenceStatusLabel(evidenceStatus)}.`);
      await loadEvidenceItems(selectedConditionId);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not update evidence status.";
      setErrorMessage(message);
    } finally {
      setUpdatingEvidenceStatusId(null);
    }
  }


  async function handleDeleteUploadedFile(evidenceItem: EvidenceItem) {
    const confirmed = window.confirm(
      "Delete the uploaded file for this evidence item?\n\nThe evidence item will stay listed in this workspace, but the stored file will be deleted and the item will return to listed, not uploaded. This does not contact DVA and does not delete anything already submitted outside this app.",
    );

    if (!confirmed) {
      return;
    }

    setStatusMessage("");
    setErrorMessage("");
    setDeletingUploadedFileItemId(evidenceItem.id);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const updatedEvidenceItem = await deleteEvidenceUploadedFile(token, workspaceId, evidenceItem.id);

      setEvidenceItems((currentItems) =>
        currentItems.map((item) =>
          item.id === updatedEvidenceItem.id ? updatedEvidenceItem : item,
        ),
      );

      setStatusMessage("Uploaded file deleted. Evidence item remains listed as not uploaded.");
      await loadEvidenceItems(selectedConditionId);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not delete uploaded evidence file.";
      setErrorMessage(message);
    } finally {
      setDeletingUploadedFileItemId(null);
    }
  }
  async function handleArchiveEvidenceItem(evidenceItem: EvidenceItem) {
    const confirmed = window.confirm(
      "Remove this evidence item from the active workspace?\n\nThis will stop it appearing in active evidence lists, evidence gap checks and future AI draft preparation. It does not contact DVA and does not delete anything already submitted outside this app.",
    );

    if (!confirmed) {
      return;
    }

    setStatusMessage("");
    setErrorMessage("");
    setRemovingEvidenceItemId(evidenceItem.id);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      await archiveEvidenceItem(token, workspaceId, evidenceItem.id);

      setStatusMessage("Evidence item removed from the active workspace.");
      await loadEvidenceItems(selectedConditionId);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not remove evidence item from workspace.";
      setErrorMessage(message);
    } finally {
      setRemovingEvidenceItemId(null);
    }
  }
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
        setErrorMessage("Select a condition before adding evidence metadata.");
        return;
      }

      await createEvidenceItem(token, workspaceId, selectedConditionId, {
        evidenceType,
        evidenceStatus,
        originalFileName: originalFileName || undefined,
        fileType: fileType || undefined,
        documentDate: documentDate || undefined,
        providerName: providerName || undefined,
        userNotes: userNotes || undefined,
        usedInGeneratedPack,
      });

      setEvidenceType("MEDICAL_REPORT");
      setEvidenceStatus("LISTED_NOT_UPLOADED");
      setOriginalFileName("");
      setFileType("");
      setDocumentDate("");
      setProviderName("");
      setUserNotes("");
      setUsedInGeneratedPack(false);

      setStatusMessage("Evidence metadata saved.");
      await loadEvidenceItems(selectedConditionId);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not save evidence metadata.";
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
          Sign in before adding evidence metadata.
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
          Evidence metadata is recorded against a specific condition.
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
          Evidence metadata
        </p>

        <h1 className="mt-4 text-3xl font-bold">List an evidence item</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          Record what evidence exists, what is missing, what needs review, and which condition it
          belongs to. File upload is handled in the next task.
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
              id="evidenceType"
              label="Evidence type"
              value={evidenceType}
              options={evidenceTypes}
              onChange={setEvidenceType}
            />

            <SelectField
              id="evidenceStatus"
              label="Evidence status"
              value={evidenceStatus}
              options={evidenceStatuses}
              onChange={setEvidenceStatus}
            />
          </div>

          <div className="grid gap-5 md:grid-cols-2">
            <div>
              <label htmlFor="originalFileName" className="text-sm font-medium text-slate-200">
                File name or document name
              </label>
              <input
                id="originalFileName"
                type="text"
                value={originalFileName}
                onChange={(event) => setOriginalFileName(event.target.value)}
                placeholder="Example: GP report May 2026.pdf"
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              />
            </div>

            <div>
              <label htmlFor="fileType" className="text-sm font-medium text-slate-200">
                File type
              </label>
              <input
                id="fileType"
                type="text"
                value={fileType}
                onChange={(event) => setFileType(event.target.value)}
                placeholder="Example: application/pdf"
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              />
            </div>
          </div>

          <div className="grid gap-5 md:grid-cols-2">
            <div>
              <label htmlFor="documentDate" className="text-sm font-medium text-slate-200">
                Document date
              </label>
              <DatePickerInput id="documentDate" value={documentDate} onChange={setDocumentDate} />
            </div>

            <div>
              <label htmlFor="providerName" className="text-sm font-medium text-slate-200">
                Provider or source
              </label>
              <input
                id="providerName"
                type="text"
                value={providerName}
                onChange={(event) => setProviderName(event.target.value)}
                placeholder="Example: GP, specialist, DVA, personal notes"
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              />

            <div className="mt-3 flex flex-wrap gap-2">
              {evidenceSourceQuickTags.map((source) => (
                <button
                  key={source}
                  type="button"
                  onClick={() => setProviderName(source)}
                  className="rounded-full border border-white/10 bg-slate-900 px-3 py-1 text-xs text-slate-300 hover:border-cyan-300 hover:text-cyan-100"
                >
                  {source}
                </button>
              ))}
            </div>
            </div>
          </div>

          <div>
            <label htmlFor="userNotes" className="text-sm font-medium text-slate-200">
              Notes
            </label>
            <textarea
              id="userNotes"
              value={userNotes}
              onChange={(event) => setUserNotes(event.target.value)}
              rows={4}
              placeholder="Example: Need to request this from GP. Relevant to current symptoms and treatment."
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
            />
          </div>

          <label className="flex items-center gap-3 rounded-xl border border-white/10 bg-slate-900 p-4 text-sm text-slate-200">
            <input
              type="checkbox"
              checked={usedInGeneratedPack}
              onChange={(event) => setUsedInGeneratedPack(event.target.checked)}
            />
            Mark this item for use in a generated preparation pack.
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
            {isSubmitting ? "Saving evidence..." : "Save evidence metadata"}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Evidence items
        </p>

        <h2 className="mt-4 text-2xl font-bold">
          {selectedCondition?.conditionName ?? "Selected condition"}
        </h2>

        <p className="mt-3 text-sm text-cyan-100">
          Evidence metadata is linked to condition: {selectedCondition?.conditionName ?? "Select a condition"}
        </p>

        <EvidenceListSummaryPanel
          evidenceItems={evidenceItems}
          conditionName={selectedCondition?.conditionName}
        />

        {isLoadingEvidence ? (
          <p className="mt-6 text-slate-300">Loading evidence items...</p>
        ) : evidenceItems.length === 0 ? (
          <p className="mt-6 text-slate-300">
            No evidence metadata has been saved for this condition yet.
          </p>
        ) : (
          <div className="mt-6 grid gap-4">
            {evidenceItems.map((item) => (
              <div key={item.id} className="rounded-xl border border-white/10 bg-slate-900 p-5">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <p className="font-mono text-xs text-cyan-200">{getEvidenceTypeLabel(item.evidenceType)}</p>
                    <h3 className="mt-2 font-semibold">
                      {item.originalFileName || "Unnamed evidence item"}
                    </h3>
                    <p className="mt-2 text-sm text-slate-300">
                      Status: {getEvidenceStatusLabel(item.evidenceStatus)}
                    </p>
                    <p className="mt-1 text-sm text-slate-400">
                      Category: {getEvidenceTypeCategory(item.evidenceType)}
                    </p>
                  </div>

                  <span className="rounded-xl border border-white/10 px-3 py-2 text-xs text-slate-300">
                    {item.usedInGeneratedPack ? "PACK" : "NOT IN PACK"}
                  </span>
                </div>

                <div className="mt-4 grid gap-2 text-sm text-slate-400 md:grid-cols-2">
                  <p>Provider/source: {item.providerName || "Not recorded"}</p>
                  <p>Document date: {item.documentDate || "Not recorded"}</p>
                  <p>File type: {item.fileType || "Not recorded"}</p>
                  <p>Uploaded: {item.uploadedAt && item.storagePath ? "Yes" : "No"}</p>
                </div>

                {item.userNotes && (
                  <p className="mt-4 text-sm leading-6 text-slate-400">{item.userNotes}</p>
                )}

                <div className="mt-5 rounded-xl border border-white/10 bg-slate-950 p-4">
                  <p className="text-sm font-semibold text-white">Evidence status</p>
                  <p className="mt-1 text-xs leading-5 text-slate-400">
                    This is your preparation status only. It does not mean DVA has reviewed, accepted or relied on the evidence.
                  </p>

                  <div className="mt-4 grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
                    {evidenceStatusOptions.map((statusOption) => (
                      <button
                        key={statusOption.value}
                        type="button"
                        title={statusOption.description}
                        disabled={
                          updatingEvidenceStatusId === item.id ||
                          item.evidenceStatus === statusOption.value
                        }
                        onClick={() => handleUpdateEvidenceStatus(item, statusOption.value)}
                        className={
                          item.evidenceStatus === statusOption.value
                            ? "rounded-xl border border-cyan-300 bg-cyan-300/10 px-3 py-2 text-xs font-semibold text-cyan-100"
                            : "rounded-xl border border-white/10 bg-slate-900 px-3 py-2 text-xs text-slate-300 hover:border-cyan-300 hover:text-cyan-100 disabled:opacity-60"
                        }
                      >
                        {statusOption.label}
                      </button>
                    ))}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. Evidence metadata helps organise documents and notes.
        It does not upload files yet, submit material to DVA, provide legal advice, provide medical
        advice, estimate compensation, or guarantee claim success.
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