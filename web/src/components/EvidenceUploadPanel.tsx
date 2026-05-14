"use client";

import { DatePickerInput } from "@/components/DatePickerInput";
import { EvidenceListSummaryPanel } from "@/components/EvidenceListSummaryPanel";
import {
  evidenceUploadAcceptValue,
  formatEvidenceFileSize,
  validateEvidenceUploadFile,
} from "@/lib/evidenceUploadValidation";
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
  createEvidenceUploadUrl,
  createEvidenceDownloadUrl,
  getClaimConditions,
  getConditionEvidenceItems,
  markEvidenceUploaded,
  updateEvidenceStatus,
  archiveEvidenceItem,
  deleteEvidenceUploadedFile,
} from "@/lib/api";

type EvidenceUploadPanelProps = {
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

export function EvidenceUploadPanel({ workspaceId }: EvidenceUploadPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [selectedConditionId, setSelectedConditionId] = useState("");
  const [evidenceItems, setEvidenceItems] = useState<EvidenceItem[]>([]);

  const [evidenceType, setEvidenceType] = useState("MEDICAL_REPORT");
  const [documentDate, setDocumentDate] = useState("");
  const [providerName, setProviderName] = useState("");
  const [userNotes, setUserNotes] = useState("");
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const [isLoadingConditions, setIsLoadingConditions] = useState(false);
  const [isLoadingEvidence, setIsLoadingEvidence] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [openingEvidenceItemId, setOpeningEvidenceItemId] = useState<string | null>(null);
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




  function handleSelectedFileChange(file: File | null) {
    setSelectedFile(file);
    setStatusMessage("");

    if (!file) {
      setErrorMessage("");
      return;
    }

    const validationMessage = validateEvidenceUploadFile(file);
    setErrorMessage(validationMessage ?? "");
  }
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
  async function handleOpenEvidenceItem(evidenceItem: EvidenceItem) {
    setStatusMessage("");
    setErrorMessage("");
    setOpeningEvidenceItemId(evidenceItem.id);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const downloadResponse = await createEvidenceDownloadUrl(
        token,
        workspaceId,
        evidenceItem.id,
      );

      window.open(downloadResponse.url, "_blank", "noopener,noreferrer");
      setStatusMessage("Evidence file opened in a new tab using a short-lived download link.");
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not open evidence file.";
      setErrorMessage(message);
    } finally {
      setOpeningEvidenceItemId(null);
    }
  }
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setStatusMessage("");
    setErrorMessage("");
    setIsUploading(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      if (!selectedConditionId) {
        setErrorMessage("Select a condition before uploading evidence.");
        return;
      }

      const fileValidationMessage = validateEvidenceUploadFile(selectedFile);

      if (fileValidationMessage) {
        setErrorMessage(fileValidationMessage);
        return;
      }

      const fileToUpload = selectedFile;

      if (!fileToUpload) {
        setErrorMessage("Choose a file before uploading.");
        return;
      }

      const uploadResponse = await createEvidenceUploadUrl(
        token,
        workspaceId,
        selectedConditionId,
        {
          evidenceType,
          originalFileName: fileToUpload.name,
          fileType: fileToUpload.type || "application/octet-stream",
          fileSize: fileToUpload.size,
          documentDate: documentDate || undefined,
          providerName: providerName || undefined,
          userNotes: userNotes || undefined,
        },
      );

      const putResponse = await fetch(uploadResponse.upload.url, {
        method: "PUT",
        headers: uploadResponse.upload.requiredHeaders ?? {},
        body: fileToUpload,
      });

      if (!putResponse.ok) {
        throw new Error(
          `Cloud Storage upload failed. The signed upload link may have expired or the file may not have been accepted. HTTP ${putResponse.status}`,
        );
      }

      await markEvidenceUploaded(token, workspaceId, uploadResponse.evidenceItem.id);

      setSelectedFile(null);
      setDocumentDate("");
      setProviderName("");
      setUserNotes("");
      setEvidenceType("MEDICAL_REPORT");

      const fileInput = document.getElementById("evidenceFile") as HTMLInputElement | null;
      if (fileInput) {
        fileInput.value = "";
      }

      setStatusMessage("Evidence file uploaded and confirmed.");
      await loadEvidenceItems(selectedConditionId);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not upload evidence file.";
      setErrorMessage(message);
    } finally {
      setIsUploading(false);
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
        <p className="mt-2 text-sm">Sign in before uploading evidence.</p>
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
          Evidence uploads are attached to a specific condition.
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
          Evidence upload
        </p>

        <h1 className="mt-4 text-3xl font-bold">Upload an evidence file</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          Upload a file directly to secure Cloud Storage using a short-lived signed upload URL.
          After upload, the app confirms the evidence item with the backend.
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
              <label htmlFor="evidenceType" className="text-sm font-medium text-slate-200">
                Evidence type
              </label>

              <select
                id="evidenceType"
                value={evidenceType}
                onChange={(event) => setEvidenceType(event.target.value)}
                className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
              >
                {evidenceTypes.map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="documentDate" className="text-sm font-medium text-slate-200">
                Document date
              </label>

              <DatePickerInput id="documentDate" value={documentDate} onChange={setDocumentDate} />
            </div>
          </div>

          <div>
            <label htmlFor="evidenceFile" className="text-sm font-medium text-slate-200">
              Evidence file
            </label>

            <input
              id="evidenceFile"
              type="file"
              accept={evidenceUploadAcceptValue}
              onChange={(event) => handleSelectedFileChange(event.target.files?.[0] ?? null)}
              required
              className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white file:mr-4 file:rounded-lg file:border-0 file:bg-cyan-300 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-slate-950"
            />

            <p className="mt-2 text-xs leading-5 text-slate-400">
              Supported files: PDF, image, Word, text and RTF. Maximum size: 25 MB.
            </p>

            {selectedFile && (
              <p className="mt-2 text-sm text-slate-400">
                Selected: {selectedFile.name} · {formatEvidenceFileSize(selectedFile.size)}
                {selectedFile.type ? ` · ${selectedFile.type}` : ""}
              </p>
            )}
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

          <div>
            <label htmlFor="userNotes" className="text-sm font-medium text-slate-200">
              Notes
            </label>

            <textarea
              id="userNotes"
              value={userNotes}
              onChange={(event) => setUserNotes(event.target.value)}
              rows={4}
              placeholder="Example: Relevant to current symptoms, treatment, diagnosis or functional impact."
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
            disabled={isUploading}
            className="w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
          >
            {isUploading ? "Uploading evidence..." : "Upload evidence file"}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Uploaded and listed evidence
        </p>

        <h2 className="mt-4 text-2xl font-bold">
          {selectedCondition?.conditionName ?? "Selected condition"}
        </h2>

        <p className="mt-3 text-sm text-cyan-100">
          Evidence linked to condition: {selectedCondition?.conditionName ?? "Select a condition"}
        </p>

        <EvidenceListSummaryPanel
          evidenceItems={evidenceItems}
          conditionName={selectedCondition?.conditionName}
        />

        {isLoadingEvidence ? (
          <p className="mt-6 text-slate-300">Loading evidence items...</p>
        ) : evidenceItems.length === 0 ? (
          <p className="mt-6 text-slate-300">
            No evidence has been listed or uploaded for this condition yet.
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
                    {item.uploadedAt && item.storagePath ? "UPLOADED" : "NOT UPLOADED"}
                  </span>
                </div>

                <div className="mt-4 grid gap-2 text-sm text-slate-400 md:grid-cols-2">
                  <p>Provider/source: {item.providerName || "Not recorded"}</p>
                  <p>Document date: {item.documentDate || "Not recorded"}</p>
                  <p>File type: {item.fileType || "Not recorded"}</p>
                  <p>File size: {item.fileSize ? `${Math.ceil(item.fileSize / 1024)} KB` : "Not recorded"}</p>
                </div>

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

                <button
                  type="button"
                  disabled={!item.uploadedAt || !item.storagePath || openingEvidenceItemId === item.id}
                  onClick={() => handleOpenEvidenceItem(item)}
                  className="mt-5 w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {openingEvidenceItemId === item.id
                    ? "Opening file..."
                    : item.uploadedAt && item.storagePath
                      ? "Open file"
                      : "File not uploaded yet"}
                </button>


                {item.uploadedAt && item.storagePath && (
                  <div className="mt-5 rounded-xl border border-yellow-300/20 bg-yellow-300/5 p-4">
                    <p className="text-sm font-semibold text-yellow-100">Delete uploaded file</p>
                    <p className="mt-1 text-xs leading-5 text-yellow-100/80">
                      Deletes the stored file but keeps this evidence item listed in the workspace.
                    </p>

                    <button
                      type="button"
                      disabled={deletingUploadedFileItemId === item.id}
                      onClick={() => handleDeleteUploadedFile(item)}
                      className="mt-4 rounded-xl border border-yellow-300/40 px-4 py-2 text-sm font-semibold text-yellow-100 hover:bg-yellow-300/10 disabled:opacity-60"
                    >
                      {deletingUploadedFileItemId === item.id
                        ? "Deleting uploaded file..."
                        : "Delete uploaded file"}
                    </button>
                  </div>
                )}
                <div className="mt-5 rounded-xl border border-red-300/20 bg-red-300/5 p-4">
                  <p className="text-sm font-semibold text-red-100">Remove evidence</p>
                  <p className="mt-1 text-xs leading-5 text-red-100/80">
                    Removes this item from the active workspace. It will stop appearing in active evidence lists, gap checks and future AI draft preparation.
                  </p>

                  <button
                    type="button"
                    disabled={removingEvidenceItemId === item.id}
                    onClick={() => handleArchiveEvidenceItem(item)}
                    className="mt-4 rounded-xl border border-red-300/40 px-4 py-2 text-sm font-semibold text-red-100 hover:bg-red-300/10 disabled:opacity-60"
                  >
                    {removingEvidenceItemId === item.id
                      ? "Removing evidence..."
                      : "Remove from workspace"}
                  </button>
                </div>

                {item.userNotes && (
                  <p className="mt-4 text-sm leading-6 text-slate-400">{item.userNotes}</p>
                )}
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. Uploading evidence stores a file for preparation use inside this
        app. It does not submit the file to DVA, provide legal advice, provide medical advice,
        estimate compensation, or guarantee claim success.
      </section>
    </div>
  );
}