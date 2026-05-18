"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  AdminMeResponse,
  AdminTemplateRegistryEntry,
  createAdminTemplate,
  getAdminMe,
  getAdminTemplates,
  updateAdminTemplate,
} from "@/lib/api";

const templateTypes = ["", "QUESTION", "DOCUMENT"];
const approvalStatuses = ["", "DRAFT", "PENDING_REVIEW", "APPROVED", "REJECTED"];
const statusOptions = ["", "ACTIVE", "ARCHIVED"];

type AdminTemplateEditorPanelProps = {
  defaultTemplateType?: "QUESTION" | "DOCUMENT";
};

export function AdminTemplateEditorPanel({
  defaultTemplateType,
}: AdminTemplateEditorPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [adminStatus, setAdminStatus] = useState<AdminMeResponse | null>(null);
  const [templates, setTemplates] = useState<AdminTemplateRegistryEntry[]>([]);
  const [selectedTemplateId, setSelectedTemplateId] = useState("");

  const [search, setSearch] = useState("");
  const [templateTypeFilter, setTemplateTypeFilter] = useState(defaultTemplateType ?? "");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [approvalStatusFilter, setApprovalStatusFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");

  const [templateKey, setTemplateKey] = useState("");
  const [templateType, setTemplateType] = useState<"QUESTION" | "DOCUMENT">(
    defaultTemplateType ?? "QUESTION",
  );
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [category, setCategory] = useState("GENERAL");
  const [templateVersion, setTemplateVersion] = useState("v1");
  const [templateBody, setTemplateBody] = useState("");
  const [outputFormat, setOutputFormat] = useState("TEXT");
  const [approvalStatus, setApprovalStatus] = useState("DRAFT");
  const [approvedBy, setApprovedBy] = useState("");
  const [reviewNotes, setReviewNotes] = useState("");
  const [isActive, setIsActive] = useState(true);
  const [status, setStatus] = useState("ACTIVE");

  const [isCreateMode, setIsCreateMode] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  const selectedTemplate = useMemo(
    () => templates.find((template) => template.id === selectedTemplateId) ?? null,
    [templates, selectedTemplateId],
  );

  const summary = useMemo(
    () => ({
      total: templates.length,
      questions: templates.filter((template) => template.templateType === "QUESTION").length,
      documents: templates.filter((template) => template.templateType === "DOCUMENT").length,
      approved: templates.filter((template) => template.approvalStatus === "APPROVED").length,
    }),
    [templates],
  );

  async function getTokenOrSetError() {
    const token = await getIdToken();

    if (!token) {
      setErrorMessage("No Firebase ID token is available. Please sign in again.");
      return null;
    }

    return token;
  }

  async function loadAdminStatus() {
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const me = await getAdminMe(token);
      setAdminStatus(me);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load admin access status.";
      setErrorMessage(message);
    }
  }

  async function loadTemplates() {
    setIsLoading(true);
    setStatusMessage("");
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = await getAdminTemplates(token, {
        search,
        templateType: templateTypeFilter,
        category: categoryFilter,
        approvalStatus: approvalStatusFilter,
        status: statusFilter,
      });

      setTemplates(rows);
      setStatusMessage(`Loaded ${rows.length} template${rows.length === 1 ? "" : "s"}.`);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not load templates.";
      setErrorMessage(message);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    if (!loading && user) {
      loadAdminStatus();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user]);

  useEffect(() => {
    if (adminStatus?.isAdmin) {
      loadTemplates();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [adminStatus?.isAdmin]);

  function startCreateMode(nextType: "QUESTION" | "DOCUMENT" = defaultTemplateType ?? "QUESTION") {
    setIsCreateMode(true);
    setSelectedTemplateId("");
    setTemplateKey("");
    setTemplateType(nextType);
    setTitle("");
    setDescription("");
    setCategory("GENERAL");
    setTemplateVersion("v1");
    setTemplateBody("");
    setOutputFormat("TEXT");
    setApprovalStatus("DRAFT");
    setApprovedBy("");
    setReviewNotes("");
    setIsActive(true);
    setStatus("ACTIVE");
    setStatusMessage("Create mode started.");
  }

  function selectTemplate(template: AdminTemplateRegistryEntry) {
    setIsCreateMode(false);
    setSelectedTemplateId(template.id);
    setTemplateKey(template.templateKey);
    setTemplateType(template.templateType);
    setTitle(template.title);
    setDescription(template.description);
    setCategory(template.category);
    setTemplateVersion(template.templateVersion);
    setTemplateBody(template.templateBody);
    setOutputFormat(template.outputFormat);
    setApprovalStatus(template.approvalStatus);
    setApprovedBy(template.approvedBy ?? "");
    setReviewNotes(template.reviewNotes ?? "");
    setIsActive(template.isActive);
    setStatus(template.status);
    setStatusMessage(`Selected ${template.templateKey}.`);
  }

  async function handleFilterSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await loadTemplates();
  }

  async function handleSave(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setIsSaving(true);
    setStatusMessage("");
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      if (isCreateMode) {
        const created = await createAdminTemplate(token, {
          templateKey,
          templateType,
          title,
          description,
          category,
          templateVersion,
          templateBody,
          outputFormat,
          approvalStatus,
          approvedBy,
          reviewNotes,
          isActive,
          status,
        });

        setTemplates((current) => [created, ...current]);
        selectTemplate(created);
        setStatusMessage(`Created template ${created.templateKey}.`);
      } else {
        if (!selectedTemplate) {
          setErrorMessage("Select a template before saving.");
          return;
        }

        const updated = await updateAdminTemplate(token, selectedTemplate.id, {
          title,
          description,
          category,
          templateVersion,
          templateBody,
          outputFormat,
          approvalStatus,
          approvedBy,
          reviewNotes,
          isActive,
          status,
        });

        setTemplates((current) =>
          current.map((template) => (template.id === updated.id ? updated : template)),
        );

        selectTemplate(updated);
        setStatusMessage(`Saved template ${updated.templateKey}.`);
      }
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not save template.";
      setErrorMessage(message);
    } finally {
      setIsSaving(false);
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
        <h1 className="text-3xl font-bold text-white">Sign in required</h1>
        <p className="mt-4 text-sm text-slate-300">Sign in before opening admin tools.</p>
      </section>
    );
  }

  if (adminStatus && !adminStatus.isAdmin) {
    return (
      <section className="rounded-2xl border border-yellow-300/30 bg-yellow-300/10 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-yellow-100">
          Admin access denied
        </p>

        <h1 className="mt-4 text-3xl font-bold text-white">
          This account is not an admin
        </h1>

        <p className="mt-4 text-sm leading-6 text-yellow-100">
          Role: {adminStatus.role}. Reason: {adminStatus.reason}.
        </p>
      </section>
    );
  }

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Template editor
        </p>

        <h1 className="mt-4 text-3xl font-bold text-white">
          Question and document templates
        </h1>

        <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
          Create and edit admin-managed question and document template records.
          This registry is for controlled template content and future versioning.
        </p>

        <div className="mt-6 grid gap-4 md:grid-cols-4">
          <SummaryCard label="Loaded" value={summary.total} />
          <SummaryCard label="Questions" value={summary.questions} />
          <SummaryCard label="Documents" value={summary.documents} />
          <SummaryCard label="Approved" value={summary.approved} />
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
          Filters
        </p>

        <form onSubmit={handleFilterSubmit} className="mt-6 grid gap-4 md:grid-cols-5">
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search templates"
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
          />

          <select
            value={templateTypeFilter}
            onChange={(event) => setTemplateTypeFilter(event.target.value)}
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
          >
            {templateTypes.map((value) => (
              <option key={value || "any"} value={value}>
                {value || "Any type"}
              </option>
            ))}
          </select>

          <input
            value={categoryFilter}
            onChange={(event) => setCategoryFilter(event.target.value)}
            placeholder="Category"
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500"
          />

          <select
            value={approvalStatusFilter}
            onChange={(event) => setApprovalStatusFilter(event.target.value)}
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
          >
            {approvalStatuses.map((value) => (
              <option key={value || "any"} value={value}>
                {value || "Any approval"}
              </option>
            ))}
          </select>

          <select
            value={statusFilter}
            onChange={(event) => setStatusFilter(event.target.value)}
            className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
          >
            {statusOptions.map((value) => (
              <option key={value || "any"} value={value}>
                {value || "Any status"}
              </option>
            ))}
          </select>

          <button
            type="submit"
            disabled={isLoading}
            className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60 md:col-span-5"
          >
            {isLoading ? "Loading..." : "Apply filters"}
          </button>
        </form>
      </section>

      <section className="grid gap-8 xl:grid-cols-[1fr_1.2fr]">
        <div className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
            <div>
              <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
                Templates
              </p>
              <p className="mt-3 text-sm text-slate-300">
                Select a template to edit, or create a new question/document template.
              </p>
            </div>

            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                onClick={() => startCreateMode("QUESTION")}
                className="rounded-xl border border-cyan-300/40 px-4 py-2 text-xs font-semibold text-cyan-100 hover:bg-cyan-300/10"
              >
                New question
              </button>

              <button
                type="button"
                onClick={() => startCreateMode("DOCUMENT")}
                className="rounded-xl border border-cyan-300/40 px-4 py-2 text-xs font-semibold text-cyan-100 hover:bg-cyan-300/10"
              >
                New document
              </button>
            </div>
          </div>

          {templates.length === 0 ? (
            <p className="mt-6 text-sm text-slate-300">
              No templates match the current filter.
            </p>
          ) : (
            <div className="mt-6 grid gap-4">
              {templates.map((template) => (
                <button
                  key={template.id}
                  type="button"
                  onClick={() => selectTemplate(template)}
                  className={`rounded-xl border p-5 text-left transition ${
                    selectedTemplateId === template.id
                      ? "border-cyan-300 bg-cyan-300/10"
                      : "border-white/10 bg-slate-900 hover:border-cyan-300/60"
                  }`}
                >
                  <p className="font-mono text-xs text-cyan-200">{template.templateKey}</p>
                  <h2 className="mt-2 text-base font-bold text-white">{template.title}</h2>
                  <p className="mt-2 line-clamp-2 text-sm leading-6 text-slate-300">
                    {template.description || "No description recorded."}
                  </p>

                  <div className="mt-3 flex flex-wrap gap-2 text-xs">
                    <span className="rounded-full border border-white/10 px-3 py-1 text-slate-300">
                      {template.templateType}
                    </span>
                    <span className="rounded-full border border-white/10 px-3 py-1 text-slate-300">
                      {template.category}
                    </span>
                    <span className="rounded-full border border-cyan-300/30 bg-cyan-300/10 px-3 py-1 text-cyan-100">
                      {template.approvalStatus}
                    </span>
                    <span className="rounded-full border border-white/10 px-3 py-1 text-slate-300">
                      {template.status}
                    </span>
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            {isCreateMode ? "Create template" : "Edit template"}
          </p>

          <form onSubmit={handleSave} className="mt-6 grid gap-5">
            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">Template key</span>
              <input
                value={templateKey}
                onChange={(event) => setTemplateKey(event.target.value)}
                disabled={!isCreateMode}
                placeholder="example-question-template-v1"
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white placeholder:text-slate-500 disabled:opacity-60"
              />
            </label>

            <div className="grid gap-5 md:grid-cols-2">
              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Template type</span>
                <select
                  value={templateType}
                  onChange={(event) => setTemplateType(event.target.value as "QUESTION" | "DOCUMENT")}
                  disabled={!isCreateMode}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white disabled:opacity-60"
                >
                  <option value="QUESTION">QUESTION</option>
                  <option value="DOCUMENT">DOCUMENT</option>
                </select>
              </label>

              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Category</span>
                <input
                  value={category}
                  onChange={(event) => setCategory(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                />
              </label>
            </div>

            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">Title</span>
              <input
                value={title}
                onChange={(event) => setTitle(event.target.value)}
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
              />
            </label>

            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">Description</span>
              <textarea
                value={description}
                onChange={(event) => setDescription(event.target.value)}
                rows={3}
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
              />
            </label>

            <div className="grid gap-5 md:grid-cols-2">
              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Template version</span>
                <input
                  value={templateVersion}
                  onChange={(event) => setTemplateVersion(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                />
              </label>

              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Output format</span>
                <input
                  value={outputFormat}
                  onChange={(event) => setOutputFormat(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                />
              </label>
            </div>

            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">Template body</span>
              <textarea
                value={templateBody}
                onChange={(event) => setTemplateBody(event.target.value)}
                rows={16}
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 font-mono text-sm leading-6 text-white"
              />
            </label>

            <div className="grid gap-5 md:grid-cols-2">
              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Approval status</span>
                <select
                  value={approvalStatus}
                  onChange={(event) => setApprovalStatus(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                >
                  {approvalStatuses.filter(Boolean).map((value) => (
                    <option key={value} value={value}>
                      {value}
                    </option>
                  ))}
                </select>
              </label>

              <label className="grid gap-2">
                <span className="text-sm font-medium text-slate-200">Status</span>
                <select
                  value={status}
                  onChange={(event) => setStatus(event.target.value)}
                  className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
                >
                  {statusOptions.filter(Boolean).map((value) => (
                    <option key={value} value={value}>
                      {value}
                    </option>
                  ))}
                </select>
              </label>
            </div>

            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">Approved by</span>
              <input
                value={approvedBy}
                onChange={(event) => setApprovedBy(event.target.value)}
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
              />
            </label>

            <label className="grid gap-2">
              <span className="text-sm font-medium text-slate-200">Review notes</span>
              <textarea
                value={reviewNotes}
                onChange={(event) => setReviewNotes(event.target.value)}
                rows={4}
                className="rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white"
              />
            </label>

            <label className="flex items-center gap-3 rounded-xl border border-white/10 bg-slate-950 p-4 text-sm text-slate-200">
              <input
                type="checkbox"
                checked={isActive}
                onChange={(event) => setIsActive(event.target.checked)}
                className="h-4 w-4"
              />
              Template is active
            </label>

            <button
              type="submit"
              disabled={isSaving}
              className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
            >
              {isSaving ? "Saving..." : isCreateMode ? "Create template" : "Save template"}
            </button>
          </form>
        </div>
      </section>
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