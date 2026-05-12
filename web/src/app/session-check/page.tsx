"use client";

import Link from "next/link";
import { useState } from "react";
import { useAuth } from "@/components/AuthProvider";

export default function SessionCheckPage() {
  const { user, loading, getIdToken } = useAuth();
  const [tokenPreview, setTokenPreview] = useState("");

  async function handleShowTokenPreview() {
    const token = await getIdToken();

    if (!token) {
      setTokenPreview("No token available. Sign in first.");
      return;
    }

    setTokenPreview(`${token.slice(0, 32)}...`);
  }

  return (
    <main className="min-h-screen bg-slate-950 px-6 py-12 text-white">
      <div className="mx-auto max-w-4xl">
        <Link href="/" className="text-sm text-cyan-300 hover:text-cyan-200">
          ← Back to home
        </Link>

        <section className="mt-10 rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Session check
          </p>

          <h1 className="mt-4 text-3xl font-bold">Firebase session state</h1>

          <div className="mt-6 rounded-xl bg-slate-900 p-5 text-sm text-slate-200">
            {loading ? (
              <p>Checking session...</p>
            ) : user ? (
              <div className="space-y-2">
                <p className="text-green-300">Signed in</p>
                <p>Email: {user.email}</p>
                <p>User ID: {user.uid}</p>
              </div>
            ) : (
              <p className="text-yellow-300">Not signed in.</p>
            )}
          </div>

          <button
            type="button"
            onClick={handleShowTokenPreview}
            className="mt-6 rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200"
          >
            Show Firebase token preview
          </button>

          {tokenPreview && (
            <pre className="mt-6 overflow-x-auto rounded-xl bg-slate-900 p-5 text-sm text-cyan-200">
              {tokenPreview}
            </pre>
          )}
        </section>
      </div>
    </main>
  );
}