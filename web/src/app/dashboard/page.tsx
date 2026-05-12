import Link from "next/link";

const sampleWorkspaces = [
  {
    id: "demo-workspace-1",
    title: "Post-2026 PI Claim Starter Pack",
    status: "IN_PROGRESS",
    scenario: "UNSURE",
  },
];

export default function DashboardPage() {
  return (
    <main className="min-h-screen bg-slate-950 px-6 py-12 text-white">
      <div className="mx-auto max-w-6xl">
        <Link href="/" className="text-sm text-cyan-300 hover:text-cyan-200">
          ← Back to home
        </Link>

        <div className="mt-10 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
              Dashboard
            </p>
            <h1 className="mt-4 text-3xl font-bold">Claim workspaces</h1>
            <p className="mt-3 max-w-2xl text-slate-300">
              This shell will list authenticated user claim workspaces from the backend.
            </p>
          </div>

          <Link
            href="/claim-workspaces/new"
            className="rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200"
          >
            New claim workspace
          </Link>
        </div>

        <section className="mt-8 grid gap-4">
          {sampleWorkspaces.map((workspace) => (
            <Link
              key={workspace.id}
              href={`/claim-workspaces/${workspace.id}`}
              className="rounded-2xl border border-white/10 bg-white/5 p-6 hover:bg-white/10"
            >
              <h2 className="text-xl font-semibold">{workspace.title}</h2>
              <p className="mt-2 text-sm text-slate-300">
                Status: {workspace.status} · Scenario: {workspace.scenario}
              </p>
            </Link>
          ))}
        </section>
      </div>
    </main>
  );
}
