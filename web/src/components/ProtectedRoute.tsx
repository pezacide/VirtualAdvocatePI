"use client";

import Link from "next/link";
import { ReactNode, useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";
import { useAuth } from "@/components/AuthProvider";

type ProtectedRouteProps = {
  children: ReactNode;
};

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const router = useRouter();
  const pathname = usePathname();
  const { user, loading } = useAuth();

  useEffect(() => {
    if (!loading && !user) {
      router.replace(`/login?returnTo=${encodeURIComponent(pathname)}`);
    }
  }, [loading, pathname, router, user]);

  if (loading) {
    return (
      <main className="min-h-screen bg-slate-950 px-6 py-12 text-white">
        <div className="mx-auto max-w-4xl rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Checking session
          </p>

          <h1 className="mt-4 text-3xl font-bold">Checking sign-in status...</h1>

          <p className="mt-4 text-slate-300">
            Please wait while the app checks your Firebase session.
          </p>
        </div>
      </main>
    );
  }

  if (!user) {
    return (
      <main className="min-h-screen bg-slate-950 px-6 py-12 text-white">
        <div className="mx-auto max-w-4xl rounded-2xl border border-yellow-300/30 bg-yellow-300/10 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-yellow-200">
            Sign in required
          </p>

          <h1 className="mt-4 text-3xl font-bold">This page needs a signed-in session</h1>

          <p className="mt-4 text-yellow-100">
            Sign in before opening dashboard, workspace, evidence, draft or document pages.
          </p>

          <Link
            href="/login"
            className="mt-8 inline-flex rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200"
          >
            Go to login
          </Link>
        </div>
      </main>
    );
  }

  return children;
}