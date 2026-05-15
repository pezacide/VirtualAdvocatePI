"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import { AdminMeResponse, getAdminMe } from "@/lib/api";

type AdminPlaceholderPanelProps = {
  title: string;
  description: string;
  nextTask: string;
};

export function AdminPlaceholderPanel({
  title,
  description,
  nextTask,
}: AdminPlaceholderPanelProps) {
  const { user, loading, getIdToken } = useAuth();

  const [adminStatus, setAdminStatus] = useState<AdminMeResponse | null>(null);
  const [errorMessage, setErrorMessage] = useState("");

  async function checkAdminAccess() {
    setErrorMessage("");

    try {
      const token = await getIdToken();

      if (!token) {
        setErrorMessage("No Firebase ID token is available. Please sign in again.");
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

  useEffect(() => {
    if (!loading && user) {
      checkAdminAccess();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user]);

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
    <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
        Admin tool
      </p>

      <h1 className="mt-4 text-3xl font-bold text-white">{title}</h1>

      <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
        {description}
      </p>

      <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm font-semibold leading-6 text-yellow-100">
        This admin page shell is ready. The implementation task is: {nextTask}.
      </div>

      {adminStatus?.isAdmin && (
        <div className="mt-6 rounded-xl border border-emerald-300/30 bg-emerald-300/10 p-4 text-sm leading-6 text-emerald-100">
          <p>Admin access confirmed.</p>
          <p>Email: {adminStatus.email}</p>
          <p>Role: {adminStatus.role}</p>
        </div>
      )}

      {errorMessage && (
        <div className="mt-6 rounded-xl border border-red-300/30 bg-red-300/10 p-4 text-sm text-red-100">
          {errorMessage}
        </div>
      )}

      <Link
        href="/admin"
        className="mt-6 inline-flex rounded-xl border border-cyan-300/40 px-5 py-3 text-sm font-semibold text-cyan-100 hover:bg-cyan-300/10"
      >
        ← Back to admin dashboard
      </Link>
    </section>
  );
}