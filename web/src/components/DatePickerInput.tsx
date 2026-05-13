"use client";

import { useRef } from "react";

type DatePickerInputProps = {
  id: string;
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
  buttonLabel?: string;
};

export function DatePickerInput({
  id,
  value,
  onChange,
  disabled = false,
  buttonLabel = "Choose date",
}: DatePickerInputProps) {
  const inputRef = useRef<HTMLInputElement>(null);

  return (
    <div className="grid gap-3 sm:grid-cols-[1fr_auto]">
      <input
        ref={inputRef}
        id={id}
        type="date"
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.target.value)}
        className="w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300 disabled:opacity-60"
      />

      <button
        type="button"
        disabled={disabled}
        onClick={() => {
          const dateInput = inputRef.current as
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
        {buttonLabel}
      </button>
    </div>
  );
}