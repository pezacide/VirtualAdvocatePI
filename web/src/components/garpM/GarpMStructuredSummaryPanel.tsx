"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import {
  ClaimCondition,
  QuestionResponse,
  getClaimConditions,
  getQuestionResponses,
} from "@/lib/api";
import {
  GarpMQuestionTemplate,
  garpMQuestionGroupTemplateSet,
  getAllQuestions,
} from "@/lib/garpM";

type GarpMStructuredSummaryPanelProps = {
  workspaceId: string;
};

type SummaryAnswer = {
  question: GarpMQuestionTemplate;
  answerText: string;
  updatedAt: string;
};

type SummarySection = {
  title: string;
  description: string;
  answers: SummaryAnswer[];
  missingRequired: GarpMQuestionTemplate[];
};

export function GarpMStructuredSummaryPanel({
  workspaceId,
}: GarpMStructuredSummaryPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [selectedConditionId, setSelectedConditionId] = useState("");
  const [responses, setResponses] = useState<QuestionResponse[]>([]);

  const [isLoadingConditions, setIsLoadingConditions] = useState(false);
  const [isLoadingResponses, setIsLoadingResponses] = useState(false);

  const [statusMessage, setStatusMessage] = useState("");
  const [copyStatusMessage, setCopyStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  const allQuestions = useMemo(() => getAllQuestions(garpMQuestionGroupTemplateSet), []);

  const questionById = useMemo(() => {
    const map = new Map<string, GarpMQuestionTemplate>();

    for (const question of allQuestions) {
      map.set(question.id, question);
    }

    return map;
  }, [allQuestions]);

  const latestAnswers = useMemo(() => {
    return mapResponsesToLatestAnswers(responses, questionById);
  }, [responses, questionById]);

  const selectedCondition = conditions.find(
    (condition) => condition.id === selectedConditionId,
  );

  const summarySections = useMemo(() => {
    return garpMQuestionGroupTemplateSet.groups
      .filter((group) => group.questions.length > 0)
      .map<SummarySection>((group) => {
        const answers = group.questions
          .map((question) => {
            const answer = latestAnswers.get(question.id);

            if (!answer) {
              return null;
            }

            return {
              question,
              answerText: formatAnswer(question, answer.answerText),
              updatedAt: answer.updatedAt,
            };
          })
          .filter((answer): answer is SummaryAnswer => answer !== null);

        const missingRequired = group.questions.filter((question) => {
          if (question.requirementLevel !== "REQUIRED") {
            return false;
          }

          const answer = latestAnswers.get(question.id);
          return !answer || !answer.answerText.trim();
        });

        return {
          title: group.title,
          description: group.description,
          answers,
          missingRequired,
        };
      });
  }, [latestAnswers]);

  const totalQuestions = allQuestions.length;
  const uniqueAnsweredCount = latestAnswers.size;
  const requiredQuestions = allQuestions.filter(
    (question) => question.requirementLevel === "REQUIRED",
  );
  const missingRequiredQuestions = requiredQuestions.filter(
    (question) => !latestAnswers.get(question.id)?.answerText?.trim(),
  );
  const latestSavedAt = getLatestSavedAt(responses);

  const plainEnglishSummary = useMemo(() => {
    return buildPlainEnglishSummary({
      conditionName: selectedCondition?.conditionName ?? "Selected condition",
      sections: summarySections,
      missingRequiredQuestions,
      latestSavedAt,
    });
  }, [latestSavedAt, missingRequiredQuestions, selectedCondition, summarySections]);

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

  async function loadResponses(conditionId: string) {
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
      setResponses(rows);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Could not load saved GARP M-aware answers.";

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
      loadResponses(selectedConditionId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedConditionId]);

  async function handleCopySummary() {
    setStatusMessage("");
    setCopyStatusMessage("");
    setErrorMessage("");

    try {
      await navigator.clipboard.writeText(plainEnglishSummary);
      setStatusMessage("Summary copied to clipboard.");
      setCopyStatusMessage("Copied to clipboard. You can now paste it into notes, an email, or another document.");
    } catch {
      setCopyStatusMessage("Copy did not complete automatically. You can still select the text box and copy it manually.");
      setErrorMessage("Could not copy automatically. You can still select and copy the summary text manually.");
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
          Sign in before viewing the structured summary.
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
          The structured summary needs a saved condition and saved question answers.
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

  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Structured summary
        </p>

        <h1 className="mt-4 text-3xl font-bold">GARP M-aware preparation summary</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          Review saved answers for a selected condition. This page turns user-provided answers
          into a plain-English preparation summary for discussion with a doctor, advocate,
          lawyer or support person.
        </p>

        <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-5 text-sm leading-6 text-yellow-100">
          This summary is preparation support only. It does not calculate GARP M impairment
          points, estimate compensation, provide legal advice, provide medical advice, make a
          DVA decision, or guarantee a claim outcome.
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
        </div>
      </section>

      {isLoadingResponses ? (
        <div className="rounded-2xl border border-white/10 bg-white/5 p-6 text-slate-300">
          Loading saved answers...
        </div>
      ) : (
        <>
          <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
            <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
              Readiness snapshot
            </p>

            <div className="mt-6 grid gap-4 md:grid-cols-4">
              <SummaryCard label="Saved answers" value={uniqueAnsweredCount} />
              <SummaryCard label="Total questions" value={totalQuestions} />
              <SummaryCard label="Required missing" value={missingRequiredQuestions.length} />
              <div className="rounded-xl border border-white/10 bg-slate-900 p-5">
                <p className="text-sm text-slate-400">Last saved</p>
                <p className="mt-2 text-sm font-semibold text-white">
                  {latestSavedAt ? formatDateTime(latestSavedAt) : "Not saved yet"}
                </p>
              </div>
            </div>

            {missingRequiredQuestions.length > 0 && (
              <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-5 text-sm text-yellow-100">
                <p className="font-semibold">Missing required answers</p>
                <ul className="mt-3 list-inside list-disc space-y-1">
                  {missingRequiredQuestions.map((question) => (
                    <li key={question.id}>{question.questionText}</li>
                  ))}
                </ul>
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
          </section>

          <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
            <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
              Section review
            </p>

            <div className="mt-6 space-y-5">
              {summarySections.map((section) => (
                <article
                  key={section.title}
                  className="rounded-2xl border border-white/10 bg-slate-900 p-5"
                >
                  <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                    <div>
                      <h2 className="text-xl font-semibold text-white">{section.title}</h2>
                      <p className="mt-2 text-sm leading-6 text-slate-400">
                        {section.description}
                      </p>
                    </div>

                    <span className="rounded-xl border border-white/10 px-3 py-2 text-xs text-slate-300">
                      {section.answers.length} saved answer(s)
                    </span>
                  </div>

                  {section.answers.length === 0 ? (
                    <p className="mt-5 text-sm text-slate-400">
                      No saved answers yet for this section.
                    </p>
                  ) : (
                    <dl className="mt-5 space-y-4">
                      {section.answers.map((answer) => (
                        <div key={answer.question.id}>
                          <dt className="text-sm font-semibold text-cyan-100">
                            {answer.question.summaryLabel}
                          </dt>
                          <dd className="mt-1 whitespace-pre-wrap text-sm leading-6 text-slate-300">
                            {answer.answerText}
                          </dd>
                        </div>
                      ))}
                    </dl>
                  )}

                  {section.missingRequired.length > 0 && (
                    <div className="mt-5 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm text-yellow-100">
                      <p className="font-semibold">Required answers still missing</p>
                      <ul className="mt-2 list-inside list-disc space-y-1">
                        {section.missingRequired.map((question) => (
                          <li key={question.id}>{question.questionText}</li>
                        ))}
                      </ul>
                    </div>
                  )}
                </article>
              ))}
            </div>
          </section>

          <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
            <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
              Plain-English summary
            </p>

            <h2 className="mt-4 text-2xl font-bold">Copyable preparation summary</h2>

            <p className="mt-4 text-slate-300">
              Review this text before using it anywhere else. It is based on saved user-provided
              answers only.
            </p>

            <textarea
              readOnly
              value={plainEnglishSummary}
              rows={18}
              className="mt-6 w-full rounded-xl border border-white/10 bg-slate-950 px-4 py-3 font-mono text-sm leading-6 text-slate-100 outline-none"
            />

            <button
              type="button"
              onClick={handleCopySummary}
              className="mt-5 w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200"
            >
              {copyStatusMessage ? "Copied" : "Copy summary"}
            </button>

            {copyStatusMessage && (
              <div className="mt-4 rounded-xl border border-green-300/30 bg-green-300/10 p-4 text-sm text-green-100">
                {copyStatusMessage}
              </div>
            )}
          </section>
        </>
      )}
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

function mapResponsesToLatestAnswers(
  responses: QuestionResponse[],
  questionById: Map<string, GarpMQuestionTemplate>,
) {
  const latestAnswers = new Map<
    string,
    {
      answerText: string;
      updatedAt: string;
    }
  >();

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

    if (!questionById.has(questionId)) {
      continue;
    }

    latestAnswers.set(questionId, {
      answerText: response.answerText ?? "",
      updatedAt: response.updatedAt || response.createdAt,
    });
  }

  return latestAnswers;
}

function formatAnswer(question: GarpMQuestionTemplate, answerText: string) {
  if (!answerText.trim()) {
    return "";
  }

  if (!question.options || question.options.length === 0) {
    return answerText;
  }

  const optionMap = new Map(question.options.map((option) => [option.value, option.label]));
  const parts = answerText.split("|").map((part) => part.trim()).filter(Boolean);

  if (parts.length <= 1) {
    return optionMap.get(answerText) ?? answerText;
  }

  return parts.map((part) => optionMap.get(part) ?? part).join(", ");
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

function buildPlainEnglishSummary({
  conditionName,
  sections,
  missingRequiredQuestions,
  latestSavedAt,
}: {
  conditionName: string;
  sections: SummarySection[];
  missingRequiredQuestions: GarpMQuestionTemplate[];
  latestSavedAt: Date | null;
}) {
  const lines: string[] = [];

  lines.push("GARP M-aware preparation summary");
  lines.push("");
  lines.push(`Condition: ${conditionName}`);
  lines.push(`Last saved: ${latestSavedAt ? formatDateTime(latestSavedAt) : "Not saved yet"}`);
  lines.push("");
  lines.push("Important boundary:");
  lines.push(
    "This summary is preparation support only. It does not calculate GARP M impairment points, estimate compensation, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.",
  );
  lines.push("");

  for (const section of sections) {
    lines.push(section.title);
    lines.push("-".repeat(section.title.length));

    if (section.answers.length === 0) {
      lines.push("No saved answers yet.");
      lines.push("");
      continue;
    }

    for (const answer of section.answers) {
      lines.push(`${answer.question.summaryLabel}:`);
      lines.push(answer.answerText || "No answer recorded.");
      lines.push("");
    }
  }

  lines.push("Missing required answers");
  lines.push("------------------------");

  if (missingRequiredQuestions.length === 0) {
    lines.push("No required answers are currently missing.");
  } else {
    for (const question of missingRequiredQuestions) {
      lines.push(`- ${question.questionText}`);
    }
  }

  lines.push("");
  lines.push("Suggested next step:");
  lines.push(
    "Review this summary, correct anything that is incomplete or unclear, and discuss relevant points with a doctor, advocate, lawyer or support person where appropriate.",
  );

  return lines.join("\n");
}