"use client";

import Link from "next/link";
import { FormEvent, useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  GeneratedDocument,
  createGeneratedDocument,
  getGeneratedDocuments,
  updateGeneratedDocument,
} from "@/lib/api";

type GeneratedDocumentListPanelProps = {
  workspaceId: string;
};

const documentTypes = [
  "POST_2026_PI_CLAIM_STARTER_PACK",
  "DOCTOR_GUIDANCE_PACK",
  "DOCTOR_REQUEST_LETTER",
  "EVIDENCE_GAP_SUMMARY",
];

const documentStatuses = [
  "REQUESTED",
  "GENERATING",
  "GENERATED",
  "FAILED",
  "DOWNLOADED",
  "SUPERSEDED",
];

export function GeneratedDocumentListPanel({
  workspaceId,
}: GeneratedDocumentListPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [documents, setDocuments] = useState<GeneratedDocument[]>([]);
  const [selectedDocumentId, setSelectedDocumentId] = useState("");

  const [documentType, setDocumentType] = useState("POST_2026_PI_CLAIM_STARTER_PACK");
  const [documentStatus, setDocumentStatus] = useState("REQUESTED");
  const [templateVersion, setTemplateVersion] = useState("web-manual-v1");
  const [docxStoragePath, setDocxStoragePath] = useState("");
  const [pdfStoragePath, setPdfStoragePath] = useState("");
  const [includedAiDraftIds, setIncludedAiDraftIds] = useState("");

  const [isLoadingDocuments, setIsLoadingDocuments] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);

  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  const selectedDocument = useMemo(
    () => documents.find((document) => document.id === selectedDocumentId) ?? null,
    [documents, selectedDocumentId],
  );

  const summary = useMemo(() => {
    return {
      total: documents.length,
      requested: documents.filter((document) => document.documentStatus === "REQUESTED").length,
      generated: documents.filter((document) => document.documentStatus === "GENERATED").length,
      downloaded: documents.filter((document) => document.documentStatus === "DOWNLOADED").length,
      failed: documents.filter((document) => document.documentStatus === "FAILED").length,
    };
  }, [documents]);

  async function getTokenOrSetError() {
    const token = await getIdToken();

    if (!token) {
      setErrorMessage("No Firebase ID token is available. Please sign in again.");
      return null;
    }

    return token;
  }

  async function loadDocuments() {
    if (loading || !user) {
      return;
    }

    setIsLoadingDocuments(true);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = await getGeneratedDocuments(token, workspaceId);
      setDocuments(rows);

      if (rows.length > 0 && !rows.some((document) => document.id === selectedDocumentId)) {
        setSelectedDocumentId(rows[0].id);
      }

      if (rows.length === 0) {
        setSelectedDocumentId("");
      }
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load generated documents.";
      setErrorMessage(message);
    } finally {
      setIsLoadingDocuments(false);
    }
  }

  useEffect(() => {
    loadDocuments();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  useEffect(() => {
    if (!selectedDocument) {
      return;
    }

    setDocumentType(selectedDocument.documentType);
    setDocumentStatus(selectedDocument.documentStatus);
    setTemplateVersion(selectedDocument.templateVersion);
    setDocxStoragePath(selectedDocument.docxStoragePath ?? "");
    setPdfStoragePath(selectedDocument.pdfStoragePath ?? "");
    setIncludedAiDraftIds(selectedDocument.includedAiDraftIds ?? "");
  }, [selectedDocument]);

  async function handleCreateDocument(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setStatusMessage("");
    setErrorMessage("");
    setIsCreating(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      await createGeneratedDocument(token, workspaceId, {
        documentType,
        documentStatus,
        templateVersion: templateVersion || "web-manual-v1",
        docxStoragePath: docxStoragePath || undefined,
        pdfStoragePath: pdfStoragePath || undefined,
        includedAiDraftIds: includedAiDraftIds || undefined,
      });

      setDocumentType("POST_2026_PI_CLAIM_STARTER_PACK");
      setDocumentStatus("REQUESTED");
      setTemplateVersion("web-manual-v1");
      setDocxStoragePath("");
      setPdfStoragePath("");
      setIncludedAiDraftIds("");

      setStatusMessage("Generated document metadata created.");
      await loadDocuments();
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Could not create generated document metadata.";

      setErrorMessage(message);
    } finally {
      setIsCreating(false);
    }
  }

  async function handleUpdateDocument(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setStatusMessage("");
    setErrorMessage("");
    setIsUpdating(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      if (!selectedDocument) {
        setErrorMessage("Select a generated document before saving changes.");
        return;
      }

      await updateGeneratedDocument(token, workspaceId, selectedDocument.id, {
        documentType,
        documentStatus,
        templateVersion,
        docxStoragePath,
        pdfStoragePath,
        includedAiDraftIds,
      });

      setStatusMessage("Generated document metadata updated.");
      await loadDocuments();
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Could not update generated document metadata.";

      setErrorMessage(message);
    } finally {
      setIsUpdating(false);
    }
  }

  async function handleQuickStatusUpdate(nextStatus: string) {
    setStatusMessage("");
    setErrorMessage("");
    setIsUpdating(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      if (!selectedDocument) {
        setErrorMessage("Select a generated document before updating status.");
        return;
      }

      await updateGeneratedDocument(token, workspaceId, selectedDocument.id, {
        documentStatus: nextStatus,
      });

      setDocumentStatus(nextStatus);
      setStatusMessage(`Generated document marked as ${nextStatus}.`);
      await loadDocuments();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not update generated document status.";

      setErrorMessage(message);
    } finally {
      setIsUpdating(false);
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
        <p className="mt-2 text-sm">Sign in before viewing generated documents.</p>
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
          Generated documents
        </p>

        <h1 className="mt-4 text-3xl font-bold">Document metadata list</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          Track generated document records for this workspace. This UI stores document metadata
          only. Actual DOCX/PDF generation will be connected later.
        </p>

        <div className="mt-8 grid gap-4 md:grid-cols-5">
          <SummaryCard label="Total" value={summary.total} />
          <SummaryCard label="Requested" value={summary.requested} />
          <SummaryCard label="Generated" value={summary.generated} />
          <SummaryCard label="Downloaded" value={summary.downloaded} />
          <SummaryCard label="Failed" value={summary.failed} />
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
          Create metadata
        </p>

        <h2 className="mt-4 text-2xl font-bold">Create a generated document record</h2>

        <form onSubmit={handleCreateDocument} className="mt-8 space-y-6">
          <GeneratedDocumentFields
            documentType={documentType}
            setDocumentType={setDocumentType}
            documentStatus={documentStatus}
            setDocumentStatus={setDocumentStatus}
            templateVersion={templateVersion}
            setTemplateVersion={setTemplateVersion}
            docxStoragePath={docxStoragePath}
            setDocxStoragePath={setDocxStoragePath}
            pdfStoragePath={pdfStoragePath}
            setPdfStoragePath={setPdfStoragePath}
            includedAiDraftIds={includedAiDraftIds}
            setIncludedAiDraftIds={setIncludedAiDraftIds}
          />

          <button
            type="submit"
            disabled={isCreating}
            className="w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
          >
            {isCreating ? "Creating document metadata..." : "Create document metadata"}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Document records
        </p>

        {isLoadingDocuments ? (
          <p className="mt-6 text-slate-300">Loading generated documents...</p>
        ) : documents.length === 0 ? (
          <p className="mt-6 text-slate-300">
            No generated document metadata has been created yet.
          </p>
        ) : (
          <div className="mt-6 grid gap-4">
            {documents.map((document) => (
              <button
                key={document.id}
                type="button"
                onClick={() => setSelectedDocumentId(document.id)}
                className={
                  selectedDocumentId === document.id
                    ? "rounded-xl border border-cyan-300 bg-cyan-300/10 p-5 text-left"
                    : "rounded-xl border border-white/10 bg-slate-900 p-5 text-left hover:bg-white/5"
                }
              >
                <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                  <div>
                    <p className="font-mono text-xs text-cyan-200">{document.documentType}</p>
                    <h3 className="mt-2 font-semibold text-white">
                      {document.templateVersion}
                    </h3>
                    <p className="mt-2 text-sm text-slate-400">
                      ID: {document.id}
                    </p>
                  </div>

                  <span className="rounded-xl border border-white/10 px-3 py-2 text-xs text-slate-300">
                    {document.documentStatus}
                  </span>
                </div>

                <div className="mt-4 grid gap-2 text-sm text-slate-400 md:grid-cols-2">
                  <p>DOCX path: {document.docxStoragePath || "Not recorded"}</p>
                  <p>PDF path: {document.pdfStoragePath || "Not recorded"}</p>
                  <p>Generated: {document.generatedAt || "Not generated"}</p>
                  <p>Downloaded: {document.downloadedAt || "Not downloaded"}</p>
                </div>
              </button>
            ))}
          </div>
        )}
      </section>

      {selectedDocument && (
        <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Review selected document
          </p>

          <h2 className="mt-4 text-2xl font-bold">Update document metadata</h2>

          <div className="mt-6 flex flex-wrap gap-3">
            <button
              type="button"
              onClick={() => handleQuickStatusUpdate("GENERATED")}
              disabled={isUpdating}
              className="rounded-xl border border-green-300/30 bg-green-300/10 px-4 py-2 text-sm font-semibold text-green-100 hover:bg-green-300/20 disabled:opacity-60"
            >
              Mark generated
            </button>

            <button
              type="button"
              onClick={() => handleQuickStatusUpdate("DOWNLOADED")}
              disabled={isUpdating}
              className="rounded-xl border border-cyan-300/30 bg-cyan-300/10 px-4 py-2 text-sm font-semibold text-cyan-100 hover:bg-cyan-300/20 disabled:opacity-60"
            >
              Mark downloaded
            </button>

            <button
              type="button"
              onClick={() => handleQuickStatusUpdate("FAILED")}
              disabled={isUpdating}
              className="rounded-xl border border-red-300/30 bg-red-300/10 px-4 py-2 text-sm font-semibold text-red-100 hover:bg-red-300/20 disabled:opacity-60"
            >
              Mark failed
            </button>
          </div>

          <form onSubmit={handleUpdateDocument} className="mt-8 space-y-6">
            <GeneratedDocumentFields
              documentType={documentType}
              setDocumentType={setDocumentType}
              documentStatus={documentStatus}
              setDocumentStatus={setDocumentStatus}
              templateVersion={templateVersion}
              setTemplateVersion={setTemplateVersion}
              docxStoragePath={docxStoragePath}
              setDocxStoragePath={setDocxStoragePath}
              pdfStoragePath={pdfStoragePath}
              setPdfStoragePath={setPdfStoragePath}
              includedAiDraftIds={includedAiDraftIds}
              setIncludedAiDraftIds={setIncludedAiDraftIds}
            />

            <button
              type="submit"
              disabled={isUpdating}
              className="w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
            >
              {isUpdating ? "Saving document metadata..." : "Save document metadata"}
            </button>
          </form>
        </section>
      )}

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. Generated document records are metadata only at this stage.
        This page does not submit material to DVA, provide legal advice, provide medical advice,
        estimate compensation, or guarantee claim success.
      </section>
    </div>
  );
}

type GeneratedDocumentFieldsProps = {
  documentType: string;
  setDocumentType: (value: string) => void;
  documentStatus: string;
  setDocumentStatus: (value: string) => void;
  templateVersion: string;
  setTemplateVersion: (value: string) => void;
  docxStoragePath: string;
  setDocxStoragePath: (value: string) => void;
  pdfStoragePath: string;
  setPdfStoragePath: (value: string) => void;
  includedAiDraftIds: string;
  setIncludedAiDraftIds: (value: string) => void;
};

function GeneratedDocumentFields({
  documentType,
  setDocumentType,
  documentStatus,
  setDocumentStatus,
  templateVersion,
  setTemplateVersion,
  docxStoragePath,
  setDocxStoragePath,
  pdfStoragePath,
  setPdfStoragePath,
  includedAiDraftIds,
  setIncludedAiDraftIds,
}: GeneratedDocumentFieldsProps) {
  return (
    <>
      <div className="grid gap-5 md:grid-cols-2">
        <div>
          <label htmlFor="documentType" className="text-sm font-medium text-slate-200">
            Document type
          </label>

          <select
            id="documentType"
            value={documentType}
            onChange={(event) => setDocumentType(event.target.value)}
            className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
          >
            {documentTypes.map((type) => (
              <option key={type} value={type}>
                {type}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="documentStatus" className="text-sm font-medium text-slate-200">
            Document status
          </label>

          <select
            id="documentStatus"
            value={documentStatus}
            onChange={(event) => setDocumentStatus(event.target.value)}
            className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
          >
            {documentStatuses.map((status) => (
              <option key={status} value={status}>
                {status}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div>
        <label htmlFor="templateVersion" className="text-sm font-medium text-slate-200">
          Template version
        </label>

        <input
          id="templateVersion"
          type="text"
          value={templateVersion}
          onChange={(event) => setTemplateVersion(event.target.value)}
          className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
        />
      </div>

      <div>
        <label htmlFor="docxStoragePath" className="text-sm font-medium text-slate-200">
          DOCX storage path
        </label>

        <input
          id="docxStoragePath"
          type="text"
          value={docxStoragePath}
          onChange={(event) => setDocxStoragePath(event.target.value)}
          placeholder="Example: gs://bucket/generated/workspace/document.docx"
          className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
        />
      </div>

      <div>
        <label htmlFor="pdfStoragePath" className="text-sm font-medium text-slate-200">
          PDF storage path
        </label>

        <input
          id="pdfStoragePath"
          type="text"
          value={pdfStoragePath}
          onChange={(event) => setPdfStoragePath(event.target.value)}
          placeholder="Example: gs://bucket/generated/workspace/document.pdf"
          className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
        />
      </div>

      <div>
        <label htmlFor="includedAiDraftIds" className="text-sm font-medium text-slate-200">
          Included AI draft IDs
        </label>

        <textarea
          id="includedAiDraftIds"
          value={includedAiDraftIds}
          onChange={(event) => setIncludedAiDraftIds(event.target.value)}
          rows={3}
          placeholder="Optional list of AI draft IDs used in this document."
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