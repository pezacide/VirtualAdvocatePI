"use client";

import Link from "next/link";
import { useAuth } from "@/components/AuthProvider";

export function AuthStatusPanel() {
  const { user, loading } = useAuth();

  if (loading) {
    return (
      <div className="rounded-xl border border-white/10 bg-slate-900 p-5 text-sm text-slate-300">
        Checking session...
      </div>
    );
  }

  if (!user) {
    return (
      <div className="rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-5 text-sm text-yellow-100">
        <p className="font-semibold">Not signed in</p>
        <p className="mt-2">
          Sign in to connect the web shell to your Firebase account.
        </p>
        <Link
          href="/login"
          className="mt-4 inline-flex rounded-lg bg-cyan-300 px-4 py-2 font-semibold text-slate-950 hover:bg-cyan-200"
        >
          Go to login
        </Link>
      </div>
    );
  }

  return (
    <div className="rounded-xl border border-green-300/30 bg-green-300/10 p-5 text-sm text-green-100">
      <p className="font-semibold text-green-200">Signed in</p>
      <p className="mt-2">Email: {user.email}</p>
      <p className="mt-1 break-all">Firebase UID: {user.uid}</p>
    </div>
  );
}