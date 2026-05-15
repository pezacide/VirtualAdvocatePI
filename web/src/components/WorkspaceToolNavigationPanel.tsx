import Link from "next/link";

type WorkspaceToolNavigationPanelProps = {
  workspaceId: string;
};

type WorkspaceTool = {
  title: string;
  hrefSuffix: string;
  description: string;
  badge?: string;
};

const workspaceTools: WorkspaceTool[] = [
  {
    title: "Conditions",
    hrefSuffix: "conditions",
    description: "Add, review and manage conditions in this workspace.",
    badge: "Core",
  },
  {
    title: "Accepted history",
    hrefSuffix: "accepted-history",
    description: "Record accepted-condition and prior decision history.",
    badge: "History",
  },
  {
    title: "Guided questions",
    hrefSuffix: "guided-questions",
    description: "Answer guided preparation questions for the claim workspace.",
    badge: "Prep",
  },
  {
    title: "GARP M questions",
    hrefSuffix: "garp-m-questions",
    description: "Capture GARP M-aware functional impact and preparation answers.",
    badge: "GARP M",
  },
  {
    title: "GARP M summary",
    hrefSuffix: "garp-m-summary",
    description: "Review GARP M-aware summaries and preparation notes.",
    badge: "Summary",
  },
  {
    title: "Evidence metadata",
    hrefSuffix: "evidence-metadata",
    description: "List evidence, document details, providers and dates.",
    badge: "Evidence",
  },
  {
    title: "Evidence upload",
    hrefSuffix: "evidence-upload",
    description: "Upload, open and remove evidence files.",
    badge: "Files",
  },
  {
    title: "Evidence gaps",
    hrefSuffix: "evidence-gaps",
    description: "Review possible missing evidence and follow-up items.",
    badge: "Gaps",
  },
  {
    title: "Evidence checklist",
    hrefSuffix: "evidence-checklist",
    description: "Use the claim evidence checklist to organise next steps.",
    badge: "Checklist",
  },
  {
    title: "AI drafts",
    hrefSuffix: "ai-drafts",
    description: "Prepare AI-assisted draft material from saved workspace information.",
    badge: "Drafting",
  },
  {
    title: "Doctor guidance",
    hrefSuffix: "doctor-guidance",
    description: "Prepare doctor questions and appointment guidance.",
    badge: "Drafting",
  },
  {
    title: "Generated documents",
    hrefSuffix: "generated-documents",
    description: "Generate and download reviewed Claim Starter Pack documents.",
    badge: "Export",
  },
  {
    title: "Audit trail",
    hrefSuffix: "audit-trail",
    description: "Review workspace activity, safety events and change history.",
    badge: "Audit",
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
        Claim workspace navigation
      </h2>

      <p className="mt-3 max-w-4xl text-sm leading-6 text-slate-300">
        Open each workspace tool to manage conditions, evidence, questions,
        summaries, drafts and generated documents.
      </p>

      <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm font-semibold leading-6 text-yellow-100">
        Preparation support only. These tools do not submit material to DVA,
        calculate impairment points, estimate compensation, provide legal advice,
        provide medical advice, make a DVA decision, or guarantee a claim outcome.
      </div>

      <div className="mt-8 grid gap-5 md:grid-cols-2 xl:grid-cols-3">
        {workspaceTools.map((tool) => (
          <Link
            key={tool.hrefSuffix}
            href={`/claim-workspaces/${workspaceId}/${tool.hrefSuffix}`}
            className="group rounded-2xl border border-white/10 bg-slate-900 p-6 transition hover:border-cyan-300/70 hover:bg-cyan-300/10"
          >
            <div className="flex items-start justify-between gap-4">
              <h3 className="text-lg font-bold text-white">{tool.title}</h3>

              {tool.badge && (
                <span className="rounded-full border border-cyan-300/40 bg-cyan-300/10 px-3 py-1 text-xs font-semibold text-cyan-100">
                  {tool.badge}
                </span>
              )}
            </div>

            <p className="mt-4 text-sm leading-6 text-slate-300">
              {tool.description}
            </p>

            <p className="mt-5 text-sm font-semibold text-cyan-300 group-hover:text-cyan-100">
              Open →
            </p>
          </Link>
        ))}
      </div>
    </section>
  );
}