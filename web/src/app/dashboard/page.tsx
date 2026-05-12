import Link from "next/link";
import { AppHeader } from "@/components/AppHeader";
import { AuthStatusPanel } from "@/components/AuthStatusPanel";
import { DashboardWorkspaceList } from "@/components/DashboardWorkspaceList";

export default function DashboardPage() {
  return (
    <main className="min-h-screen bg-slate-950 text-white">
      <AppHeader />

      <div className="mx-auto max-w-6xl px-6 py-12">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
              Dashboard
            </p>
            <h1 className="mt-4 text-3xl font-bold">Claim workspaces</h1>
            <p className="mt-3 max-w-2xl text-slate-300">
              View your claim preparation workspaces and continue building the evidence pack.
            </p>
          </div>

          <Link
            href="/claim-workspaces/new"
            className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200"
          >
            New claim workspace
          </Link>
        </div>

        <section className="mt-8">
          <AuthStatusPanel />
        </section>

        <section className="mt-8">
          <DashboardWorkspaceList />
        </section>
      </div>
    </main>
  );
}