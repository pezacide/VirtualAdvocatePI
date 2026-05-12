import Link from "next/link";

type WorkspacePageProps = {
  params: Promise<{
    workspaceId: string;
  }>;
};

export default async function ClaimWorkspaceDetailPage({ params }: WorkspacePageProps) {
  const { workspaceId } = await params;

  return (
    <main className="min-h-screen bg-slate-950 px-6 py-12 text-white">
      <div className="mx-auto max-w-6xl">
        <Link href="/dashboard" className="text-sm text-cyan-300 hover:text-cyan-200">
          ← Back to dashboard
        </Link>

        <section className="mt-10 rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Claim workspace
          </p>

          <h1 className="mt-4 text-3xl font-bold">Workspace detail shell</h1>

          <p className="mt-4 text-slate-300">
            Workspace ID: <span className="font-mono text-cyan-200">{workspaceId}</span>
          </p>

          <div className="mt-8 grid gap-4 md:grid-cols-2">
            {[
              "Condition intake",
              "Accepted-condition history",
              "Guided questions",
              "Evidence checklist",
              "Evidence gaps",
              "AI drafts",
              "Generated documents",
            ].map((item) => (
              <div key={item} className="rounded-xl border border-white/10 bg-slate-900 p-5">
                <h2 className="font-semibold">{item}</h2>
                <p className="mt-2 text-sm text-slate-400">UI section placeholder.</p>
              </div>
            ))}
          </div>
        </section>
      </div>
    </main>
  );
}