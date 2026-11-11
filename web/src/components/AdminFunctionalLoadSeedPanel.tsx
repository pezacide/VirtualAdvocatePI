"use client";

import { useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import { getApiBaseUrl } from "@/lib/api/client";

type SeedAction = {
  key: string;
  label: string;
  description: string;
  path: string;
};

const seedActions: SeedAction[] = [
  {
    key: "templates",
    label: "Seed functional and load evidence templates and versions",
    description:
      "Creates the question, builder and annex templates and the disclaimer and prompt guardrail versions as DRAFT. Nothing is approved automatically. Existing keys are skipped.",
    path: "/api/v1/admin/functional-load-evidence/seed",
  },
  {
    key: "reference",
    label: "Seed load reference items",
    description:
      "Loads the starter set of approximate RAN/ADF equipment weights used by the load exposure lookup. Existing items are skipped.",
    path: "/api/v1/admin/reference/load-reference-items/seed",
  },
];

export function AdminFunctionalLoadSeedPanel() {
  const { user, loading, getIdToken } = useAuth();

  const [runningKey, setRunningKey] = useState<string | null>(null);
  const [results, setResults] = useState<Record<string, string>>({});
  const [errors, setErrors] = useState<Record<string, string>>({});

  async function runSeed(action: SeedAction) {
    setRunningKey(action.key);
    setResults((prev) => ({ ...prev, [action.key]: "" }));
    setErrors((prev) => ({ ...prev, [action.key]: "" }));

    try {
      const token = await getIdToken();

      if (!token) {
        setErrors((prev) => ({
          ...prev,
          [action.key]: "No Firebase ID token is available. Please sign in again.",
        }));
        return;
      }

      const response = await fetch(`${getApiBaseUrl()}${action.path}`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
          Accept: "application/json",
        },
      });

      const text = await response.text();

      if (!response.ok) {
        throw new Error(
          response.status === 403
            ? "Admin access is required to run this seed."
            : text || `HTTP ${response.status}`,
        );
      }

      setResults((prev) => ({ ...prev, [action.key]: text }));
    } catch (error) {
      const message = error instanceof Error ? error.message : "Seed request failed.";
      setErrors((prev) => ({ ...prev, [action.key]: message }));
    } finally {
      setRunningKey(null);
    }
  }

  if (loading) {
    return <p className="text-slate-300">Checking sign-in status...</p>;
  }

  if (!user) {
    return <p className="text-slate-300">Sign in before running seeds.</p>;
  }

  return (
    <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
        Functional and load evidence
      </p>

      <h1 className="mt-4 text-3xl font-bold text-white">Admin seeds</h1>

      <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
        These admin-only actions load starter content for the functional and load evidence tools.
        Each can be run more than once safely.
      </p>

      <div className="mt-6 space-y-8">
        {seedActions.map((action) => (
          <div key={action.key}>
            <p className="max-w-4xl text-sm leading-6 text-slate-300">{action.description}</p>

            <button
              type="button"
              onClick={() => runSeed(action)}
              disabled={runningKey !== null}
              className="mt-3 rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
            >
              {runningKey === action.key ? "Seeding..." : action.label}
            </button>

            {results[action.key] && (
              <pre className="mt-4 overflow-auto rounded-xl border border-emerald-300/30 bg-emerald-300/10 p-4 text-sm text-emerald-100">
                {results[action.key]}
              </pre>
            )}

            {errors[action.key] && (
              <pre className="mt-4 overflow-auto rounded-xl border border-red-300/30 bg-red-300/10 p-4 text-sm text-red-100">
                {errors[action.key]}
              </pre>
            )}
          </div>
        ))}
      </div>
    </section>
  );
}
