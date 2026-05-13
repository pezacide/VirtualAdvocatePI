import Link from "next/link";

type GarpMWorkspaceLinksProps = {
  workspaceId: string;
};

export function GarpMWorkspaceLinks({ workspaceId }: GarpMWorkspaceLinksProps) {
  return (
    <section className="rounded-2xl border border-cyan-300/30 bg-cyan-300/10 p-8">
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
        Phase 5
      </p>

      <h2 className="mt-4 text-2xl font-bold text-white">
        GARP M-aware preparation tools
      </h2>

      <p className="mt-4 max-w-3xl text-slate-300">
        Use these tools to work through structured condition questions and review a
        plain-English preparation summary. These tools do not calculate GARP M impairment
        points, estimate compensation, provide legal advice, provide medical advice, make a
        DVA decision, or guarantee a claim outcome.
      </p>

      <div className="mt-6 grid gap-4 md:grid-cols-2">
        <Link
          href={`/claim-workspaces/${workspaceId}/garp-m-questions`}
          className="rounded-2xl border border-white/10 bg-slate-900 p-5 hover:border-cyan-300"
        >
          <p className="text-lg font-semibold text-white">
            GARP M-aware question engine
          </p>

          <p className="mt-2 text-sm leading-6 text-slate-400">
            Answer structured questions about diagnosis, symptoms, treatment, stability,
            functional impact, lifestyle impact, work impact, worsening history, evidence gaps
            and appointment preparation.
          </p>

          <p className="mt-4 text-sm font-semibold text-cyan-300">
            Open question engine →
          </p>
        </Link>

        <Link
          href={`/claim-workspaces/${workspaceId}/garp-m-summary`}
          className="rounded-2xl border border-white/10 bg-slate-900 p-5 hover:border-cyan-300"
        >
          <p className="text-lg font-semibold text-white">
            Structured preparation summary
          </p>

          <p className="mt-2 text-sm leading-6 text-slate-400">
            Review saved answers, missing required responses and a copyable plain-English
            summary for discussion with a doctor, advocate, lawyer or support person.
          </p>

          <p className="mt-4 text-sm font-semibold text-cyan-300">
            Open summary →
          </p>
        </Link>
      </div>
    </section>
  );
}