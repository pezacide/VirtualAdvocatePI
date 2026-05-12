import Link from "next/link";

export default function NewClaimWorkspacePage() {
  return (
    <main className="min-h-screen bg-slate-950 px-6 py-12 text-white">
      <div className="mx-auto max-w-4xl">
        <Link href="/dashboard" className="text-sm text-cyan-300 hover:text-cyan-200">
          ← Back to dashboard
        </Link>

        <section className="mt-10 rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            New workspace
          </p>

          <h1 className="mt-4 text-3xl font-bold">Claim pathway selector shell</h1>

          <p className="mt-4 text-slate-300">
            This page will let the veteran choose a post-2026 PI claim scenario.
          </p>

          <div className="mt-8 grid gap-3">
            {[
              "NEW_CONDITION",
              "WORSENING_EXISTING_CONDITION",
              "NEW_PLUS_EXISTING",
              "EVIDENCE_PREP_ONLY",
              "UNSURE",
            ].map((scenario) => (
              <div key={scenario} className="rounded-xl border border-white/10 bg-slate-900 p-4">
                {scenario}
              </div>
            ))}
          </div>
        </section>
      </div>
    </main>
  );
}
