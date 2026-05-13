"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  GarpMAnswerMap,
  GarpMAnswerValue,
  GarpMQuestionGroupRenderer,
  getMissingRequiredGarpMQuestions,
  getGarpMQuestionValidationMessages,
} from "@/components/garpM/GarpMQuestionRenderer";
import {
  GarpMQuestionAnswerType,
  GarpMQuestionGroupKey,
  GarpMQuestionTemplate,
  garpMQuestionGroupMetadata,
  garpMQuestionGroupTemplateSet,
  getQuestionsForGroup,
} from "@/lib/garpM";
import {
  ClaimCondition,
  QuestionResponse,
  createQuestionResponse,
  getClaimConditions,
  getQuestionResponses,
} from "@/lib/api";

type GarpMQuestionEnginePanelProps = {
  workspaceId: string;
};

type SectionProgress = {
  groupKey: GarpMQuestionGroupKey;
  title: string;
  answeredCount: number;
  totalQuestions: number;
  requiredCount: number;
  missingRequiredCount: number;
  validationIssueCount: number;
  savedCount: number;
  lastSavedAt: Date | null;
  statusLabel: string;
};

function toBackendQuestionGroup(question: GarpMQuestionTemplate) {
  switch (question.evidenceCategory) {
    case "DIAGNOSIS":
      return "DIAGNOSIS";

    case "SYMPTOMS":
      return "SYMPTOMS";

    case "TREATMENT":
      return "TREATMENT";

    case "MEDICATION":
      return "MEDICATION";

    case "STABILITY":
      return "STABILITY";

    case "FUNCTIONAL_IMPACT":
      return "FUNCTIONAL_IMPACT";

    case "LIFESTYLE_IMPACT":
      return "LIFESTYLE_IMPACT";

    case "WORK_IMPACT":
      return "WORK_IMPACT";

    case "WORSENING":
      return "WORSENING";

    case "PREVIOUS_COMPENSATION":
      return "PREVIOUS_COMPENSATION";

    case "EVIDENCE_GAP":
      return "EVIDENCE_MISSING";

    case "APPOINTMENT_PREP":
      return "CLAIM_CONTEXT";

    case "SERVICE_CONNECTION":
      return "CLAIM_CONTEXT";

    case "SUMMARY":
      return "CLAIM_CONTEXT";

    default:
      return "CLAIM_CONTEXT";
  }
}

export function GarpMQuestionEnginePanel({ workspaceId }: GarpMQuestionEnginePanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const activeQuestionGroups = garpMQuestionGroupTemplateSet.groups.filter(
    (group) => group.questions.length > 0,
  );

  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [selectedConditionId, setSelectedConditionId] = useState("");
  const [selectedGroupKey, setSelectedGroupKey] = useState<GarpMQuestionGroupKey>(
    activeQuestionGroups[0]?.groupKey ?? "DIAGNOSIS_SYMPTOMS_TREATMENT",
  );

  const [answers, setAnswers] = useState<GarpMAnswerMap>({});
  const [savedResponses, setSavedResponses] = useState<QuestionResponse[]>([]);
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);

  const [isLoadingConditions, setIsLoadingConditions] = useState(false);
  const [isLoadingResponses, setIsLoadingResponses] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  const selectedCondition = conditions.find(
    (condition) => condition.id === selectedConditionId,
  );

  const selectedGroup = garpMQuestionGroupTemplateSet.groups.find(
    (group) => group.groupKey === selectedGroupKey,
  );

  const selectedQuestions = useMemo(
    () => getQuestionsForGroup(garpMQuestionGroupTemplateSet, selectedGroupKey),
    [selectedGroupKey],
  );

  const missingRequiredQuestions = useMemo(
    () => getMissingRequiredGarpMQuestions(selectedQuestions, answers),
    [answers, selectedQuestions],
  );

  const sectionProgress = useMemo<SectionProgress[]>(() => {
    return activeQuestionGroups.map((group) => {
      const questions = group.questions;
      const answeredCount = questions.filter(
        (question) => !isAnswerEmpty(answers[question.id]),
      ).length;
      const requiredQuestions = questions.filter(
        (question) => question.requirementLevel === "REQUIRED",
      );
      const missingRequiredCount = getMissingRequiredGarpMQuestions(
        questions,
        answers,
      ).length;
      const validationIssueCount = getValidationIssueCountForQuestions(questions, answers);
      const savedCount = getSavedCountForQuestions(questions, savedResponses);
      const lastSavedAt = getLastSavedAtForQuestions(questions, savedResponses);

      let statusLabel = "Not started";

      if (savedCount > 0 && missingRequiredCount > 0) {
        statusLabel = "In progress";
      }

      if (savedCount > 0 && missingRequiredCount === 0) {
        statusLabel = "Required answers saved";
      }

      if (answeredCount > savedCount) {
        statusLabel = "Unsaved changes";
      }

      return {
        groupKey: group.groupKey,
        title: group.title,
        answeredCount,
        totalQuestions: questions.length,
        requiredCount: requiredQuestions.length,
        missingRequiredCount,
        validationIssueCount,
        savedCount,
        lastSavedAt,
        statusLabel,
      };
    });
  }, [activeQuestionGroups, answers, savedResponses]);

  const selectedSectionProgress = sectionProgress.find(
    (section) => section.groupKey === selectedGroupKey,
  );

  const totalSavedSections = sectionProgress.filter(
    (section) => section.savedCount > 0,
  ).length;

  const totalRequiredCompleteSections = sectionProgress.filter(
    (section) => section.savedCount > 0 && section.missingRequiredCount === 0,
  ).length;

  const latestSavedAt = getLatestSavedAt(savedResponses);

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

  async function loadQuestionResponses(conditionId: string) {
    if (loading || !user || !conditionId) {
      return;
    }

    setIsLoadingResponses(true);
    setErrorMessage("");

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      const rows = await getQuestionResponses(token, workspaceId, conditionId);
      setSavedResponses(rows);
      setAnswers(mapResponsesToAnswers(rows));
      setHasUnsavedChanges(false);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Could not load GARP M-aware question responses.";

      setErrorMessage(message);
    } finally {
      setIsLoadingResponses(false);
    }
  }

  useEffect(() => {
    loadConditions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user, workspaceId]);

  useEffect(() => {
    if (selectedConditionId) {
      loadQuestionResponses(selectedConditionId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedConditionId]);

  function handleAnswerChange(questionId: string, value: GarpMAnswerValue) {
    setAnswers((current) => ({
      ...current,
      [questionId]: value,
    }));

    setHasUnsavedChanges(true);
    setStatusMessage("");
  }

  function handleResumeNextSection() {
    const nextIncompleteSection =
      sectionProgress.find(
        (section) =>
          section.savedCount === 0 || section.missingRequiredCount > 0,
      ) ?? sectionProgress[0];

    if (nextIncompleteSection) {
      setSelectedGroupKey(nextIncompleteSection.groupKey);
      setStatusMessage(`Resumed: ${nextIncompleteSection.title}`);
    }
  }

  async function handleSaveCurrentGroup() {
    setStatusMessage("");
    setErrorMessage("");
    setIsSaving(true);

    try {
      const token = await getTokenOrSetError();

      if (!token) {
        return;
      }

      if (!selectedConditionId) {
        setErrorMessage("Select a condition before saving question responses.");
        return;
      }

      const questionsToSave = selectedQuestions.filter((question) => {
        const answerValue = answers[question.id];
        return !isAnswerEmpty(answerValue);
      });

      if (questionsToSave.length === 0) {
        setErrorMessage("Add at least one answer before saving this section.");
        return;
      }

      for (const question of questionsToSave) {
        await createQuestionResponse(token, workspaceId, selectedConditionId, {
          questionGroup: toBackendQuestionGroup(question),
          questionKey: getBackendQuestionKey(question),
          questionText: question.questionText,
          answerText: stringifyAnswer(answers[question.id]),
          answerType: toBackendAnswerType(question.answerType),
        });
      }

      setStatusMessage(`Saved ${questionsToSave.length} response(s) for this section.`);
      setHasUnsavedChanges(false);
      await loadQuestionResponses(selectedConditionId);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Could not save GARP M-aware question responses.";

      setErrorMessage(message);
    } finally {
      setIsSaving(false);
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
          Sign in before using the GARP M-aware question engine.
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
          The GARP M-aware question engine saves answers against a specific condition.
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

  if (!selectedGroup) {
    return (
      <div className="rounded-2xl border border-red-300/30 bg-red-300/10 p-6 text-red-100">
        Question group could not be loaded.
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          GARP M-aware questions
        </p>

        <h1 className="mt-4 text-3xl font-bold">Structured question engine</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          Work through plain-English questions for a selected condition. Answers are saved to
          the existing question response API so they can be reused by later summaries, evidence
          prompts and document preparation workflows.
        </p>

        <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-5 text-sm leading-6 text-yellow-100">
          This feature helps organise information for preparation only. It does not calculate
          GARP M impairment points, estimate compensation, provide legal advice, provide medical
          advice, make a DVA decision, or guarantee a claim outcome.
        </div>

        <div className="mt-8">
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

          {selectedCondition && (
            <p className="mt-2 text-sm text-slate-400">
              Selected condition: {selectedCondition.conditionName}
            </p>
          )}
        </div>
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Save and resume
        </p>

        <h2 className="mt-4 text-2xl font-bold">Progress for this condition</h2>

        <p className="mt-4 text-slate-300">
          Saved answers reload when you return to this workspace and condition. You can leave
          the page and resume from the next incomplete section later.
        </p>

        <div className="mt-6 grid gap-4 md:grid-cols-4">
          <SummaryCard label="Sections started" value={totalSavedSections} />
          <SummaryCard label="Required sections complete" value={totalRequiredCompleteSections} />
          <SummaryCard label="Total sections" value={activeQuestionGroups.length} />
          <div className="rounded-xl border border-white/10 bg-slate-900 p-5">
            <p className="text-sm text-slate-400">Last saved</p>
            <p className="mt-2 text-sm font-semibold text-white">
              {latestSavedAt ? formatDateTime(latestSavedAt) : "Not saved yet"}
            </p>
          </div>
        </div>

        <button
          type="button"
          onClick={handleResumeNextSection}
          className="mt-6 w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200"
        >
          Continue next incomplete section
        </button>

        {hasUnsavedChanges && (
          <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm text-yellow-100">
            You have unsaved changes in this session. Use Save this section before leaving.
          </div>
        )}
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Sections
        </p>

        <div className="mt-6 grid gap-3 md:grid-cols-2">
          {activeQuestionGroups.map((group) => {
            const metadata = garpMQuestionGroupMetadata.find(
              (item) => item.groupKey === group.groupKey,
            );
            const progress = sectionProgress.find(
              (item) => item.groupKey === group.groupKey,
            );

            return (
              <button
                key={group.groupKey}
                type="button"
                onClick={() => setSelectedGroupKey(group.groupKey)}
                className={
                  selectedGroupKey === group.groupKey
                    ? "rounded-xl border border-cyan-300 bg-cyan-300/10 p-4 text-left"
                    : "rounded-xl border border-white/10 bg-slate-900 p-4 text-left hover:bg-white/5"
                }
              >
                <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <p className="font-semibold text-white">{group.title}</p>
                    <p className="mt-2 text-sm leading-6 text-slate-400">
                      {metadata?.whyThisMatters ?? group.description}
                    </p>
                  </div>

                  <span className="rounded-xl border border-white/10 px-3 py-2 text-xs text-slate-300">
                    {progress?.statusLabel ?? "Not started"}
                  </span>
                </div>

                <div className="mt-4 grid gap-2 text-xs text-slate-400 sm:grid-cols-3">
                  <p>{progress?.answeredCount ?? 0}/{group.questions.length} answered</p>
                  <p>{progress?.savedCount ?? 0} saved</p>
                  <p>{progress?.missingRequiredCount ?? 0} required missing</p>
                  <p>{progress?.validationIssueCount ?? 0} validation issue(s)</p>
                </div>

                {progress?.lastSavedAt && (
                  <p className="mt-3 text-xs text-cyan-200">
                    Last saved: {formatDateTime(progress.lastSavedAt)}
                  </p>
                )}
              </button>
            );
          })}
        </div>
      </section>

      {isLoadingResponses ? (
        <div className="rounded-2xl border border-white/10 bg-white/5 p-6 text-slate-300">
          Loading saved responses...
        </div>
      ) : (
        <GarpMQuestionGroupRenderer
          group={selectedGroup}
          answers={answers}
          disabled={isSaving}
          onAnswerChange={handleAnswerChange}
        />
      )}

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Save section
        </p>

        <h2 className="mt-4 text-2xl font-bold">Save answers for this section</h2>

        <p className="mt-4 text-slate-300">
          Saved answers are stored as question responses for the selected condition. If you save
          again, a new response record is created and the newest saved answer is used when the
          page reloads.
        </p>

        <div className="mt-6 grid gap-4 md:grid-cols-4">
          <SummaryCard label="Questions in section" value={selectedQuestions.length} />
          <SummaryCard label="Missing required" value={missingRequiredQuestions.length} />
          <SummaryCard label="Validation issues" value={selectedSectionProgress?.validationIssueCount ?? 0} />
          <SummaryCard label="Saved in section" value={selectedSectionProgress?.savedCount ?? 0} />
          <SummaryCard label="Saved responses loaded" value={savedResponses.length} />
        </div>

        {missingRequiredQuestions.length > 0 && (
          <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm text-yellow-100">
            Some required answers are still missing. You can still save partial answers and return
            later.
          </div>
        )}

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

        <button
          type="button"
          onClick={handleSaveCurrentGroup}
          disabled={isSaving}
          className="mt-6 w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
        >
          {isSaving ? "Saving answers..." : "Save this section"}
        </button>
      </section>
    </div>
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

function getBackendQuestionKey(question: GarpMQuestionTemplate) {
  return `garp_m:${question.id}`;
}

function stringifyAnswer(value?: GarpMAnswerValue) {
  if (!value) {
    return "";
  }

  if (Array.isArray(value)) {
    return value.join("|");
  }

  return value;
}

function isAnswerEmpty(value?: GarpMAnswerValue) {
  if (!value) {
    return true;
  }

  if (Array.isArray(value)) {
    return value.length === 0;
  }

  return value.trim().length === 0;
}

function toBackendAnswerType(answerType: GarpMQuestionAnswerType) {
  if (answerType === "YES_NO") {
    return "YES_NO";
  }

  if (answerType === "YES_NO_UNSURE") {
    return "YES_NO_UNSURE";
  }

  if (answerType === "DATE") {
    return "DATE";
  }

  if (answerType === "NUMBER") {
    return "NUMBER";
  }

  return "TEXT";
}

function mapResponsesToAnswers(responses: QuestionResponse[]) {
  const answers: GarpMAnswerMap = {};
  const sorted = responses
    .filter((response) => response.questionKey.startsWith("garp_m:"))
    .slice()
    .sort((a, b) => {
      const aDate = new Date(a.updatedAt || a.createdAt).getTime();
      const bDate = new Date(b.updatedAt || b.createdAt).getTime();

      return aDate - bDate;
    });

  for (const response of sorted) {
    const questionId = response.questionKey.replace("garp_m:", "");
    answers[questionId] = response.answerText ?? "";
  }

  return answers;
}

function getValidationIssueCountForQuestions(
  questions: GarpMQuestionTemplate[],
  answers: GarpMAnswerMap,
) {
  return questions.reduce((count, question) => {
    return count + getGarpMQuestionValidationMessages(question, answers[question.id]).length;
  }, 0);
}

function getSavedCountForQuestions(
  questions: GarpMQuestionTemplate[],
  responses: QuestionResponse[],
) {
  const savedKeys = new Set(
    responses
      .filter((response) => response.questionKey.startsWith("garp_m:"))
      .map((response) => response.questionKey),
  );

  return questions.filter((question) => savedKeys.has(getBackendQuestionKey(question))).length;
}

function getLastSavedAtForQuestions(
  questions: GarpMQuestionTemplate[],
  responses: QuestionResponse[],
) {
  const questionKeys = new Set(questions.map(getBackendQuestionKey));
  const matchingDates = responses
    .filter((response) => questionKeys.has(response.questionKey))
    .map((response) => new Date(response.updatedAt || response.createdAt))
    .filter((date) => !Number.isNaN(date.getTime()))
    .sort((a, b) => b.getTime() - a.getTime());

  return matchingDates[0] ?? null;
}

function getLatestSavedAt(responses: QuestionResponse[]) {
  const dates = responses
    .filter((response) => response.questionKey.startsWith("garp_m:"))
    .map((response) => new Date(response.updatedAt || response.createdAt))
    .filter((date) => !Number.isNaN(date.getTime()))
    .sort((a, b) => b.getTime() - a.getTime());

  return dates[0] ?? null;
}

function formatDateTime(value: Date) {
  return value.toLocaleString(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  });
}