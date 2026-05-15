"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import { AdminMeResponse, getAdminMe, getAdminPing } from "@/lib/api";

export function AdminAccessCheckPanel() {
  const { user, loading, getIdToken } = useAuth();

  const [adminStatus, setAdminStatus] = useState<AdminMeResponse | null>(null);
  const [pingStatus, setPingStatus] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [isChecking, setIsChecking] = useState(false);

  async function checkAdminAccess() {
    setIsChecking(true);
    setErrorMessage("");
    setPingStatus("");

    try {
      const token = await getIdToken();

      if (!token) {
        setErrorMessage("No Firebase ID token is available. Please sign in again.");
        return;
      }

      const me = await getAdminMe(token);
      setAdminStatus(me);

      if (me.isAdmin) {
        const ping = await getAdminPing(token);
        setPingStatus(ping.message);
      }
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Could not check admin access.";
      setErrorMessage(message);
    } finally {
      setIsChecking(false);
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
        <h1 className="text-2xl font-bold text-white">Admin access check</h1>
        <p className="mt-4 text-sm text-slate-300">Sign in before checking admin access.</p>
      </section>
    );
  }

  return (
    <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
        Admin access
      </p>

      <h1 className="mt-4 text-3xl font-bold text-white">
        Admin roles and access control check
      </h1>

      <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
        This page confirms whether the signed-in account has access to protected
        admin endpoints. Normal users should see admin access denied.
      </p>

      <div className="mt-6 rounded-xl border border-white/10 bg-slate-950 p-5 text-sm leading-6 text-slate-300">
        <p>Email: {user.email}</p>
        <p>Firebase UID: {user.uid}</p>
      </div>

      <button
        type="button"
        onClick={checkAdminAccess}
        disabled={isChecking}
        className="mt-6 rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-cyan-200 disabled:opacity-60"
      >
        {isChecking ? "Checking..." : "Check admin access"}
      </button>

      {adminStatus && (
        <div
          className={`mt-6 rounded-xl border p-5 text-sm leading-6 ${
            adminStatus.isAdmin
              ? "border-emerald-300/30 bg-emerald-300/10 text-emerald-100"
              : "border-yellow-300/30 bg-yellow-300/10 text-yellow-100"
          }`}
        >
          <p>Role: {adminStatus.role}</p>
          <p>Account status: {adminStatus.accountStatus}</p>
          <p>Is admin: {adminStatus.isAdmin ? "Yes" : "No"}</p>
          <p>Reason: {adminStatus.reason}</p>
          {pingStatus && <p>Protected ping: {pingStatus}</p>}
        </div>
      )}

      {errorMessage && (
        <div className="mt-6 rounded-xl border border-red-300/30 bg-red-300/10 p-5 text-sm text-red-100">
          {errorMessage}
        </div>
      )}
    </section>
  );
}