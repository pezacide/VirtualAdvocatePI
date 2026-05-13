import Link from "next/link";

type WorkspaceToolNavigationPanelProps = {
  workspaceId: string;
};

const workspaceTools = [
  {
    title: "Condition intake",
    description: "Add and review the conditions being prepared in this workspace.",
    hrefSuffix: "conditions",
    status: "Core",
  },
  {
    title: "Accepted-condition history",
    description: "Record previous DVA acceptance, assessment, worsening and compensation history.",
    hrefSuffix: "accepted-history",
    status: "Core",
  },
  {
    title: "Guided questions",
    description: "Answer general guided preparation questions for the selected workspace.",
    hrefSuffix: "guided-questions",
    status: "Core",
  },
  {
    title: "Evidence checklist",
    description: "Review practical evidence items that may support preparation.",
    hrefSuffix: "evidence-checklist",
    status: "Core",
  },
  {
    title: "Evidence metadata",
    description: "Record document details, source, date and evidence notes.",
    hrefSuffix: "evidence-metadata",
    status: "Evidence",
  },
  {
    title: "Evidence upload",
    description: "Upload or prepare evidence files for this workspace.",
    hrefSuffix: "evidence-upload",
    status: "Evidence",
  },
  {
    title: "Evidence gaps",
    description: "Review missing information, missing documents and next evidence actions.",
    hrefSuffix: "evidence-gaps",
    status: "Evidence",
  },
  {
    title: "GARP M-aware questions",
    description: "Work through structured preparation questions about symptoms, treatment, stability, impact, worsening and evidence gaps.",
    hrefSuffix: "garp-m-questions",
    status: "Phase 5",
  },
  {
    title: "GARP M structured summary",
    description: "Review saved answers and copy a plain-English preparation summary.",
    hrefSuffix: "garp-m-summary",
    status: "Phase 5",
  },
  {
    title: "AI drafts",
    description: "Prepare AI-assisted draft material from saved workspace information.",
    hrefSuffix: "ai-drafts",
    status: "Drafting",
  },
  {
    title: "Generated documents",
    description: "Review generated document metadata and prepared document outputs.",
    hrefSuffix: "generated-documents",
    status: "Documents",
  },
];

export function WorkspaceToolNavigationPanel({
  workspaceId,
}: WorkspaceToolNavigationPanelProps) {
  return (
    <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
        Workspace tools
      </p>

      <h2 className="mt-4 text-2xl font-bold text-white">
        Continue building this claim preparation workspace
      </h2>

      <p className="mt-4 max-w-3xl text-slate-300">
        Use these links to move between condition intake, evidence preparation, GARP M-aware
        questions, summaries, drafts and generated documents.
      </p>

      <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-5 text-sm leading-6 text-yellow-100">
        Preparation support only. These tools do not submit material to DVA, calculate
        impairment points, estimate compensation, provide legal advice, provide medical advice,
        make a DVA decision, or guarantee a claim outcome.
      </div>

      <div className="mt-8 grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {workspaceTools.map((tool) => (
          <Link
            key={tool.hrefSuffix}
            href={`/claim-workspaces/${workspaceId}/${tool.hrefSuffix}`}
            className="rounded-2xl border border-white/10 bg-slate-900 p-5 transition hover:border-cyan-300 hover:bg-white/5"
          >
            <div className="flex items-start justify-between gap-4">
              <h3 className="text-lg font-semibold text-white">{tool.title}</h3>

              <span className="rounded-full border border-cyan-300/30 bg-cyan-300/10 px-3 py-1 text-xs text-cyan-100">
                {tool.status}
              </span>
            </div>

            <p className="mt-3 text-sm leading-6 text-slate-400">
              {tool.description}
            </p>

            <p className="mt-4 text-sm font-semibold text-cyan-300">
              Open →
            </p>
          </Link>
        ))}
      </div>
    </section>
  );
}