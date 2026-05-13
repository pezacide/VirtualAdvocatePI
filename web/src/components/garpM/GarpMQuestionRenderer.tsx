"use client";

import { useRef } from "react";
import {
  GarpMQuestionGroupTemplate,
  GarpMQuestionOption,
  GarpMQuestionTemplate,
  yesNoUnsureOptions,
} from "@/lib/garpM";

export type GarpMAnswerValue = string | string[];

export type GarpMAnswerMap = Record<string, GarpMAnswerValue | undefined>;

type GarpMQuestionRendererProps = {
  question: GarpMQuestionTemplate;
  value?: GarpMAnswerValue;
  disabled?: boolean;
  onAnswerChange: (questionId: string, value: GarpMAnswerValue) => void;
};

type GarpMQuestionGroupRendererProps = {
  group: GarpMQuestionGroupTemplate;
  answers: GarpMAnswerMap;
  disabled?: boolean;
  onAnswerChange: (questionId: string, value: GarpMAnswerValue) => void;
};

const yesNoOptions: GarpMQuestionOption[] = [
  { value: "YES", label: "Yes" },
  { value: "NO", label: "No" },
];

export function GarpMQuestionGroupRenderer({
  group,
  answers,
  disabled = false,
  onAnswerChange,
}: GarpMQuestionGroupRendererProps) {
  const orderedQuestions = group.questions
    .slice()
    .sort((a, b) => a.displayOrder - b.displayOrder);

  return (
    <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
        Question group
      </p>

      <h2 className="mt-4 text-2xl font-bold">{group.title}</h2>

      <p className="mt-4 max-w-3xl text-slate-300">{group.description}</p>

      {group.safetyNote && (
        <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm leading-6 text-yellow-100">
          {group.safetyNote}
        </div>
      )}

      {orderedQuestions.length === 0 ? (
        <div className="mt-8 rounded-xl border border-white/10 bg-slate-900 p-5 text-sm text-slate-300">
          No questions have been added to this group yet.
        </div>
      ) : (
        <div className="mt-8 space-y-5">
          {orderedQuestions.map((question) => (
            <GarpMQuestionRenderer
              key={question.id}
              question={question}
              value={answers[question.id]}
              disabled={disabled}
              onAnswerChange={onAnswerChange}
            />
          ))}
        </div>
      )}
    </section>
  );
}

export function GarpMQuestionRenderer({
  question,
  value,
  disabled = false,
  onAnswerChange,
}: GarpMQuestionRendererProps) {
  const isRequired = question.requirementLevel === "REQUIRED";
  const isMissing = isRequired && isGarpMAnswerMissing(question, value);
  const validationMessages = getGarpMQuestionValidationMessages(question, value);

  return (
    <article
      className={
        isMissing
          ? "rounded-2xl border border-yellow-300/40 bg-yellow-300/10 p-5"
          : "rounded-2xl border border-white/10 bg-slate-900 p-5"
      }
    >
      <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
        <div>
          <p className="font-mono text-xs text-cyan-200">
            {question.groupKey} · {question.questionKey}
          </p>

          <h3 className="mt-3 text-lg font-semibold text-white">
            {question.questionText}
          </h3>

          <p className="mt-3 text-sm leading-6 text-slate-300">
            {question.helperText}
          </p>
        </div>

        <div className="flex flex-wrap gap-2">
          <span className="rounded-full border border-white/10 px-3 py-1 text-xs text-slate-300">
            {question.requirementLevel}
          </span>

          <span className="rounded-full border border-white/10 px-3 py-1 text-xs text-slate-300">
            {question.answerType}
          </span>
        </div>
      </div>

      <div className="mt-5">
        <QuestionInput
          question={question}
          value={value}
          disabled={disabled}
          onAnswerChange={onAnswerChange}
        />
      </div>

{isMissing && (
        <p className="mt-4 text-sm text-yellow-100">
          This required answer is still missing.
        </p>
      )}

      {validationMessages.length > 0 && (
        <div className="mt-4 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm text-yellow-100">
          <p className="font-semibold">Check this answer</p>
          <ul className="mt-2 list-inside list-disc space-y-1">
            {validationMessages.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ul>
        </div>
      )}

      {question.summaryLabel && (
        <p className="mt-4 text-xs text-slate-500">
          Summary label: {question.summaryLabel}
        </p>
      )}

      {question.safetyNote && (
        <p className="mt-4 text-xs leading-5 text-slate-500">
          {question.safetyNote}
        </p>
      )}
    </article>
  );
}

type QuestionInputProps = {
  question: GarpMQuestionTemplate;
  value?: GarpMAnswerValue;
  disabled: boolean;
  onAnswerChange: (questionId: string, value: GarpMAnswerValue) => void;
};

function QuestionInput({
  question,
  value,
  disabled,
  onAnswerChange,
}: QuestionInputProps) {
  const dateInputRef = useRef<HTMLInputElement>(null);
  const stringValue = getStringAnswerValue(value);
  const options = getQuestionOptions(question);

  if (question.answerType === "LONG_TEXT") {
    return (
      <textarea
        value={stringValue}
        disabled={disabled}
        rows={5}
        onChange={(event) => onAnswerChange(question.id, event.target.value)}
        className="w-full rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white outline-none focus:border-cyan-300 disabled:opacity-60"
      />
    );
  }

  if (question.answerType === "TEXT") {
    return (
      <input
        type="text"
        value={stringValue}
        disabled={disabled}
        onChange={(event) => onAnswerChange(question.id, event.target.value)}
        className="w-full rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white outline-none focus:border-cyan-300 disabled:opacity-60"
      />
    );
  }

  if (question.answerType === "DATE") {
    return (
      <div className="grid gap-3 sm:grid-cols-[1fr_auto]">
        <input
          ref={dateInputRef}
          type="date"
          value={stringValue}
          disabled={disabled}
          onChange={(event) => onAnswerChange(question.id, event.target.value)}
          className="w-full rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white outline-none focus:border-cyan-300 disabled:opacity-60"
        />

        <button
          type="button"
          disabled={disabled}
          onClick={() => {
            const dateInput = dateInputRef.current as
              | (HTMLInputElement & { showPicker?: () => void })
              | null;

            if (dateInput?.showPicker) {
              dateInput.showPicker();
              return;
            }

            dateInput?.focus();
          }}
          className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
        >
          Choose date
        </button>
      </div>
    );
  }

  if (question.answerType === "NUMBER") {
    return (
      <input
        type="number"
        value={stringValue}
        disabled={disabled}
        onChange={(event) => onAnswerChange(question.id, event.target.value)}
        className="w-full rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white outline-none focus:border-cyan-300 disabled:opacity-60"
      />
    );
  }

  if (
    question.answerType === "YES_NO" ||
    question.answerType === "YES_NO_UNSURE"
  ) {
    return (
      <div className="grid gap-3 sm:grid-cols-3">
        {options.map((option) => (
          <label
            key={option.value}
            className={
              stringValue === option.value
                ? "cursor-pointer rounded-xl border border-cyan-300 bg-cyan-300/10 p-4 text-sm text-cyan-100"
                : "cursor-pointer rounded-xl border border-white/10 bg-slate-950 p-4 text-sm text-slate-200"
            }
          >
            <input
              type="radio"
              name={question.id}
              value={option.value}
              checked={stringValue === option.value}
              disabled={disabled}
              onChange={(event) => onAnswerChange(question.id, event.target.value)}
              className="mr-2"
            />
            {option.label}
          </label>
        ))}
      </div>
    );
  }

  if (question.answerType === "SINGLE_SELECT") {
    return (
      <select
        value={stringValue}
        disabled={disabled}
        onChange={(event) => onAnswerChange(question.id, event.target.value)}
        className="w-full rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white outline-none focus:border-cyan-300 disabled:opacity-60"
      >
        <option value="">Select an answer</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    );
  }

  if (question.answerType === "MULTI_SELECT") {
    const selectedValues = getMultiAnswerValue(value);

    return (
      <div className="grid gap-3">
        {options.map((option) => (
          <label
            key={option.value}
            className="flex cursor-pointer items-start gap-3 rounded-xl border border-white/10 bg-slate-950 p-4 text-sm text-slate-200"
          >
            <input
              type="checkbox"
              value={option.value}
              checked={selectedValues.includes(option.value)}
              disabled={disabled}
              onChange={(event) => {
                const nextValues = updateMultiAnswerValue(
                  selectedValues,
                  option.value,
                  event.target.checked,
                );

                onAnswerChange(question.id, nextValues);
              }}
              className="mt-1"
            />

            <span>
              <span className="block font-medium">{option.label}</span>
              {option.helperText && (
                <span className="mt-1 block text-slate-400">{option.helperText}</span>
              )}
            </span>
          </label>
        ))}
      </div>
    );
  }

  return (
    <textarea
      value={stringValue}
      disabled={disabled}
      rows={4}
      onChange={(event) => onAnswerChange(question.id, event.target.value)}
      className="w-full rounded-xl border border-white/10 bg-slate-950 px-4 py-3 text-white outline-none focus:border-cyan-300 disabled:opacity-60"
    />
  );
}

export function isGarpMAnswerMissing(
  question: GarpMQuestionTemplate,
  value?: GarpMAnswerValue,
) {
  if (question.answerType === "MULTI_SELECT") {
    return getMultiAnswerValue(value).length === 0;
  }

  return !getStringAnswerValue(value).trim();
}

export function getGarpMQuestionValidationMessages(
  question: GarpMQuestionTemplate,
  value?: GarpMAnswerValue,
) {
  const messages: string[] = [];
  const stringValue = getStringAnswerValue(value).trim();

  if (question.requirementLevel === "REQUIRED" && isGarpMAnswerMissing(question, value)) {
    messages.push("This answer is required before the section can be considered complete.");
  }

  for (const rule of question.validationRules ?? []) {
    if (rule.type === "MIN_LENGTH") {
      const minLength = Number(rule.value ?? 0);

      if (stringValue.length > 0 && stringValue.length < minLength) {
        messages.push(rule.message ?? `Enter at least ${minLength} characters.`);
      }
    }

    if (rule.type === "MAX_LENGTH") {
      const maxLength = Number(rule.value ?? 0);

      if (maxLength > 0 && stringValue.length > maxLength) {
        messages.push(rule.message ?? `Keep this answer under ${maxLength} characters.`);
      }
    }
  }

  return Array.from(new Set(messages));
}

export function getMissingRequiredGarpMQuestions(
  questions: GarpMQuestionTemplate[],
  answers: GarpMAnswerMap,
) {
  return questions.filter(
    (question) =>
      question.requirementLevel === "REQUIRED" &&
      isGarpMAnswerMissing(question, answers[question.id]),
  );
}

function getQuestionOptions(question: GarpMQuestionTemplate) {
  if (question.options && question.options.length > 0) {
    return question.options;
  }

  if (question.answerType === "YES_NO") {
    return yesNoOptions;
  }

  if (question.answerType === "YES_NO_UNSURE") {
    return yesNoUnsureOptions;
  }

  return [];
}

function getStringAnswerValue(value?: GarpMAnswerValue) {
  if (!value) {
    return "";
  }

  if (Array.isArray(value)) {
    return value.join(", ");
  }

  return value;
}

function getMultiAnswerValue(value?: GarpMAnswerValue) {
  if (!value) {
    return [];
  }

  if (Array.isArray(value)) {
    return value;
  }

  return value
    .split("|")
    .map((item) => item.trim())
    .filter(Boolean);
}

function updateMultiAnswerValue(
  currentValues: string[],
  optionValue: string,
  isChecked: boolean,
) {
  if (isChecked) {
    return Array.from(new Set([...currentValues, optionValue]));
  }

  return currentValues.filter((value) => value !== optionValue);
}