"use client";

import { useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  GeneratedDocument,
  createGeneratedDocumentDownloadUrl,
  generateDoctorGuidancePack,
  getGeneratedDocuments,
} from "@/lib/api/generatedDocuments";

type DoctorGuidanceExportPanelProps = {
  workspaceId: string;
};

export function DoctorGuidanceExportPanel({
  workspaceId,
}: DoctorGuidanceExportPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [documents, setDocuments] = useState<GeneratedDocument[]>([]);
  const [selectedDocumentId, setSelectedDocumentId] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isGenerating, setIsGenerating] = useState(false);
  const [isDownloading, setIsDownloading] = useState("");
  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  const doctorGuidanceDocuments = useMemo(
    () => documents.filter((document) => document.documentType === "DOCTOR_GUIDANCE_PACK"),
    [documents],
  );

  const selectedDocument = useMemo(
    () =>
      doctorGuidanceDocuments.find((document) => document.id === selectedDocumentId) ??
      doctorGuidanceDocuments[0] ??
      null,
    [doctorGuidanceDocuments, selectedDocumentId],
  );

  async function getTokenOrSetError() {
    const token = await getIdToken();

    if (!token) {
      setErrorMessage("No Firebase ID token is available. Please sign in again.");
      return null;
    }

    return token;
  }

  async function loadDocuments() {
    setIsLoading(true);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = await getGeneratedDocuments(token, workspaceId);
      const doctorRows = rows.filter(
        (document) => document.documentType === "DOCTOR_GUIDANCE_PACK",
      );

      setDocuments(rows);

      if (doctorRows.length > 0 && !doctorRows.some((document) => document.id === selectedDocumentId)) {
        setSelectedDocumentId(doctorRows[0].id);
      }

      if (doctorRows.length === 0) {
        setSelectedDocumentId("");
      }
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load Doctor Guidance Pack documents.";
      setErrorMessage(message);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    if (!loading && user) {
      loadDocuments();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  async function handleGenerate() {
    setStatusMessage("");
    setErrorMessage("");

    const confirmed = window.confirm(
      "Generate a Doctor Guidance Pack now?\n\nOnly approved doctor guidance drafts and active workspace records should be included. This does not submit anything to DVA and does not tell a doctor what opinion to provide.",
    );

    if (!confirmed) {
      return;
    }

    setIsGenerating(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const response = await generateDoctorGuidancePack(token, workspaceId);

      await loadDocuments();

      setSelectedDocumentId(response.document.id);
      setStatusMessage(
        `Doctor Guidance Pack generated. Version: ${response.documentVersion ?? response.document.templateVersion}.`,
      );
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not generate Doctor Guidance Pack.";
      setErrorMessage(message);
    } finally {
      setIsGenerating(false);
    }
  }

  async function handleDownload(format: "DOCX" | "PDF") {
    setStatusMessage("");
    setErrorMessage("");

    if (!selectedDocument) {
      setErrorMessage("Select a Doctor Guidance Pack before downloading.");
      return;
    }

    setIsDownloading(format);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const response = await createGeneratedDocumentDownloadUrl(
        token,
        workspaceId,
        selectedDocument.id,
        format,
      );

      window.open(response.url, "_blank", "noopener,noreferrer");

      setDocuments((current) =>
        current.map((document) =>
          document.id === response.document.id ? response.document : document,
        ),
      );

      setSelectedDocumentId(response.document.id);
      setStatusMessage(`${format} download link opened in a new tab.`);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : `Could not create ${format} download link.`;
      setErrorMessage(message);
    } finally {
      setIsDownloading("");
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
          Doctor guidance export
        </p>
        <p className="mt-4 text-sm text-slate-300">
          Sign in before exporting a Doctor Guidance Pack.
        </p>
      </section>
    );
  }

  return (
    <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
        Doctor guidance export
      </p>

      <h2 className="mt-4 text-2xl font-bold text-white">
        Export Doctor Guidance Pack
      </h2>

      <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
        Export a doctor-facing preparation pack as DOCX and PDF. Only approved doctor
        guidance drafts should be included. This does not provide medical advice and
        does not tell a doctor what opinion to provide.
      </p>

      <div className="mt-6 flex flex-wrap gap-3">
        <button
          type="button"
          onClick={handleGenerate}
          disabled={isGenerating}
          className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-cyan-200 disabled:opacity-60"
        >
          {isGenerating ? "Generating..." : "Generate Doctor Guidance Pack"}
        </button>

        <button
          type="button"
          onClick={loadDocuments}
          disabled={isLoading}
          className="rounded-xl border border-cyan-300/40 px-5 py-3 text-sm font-semibold text-cyan-100 hover:bg-cyan-300/10 disabled:opacity-60"
        >
          {isLoading ? "Refreshing..." : "Refresh packs"}
        </button>
      </div>

      {statusMessage && (
        <div className="mt-6 rounded-xl border border-emerald-300/30 bg-emerald-300/10 p-4 text-sm text-emerald-100">
          {statusMessage}
        </div>
      )}

      {errorMessage && (
        <div className="mt-6 rounded-xl border border-red-300/30 bg-red-300/10 p-4 text-sm text-red-100">
          {errorMessage}
        </div>
      )}

      {doctorGuidanceDocuments.length === 0 ? (
        <p className="mt-6 text-sm text-slate-300">
          No Doctor Guidance Pack exports are available yet.
        </p>
      ) : (
        <div className="mt-6 grid gap-4">
          {doctorGuidanceDocuments.map((document) => (
            <button
              key={document.id}
              type="button"
              onClick={() => setSelectedDocumentId(document.id)}
              className={`rounded-xl border p-5 text-left transition ${
                selectedDocument?.id === document.id
                  ? "border-cyan-300 bg-cyan-300/10"
                  : "border-white/10 bg-slate-900 hover:border-cyan-300/60"
              }`}
            >
              <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                <div>
                  <p className="text-lg font-semibold text-white">
                    Doctor Guidance Pack
                  </p>
                  <p className="mt-1 text-xs text-slate-400">
                    Template: {document.templateVersion}
                  </p>
                </div>

                <span className="rounded-full border border-cyan-300/30 bg-cyan-300/10 px-3 py-1 text-xs text-cyan-100">
                  {document.documentStatus}
                </span>
              </div>

              <div className="mt-4 grid gap-1 text-xs leading-5 text-slate-400">
                <p>Generated: {document.generatedAt || "Not generated"}</p>
                <p>Downloaded: {document.downloadedAt || "Not downloaded"}</p>
                <p>DOCX: {document.docxStoragePath || "Not available"}</p>
                <p>PDF: {document.pdfStoragePath || "Not available"}</p>
              </div>
            </button>
          ))}
        </div>
      )}

      {selectedDocument && (
        <div className="mt-6 rounded-xl border border-white/10 bg-slate-950 p-5">
          <p className="text-sm font-semibold text-white">Download selected pack</p>

          <div className="mt-4 flex flex-wrap gap-3">
            <button
              type="button"
              onClick={() => handleDownload("DOCX")}
              disabled={isDownloading === "DOCX" || !selectedDocument.docxStoragePath}
              className="rounded-xl border border-cyan-300/40 px-5 py-3 text-sm font-semibold text-cyan-100 hover:bg-cyan-300/10 disabled:opacity-60"
            >
              {isDownloading === "DOCX" ? "Opening DOCX..." : "Download DOCX"}
            </button>

            <button
              type="button"
              onClick={() => handleDownload("PDF")}
              disabled={isDownloading === "PDF" || !selectedDocument.pdfStoragePath}
              className="rounded-xl border border-cyan-300/40 px-5 py-3 text-sm font-semibold text-cyan-100 hover:bg-cyan-300/10 disabled:opacity-60"
            >
              {isDownloading === "PDF" ? "Opening PDF..." : "Download PDF"}
            </button>
          </div>

          <div className="mt-4 text-xs leading-5 text-slate-400">
            <p>Document ID: {selectedDocument.id}</p>
            <p>Included AI draft IDs: {selectedDocument.includedAiDraftIds || "None recorded"}</p>
          </div>
        </div>
      )}
    </section>
  );
}