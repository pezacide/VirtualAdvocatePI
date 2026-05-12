import Link from "next/link";
import { AppHeader } from "@/components/AppHeader";
import { AuthStatusPanel } from "@/components/AuthStatusPanel";
import { NewClaimPathwaySelector } from "@/components/NewClaimPathwaySelector";

export default function NewClaimWorkspacePage() {
  return (
    <main className="min-h-screen bg-slate-950 text-white">
      <AppHeader />

      <div className="mx-auto max-w-5xl px-6 py-12">
        <Link href="/dashboard" className="text-sm text-cyan-300 hover:text-cyan-200">
          ← Back to dashboard
        </Link>

        <section className="mt-10 rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            New workspace
          </p>

          <h1 className="mt-4 text-3xl font-bold">Choose a claim preparation pathway</h1>

          <p className="mt-4 max-w-3xl text-slate-300">
            Pick the closest starting point. You can still add more information later as the
            evidence picture becomes clearer.
          </p>

          <div className="mt-8">
            <AuthStatusPanel />
          </div>

          <div className="mt-8">
            <NewClaimPathwaySelector />
          </div>
        </section>
      </div>
    </main>
  );
}