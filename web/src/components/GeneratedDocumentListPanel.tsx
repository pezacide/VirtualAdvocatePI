"use client";

import { useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  GeneratedDocument,
  createGeneratedDocumentDownloadUrl,
  generateClaimStarterPack,
  getGeneratedDocuments,
} from "@/lib/api";

type GeneratedDocumentListPanelProps = {
  workspaceId: string;
};

export function GeneratedDocumentListPanel({ workspaceId }: GeneratedDocumentListPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [documents, setDocuments] = useState<GeneratedDocument[]>([]);
  const [selectedDocumentId, setSelectedDocumentId] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isGenerating, setIsGenerating] = useState(false);
  const [isDownloading, setIsDownloading] = useState("");
  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  const selectedDocument = useMemo(
    () => documents.find((document) => document.id === selectedDocumentId) ?? null,
    [documents, selectedDocumentId],
  );

  const summary = useMemo(
    () => ({
      total: documents.length,
      generated: documents.filter((document) => document.documentStatus === "GENERATED").length,
      downloaded: documents.filter((document) => document.documentStatus === "DOWNLOADED").length,
      failed: documents.filter((document) => document.documentStatus === "FAILED").length,
    }),
    [documents],
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
      setIsLoading(false);
    }
  }

  useEffect(() => {
    if (!loading && user) {
      loadDocuments();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  async function handleGenerateClaimStarterPack() {
    setStatusMessage("");
    setErrorMessage("");

    const confirmed = window.confirm(
      "Generate a Claim Starter Pack now?\n\nOnly approved AI drafts and active workspace records should be included. This does not submit anything to DVA.",
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

      const response = await generateClaimStarterPack(token, workspaceId);

      await loadDocuments();

      setSelectedDocumentId(response.document.id);

      setStatusMessage(
        `Claim Starter Pack generated. Included approved AI drafts: ${response.includedAiDraftCount}.`,
      );
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not generate Claim Starter Pack.";
      setErrorMessage(message);
    } finally {
      setIsGenerating(false);
    }
  }

  async function handleDownload(format: "DOCX" | "PDF") {
    setStatusMessage("");
    setErrorMessage("");

    if (!selectedDocument) {
      setErrorMessage("Select a generated document before downloading.");
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
          Generated documents
        </p>
        <h2 className="mt-4 text-2xl font-bold text-white">Sign in required</h2>
        <p className="mt-2 text-sm text-slate-300">
          Sign in before generating or downloading documents.
        </p>
      </section>
    );
  }

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Generated documents
        </p>

        <h2 className="mt-4 text-2xl font-bold text-white">
          Claim Starter Pack document generation
        </h2>

        <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
          Generate reviewed Claim Starter Pack documents as DOCX and PDF files.
          Generated packs are preparation support only. They do not submit anything to DVA,
          provide legal advice, provide medical advice, calculate impairment points,
          estimate compensation, make a DVA decision, or guarantee a claim outcome.
        </p>

        <div className="mt-6 grid gap-4 md:grid-cols-4">
          <SummaryCard label="Total" value={summary.total} />
          <SummaryCard label="Generated" value={summary.generated} />
          <SummaryCard label="Downloaded" value={summary.downloaded} />
          <SummaryCard label="Failed" value={summary.failed} />
        </div>

        <div className="mt-6 flex flex-wrap gap-3">
          <button
            type="button"
            onClick={handleGenerateClaimStarterPack}
            disabled={isGenerating}
            className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-cyan-200 disabled:opacity-60"
          >
            {isGenerating ? "Generating..." : "Generate Claim Starter Pack"}
          </button>

          <button
            type="button"
            onClick={loadDocuments}
            disabled={isLoading}
            className="rounded-xl border border-cyan-300/40 px-5 py-3 text-sm font-semibold text-cyan-100 hover:bg-cyan-300/10 disabled:opacity-60"
          >
            {isLoading ? "Refreshing..." : "Refresh documents"}
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
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Saved generated documents
        </p>

        {documents.length === 0 ? (
          <p className="mt-6 text-sm text-slate-300">
            No generated documents are available yet.
          </p>
        ) : (
          <div className="mt-6 grid gap-4">
            {documents.map((document) => (
              <button
                key={document.id}
                type="button"
                onClick={() => setSelectedDocumentId(document.id)}
                className={`rounded-xl border p-5 text-left transition ${
                  selectedDocumentId === document.id
                    ? "border-cyan-300 bg-cyan-300/10"
                    : "border-white/10 bg-slate-900 hover:border-cyan-300/60"
                }`}
              >
                <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                  <div>
                    <p className="text-lg font-semibold text-white">
                      {formatLabel(document.documentType)}
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
      </section>

      {selectedDocument && (
        <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Download selected document
          </p>

          <h3 className="mt-4 text-xl font-bold text-white">
            {formatLabel(selectedDocument.documentType)}
          </h3>

          <p className="mt-2 text-sm text-slate-400">
            Status: {selectedDocument.documentStatus}
          </p>

          <div className="mt-6 flex flex-wrap gap-3">
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

          <div className="mt-6 rounded-xl border border-white/10 bg-slate-950 p-5 text-xs leading-5 text-slate-300">
            <p>Document ID: {selectedDocument.id}</p>
            <p>Included AI draft IDs: {selectedDocument.includedAiDraftIds || "None recorded"}</p>
            <p>DOCX path: {selectedDocument.docxStoragePath || "Not available"}</p>
            <p>PDF path: {selectedDocument.pdfStoragePath || "Not available"}</p>
          </div>
        </section>
      )}
    </div>
  );
}

function SummaryCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl border border-white/10 bg-slate-900 p-4">
      <p className="text-xs uppercase tracking-[0.2em] text-slate-400">{label}</p>
      <p className="mt-2 text-2xl font-bold text-white">{value}</p>
    </div>
  );
}

function formatLabel(value: string) {
  return value
    .toLowerCase()
    .split("_")
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(" ");
}