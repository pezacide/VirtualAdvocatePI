"use client";

import { useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import { getApiBaseUrl } from "@/lib/api/client";

export function AdminSourceRegistrySeedPanel() {
  const { user, loading, getIdToken } = useAuth();

  const [result, setResult] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [isSeeding, setIsSeeding] = useState(false);

  async function seedApprovedSources() {
    setIsSeeding(true);
    setResult("");
    setErrorMessage("");

    try {
      const token = await getIdToken();

      if (!token) {
        setErrorMessage("No Firebase ID token is available. Please sign in again.");
        return;
      }

      const response = await fetch(
        `${getApiBaseUrl()}/api/v1/admin/source-registry/seed-approved`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
            Accept: "application/json",
          },
        },
      );

      const text = await response.text();

      if (!response.ok) {
        throw new Error(text || `HTTP ${response.status}`);
      }

      setResult(text);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not seed approved source registry.";
      setErrorMessage(message);
    } finally {
      setIsSeeding(false);
    }
  }

  if (loading) {
    return <p className="text-slate-300">Checking sign-in status...</p>;
  }

  if (!user) {
    return <p className="text-slate-300">Sign in before seeding approved sources.</p>;
  }

  return (
    <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
        Source registry seed
      </p>

      <h1 className="mt-4 text-3xl font-bold text-white">
        Seed approved source registry entries
      </h1>

      <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
        This admin-only action loads approved source registry entries from the bundled seed JSON.
        Existing source keys are skipped, so this can be run more than once.
      </p>

      <button
        type="button"
        onClick={seedApprovedSources}
        disabled={isSeeding}
        className="mt-6 rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
      >
        {isSeeding ? "Seeding..." : "Seed approved sources"}
      </button>

      {result && (
        <pre className="mt-6 overflow-auto rounded-xl border border-emerald-300/30 bg-emerald-300/10 p-4 text-sm text-emerald-100">
          {result}
        </pre>
      )}

      {errorMessage && (
        <pre className="mt-6 overflow-auto rounded-xl border border-red-300/30 bg-red-300/10 p-4 text-sm text-red-100">
          {errorMessage}
        </pre>
      )}
    </section>
  );
}