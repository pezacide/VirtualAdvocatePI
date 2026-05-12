import Link from "next/link";

export default function LoginPage() {
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

          <h1 className="mt-4 text-3xl font-bold">Login and session shell</h1>

          <p className="mt-4 text-slate-300">
            Firebase Authentication will be connected in the next Phase 4 task.
            This page is currently a route and layout placeholder.
          </p>

          <div className="mt-8 rounded-xl bg-slate-900 p-5 text-sm text-slate-300">
            Next step: connect Firebase web authentication, then send the Firebase ID token
            to the backend API.
          </div>
        </section>
      </div>
    </main>
  );
}
