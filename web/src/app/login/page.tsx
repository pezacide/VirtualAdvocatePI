"use client";

import Link from "next/link";
import { FormEvent, Suspense, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useAuth } from "@/components/AuthProvider";

export default function LoginPage() {
  return (
    <Suspense fallback={<LoginLoading />}>
      <LoginContent />
    </Suspense>
  );
}

function LoginLoading() {
  return (
    <main className="min-h-screen bg-slate-950 px-6 py-12 text-white">
      <div className="mx-auto max-w-3xl rounded-2xl border border-white/10 bg-white/5 p-8">
        Loading login...
      </div>
    </main>
  );
}

function LoginContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { user, loading, signIn, register, signOutUser } = useAuth();

  const returnTo = searchParams.get("returnTo") || "/dashboard";

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [mode, setMode] = useState<"sign-in" | "register">("sign-in");
  const [statusMessage, setStatusMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setStatusMessage("");
    setIsSubmitting(true);

    try {
      if (mode === "sign-in") {
        await signIn(email, password);
        setStatusMessage("Signed in successfully.");
      } else {
        await register(email, password);
        setStatusMessage("Account created and signed in.");
      }

      router.push(returnTo);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Authentication failed.";
      setStatusMessage(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleSignOut() {
    setStatusMessage("");
    setIsSubmitting(true);

    try {
      await signOutUser();
      setStatusMessage("Signed out.");
    } catch (error) {
      const message = error instanceof Error ? error.message : "Sign out failed.";
      setStatusMessage(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="min-h-screen bg-slate-950 px-6 py-12 text-white">
      <div className="mx-auto max-w-3xl">
        <Link href="/" className="text-sm text-cyan-300 hover:text-cyan-200">
          ← Back to home
        </Link>

        <section className="mt-10 rounded-2xl border border-white/10 bg-white/5 p-8 shadow-2xl">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Login
          </p>

          <h1 className="mt-4 text-3xl font-bold">Firebase authentication</h1>

          <p className="mt-4 text-slate-300">
            Sign in or create a test account for the Virtual Advocate PI web MVP shell.
          </p>

          {returnTo !== "/dashboard" && (
            <div className="mt-6 rounded-xl border border-cyan-300/30 bg-cyan-300/10 p-4 text-sm text-cyan-100">
              After login, you will return to: <span className="font-mono">{returnTo}</span>
            </div>
          )}

          {loading ? (
            <div className="mt-8 rounded-xl bg-slate-900 p-5 text-sm text-slate-300">
              Checking session...
            </div>
          ) : user ? (
            <div className="mt-8 rounded-xl border border-green-300/30 bg-green-300/10 p-5">
              <p className="font-semibold text-green-200">Signed in</p>
              <p className="mt-2 text-sm text-green-100">{user.email}</p>

              <div className="mt-5 flex flex-wrap gap-3">
                <Link
                  href={returnTo}
                  className="rounded-xl bg-cyan-300 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-cyan-200"
                >
                  Continue
                </Link>

                <button
                  type="button"
                  onClick={handleSignOut}
                  disabled={isSubmitting}
                  className="rounded-xl bg-white px-4 py-2 text-sm font-semibold text-slate-950 disabled:opacity-60"
                >
                  Sign out
                </button>
              </div>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="mt-8 space-y-5">
              <div>
                <label htmlFor="email" className="text-sm font-medium text-slate-200">
                  Email
                </label>
                <input
                  id="email"
                  type="email"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  required
                  className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
                />
              </div>

              <div>
                <label htmlFor="password" className="text-sm font-medium text-slate-200">
                  Password
                </label>
                <input
                  id="password"
                  type="password"
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                  required
                  minLength={6}
                  className="mt-2 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
                />
              </div>

              <div className="flex flex-wrap gap-3">
                <button
                  type="button"
                  onClick={() => setMode("sign-in")}
                  className={
                    mode === "sign-in"
                      ? "rounded-xl bg-cyan-300 px-4 py-2 text-sm font-semibold text-slate-950"
                      : "rounded-xl border border-white/20 px-4 py-2 text-sm font-semibold text-white"
                  }
                >
                  Sign in
                </button>

                <button
                  type="button"
                  onClick={() => setMode("register")}
                  className={
                    mode === "register"
                      ? "rounded-xl bg-cyan-300 px-4 py-2 text-sm font-semibold text-slate-950"
                      : "rounded-xl border border-white/20 px-4 py-2 text-sm font-semibold text-white"
                  }
                >
                  Register
                </button>
              </div>

              <button
                type="submit"
                disabled={isSubmitting}
                className="w-full rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:opacity-60"
              >
                {isSubmitting
                  ? "Working..."
                  : mode === "sign-in"
                    ? "Sign in"
                    : "Create account"}
              </button>
            </form>
          )}

          {statusMessage && (
            <div className="mt-6 rounded-xl border border-white/10 bg-slate-900 p-4 text-sm text-slate-200">
              {statusMessage}
            </div>
          )}

          <p className="mt-8 text-sm leading-6 text-slate-400">
            Preparation support only. This login does not create a DVA claim, submit material to DVA,
            provide legal advice, provide medical advice, or guarantee a claim outcome.
          </p>
        </section>
      </div>
    </main>
  );
}