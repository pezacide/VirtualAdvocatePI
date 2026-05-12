import Link from "next/link";

export default function Home() {
  return (
    <main className="min-h-screen bg-slate-950 text-white">
      <section className="mx-auto flex min-h-screen max-w-5xl flex-col justify-center px-6 py-16">
        <p className="mb-4 text-sm font-semibold uppercase tracking-[0.3em] text-cyan-300">
          Virtual Advocate PI
        </p>

        <h1 className="max-w-3xl text-4xl font-bold tracking-tight sm:text-6xl">
          Post-2026 PI claim preparation support for veterans.
        </h1>

        <p className="mt-6 max-w-2xl text-lg leading-8 text-slate-300">
          Build an evidence-ready claim workspace, organise conditions, track evidence gaps,
          and prepare draft documents for review by a doctor, advocate, lawyer or support person.
        </p>

        <div className="mt-10 flex flex-wrap gap-4">
          <Link
            href="/login"
            className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 shadow-lg shadow-cyan-950/40 hover:bg-cyan-200"
          >
            Start login flow
          </Link>

          <Link
            href="/dashboard"
            className="rounded-xl border border-white/20 px-5 py-3 text-sm font-semibold text-white hover:bg-white/10"
          >
            View dashboard shell
          </Link>
        </div>

        <p className="mt-10 max-w-3xl text-sm leading-6 text-slate-400">
          Preparation support only. This app does not provide legal advice, medical advice,
          financial advice, a DVA decision, a compensation estimate, or a guarantee of claim success.
        </p>
      </section>
    </main>
  );
}
