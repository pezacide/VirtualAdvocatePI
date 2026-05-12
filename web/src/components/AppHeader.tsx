"use client";

import Link from "next/link";
import { useAuth } from "@/components/AuthProvider";

export function AppHeader() {
  const { user, loading, signOutUser } = useAuth();

  async function handleSignOut() {
    await signOutUser();
  }

  return (
    <header className="border-b border-white/10 bg-slate-950/95 px-6 py-4 text-white">
      <div className="mx-auto flex max-w-6xl flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <Link href="/" className="font-semibold tracking-wide text-cyan-300">
          Virtual Advocate PI
        </Link>

        <nav className="flex flex-wrap items-center gap-3 text-sm">
          <Link href="/dashboard" className="text-slate-300 hover:text-white">
            Dashboard
          </Link>

          <Link href="/session-check" className="text-slate-300 hover:text-white">
            Session
          </Link>

          <Link href="/env-check" className="text-slate-300 hover:text-white">
            Env
          </Link>

          {loading ? (
            <span className="text-slate-400">Checking session...</span>
          ) : user ? (
            <>
              <span className="text-slate-300">{user.email}</span>
              <button
                type="button"
                onClick={handleSignOut}
                className="rounded-lg border border-white/20 px-3 py-2 text-white hover:bg-white/10"
              >
                Sign out
              </button>
            </>
          ) : (
            <Link
              href="/login"
              className="rounded-lg bg-cyan-300 px-3 py-2 font-semibold text-slate-950 hover:bg-cyan-200"
            >
              Sign in
            </Link>
          )}
        </nav>
      </div>
    </header>
  );
}