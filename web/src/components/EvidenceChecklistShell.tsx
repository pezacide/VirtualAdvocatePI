import Link from "next/link";

type EvidenceChecklistShellProps = {
  workspaceId: string;
};

const evidenceGroups = [
  {
    title: "DVA history and previous decisions",
    description:
      "Useful when a condition has previously been accepted, assessed, compensated, reviewed or worsened.",
    items: [
      "DVA decision letter",
      "Previous PI assessment letter",
      "DCP assessment or review material",
      "Previous compensation or payment correspondence",
      "Any letter showing the original Act, such as MRCA, DRCA or VEA",
    ],
  },
  {
    title: "Medical diagnosis and treatment",
    description:
      "Useful for showing the current clinical picture and what treatment has been tried.",
    items: [
      "GP report or health summary",
      "Specialist report",
      "Diagnosis confirmation",
      "Treatment summary",
      "Medication list",
      "Medication side effect notes",
      "Imaging or test reports if relevant",
    ],
  },
  {
    title: "Functional and lifestyle impact",
    description:
      "Useful for describing how the condition affects ordinary daily activities.",
    items: [
      "Personal statement notes",
      "Sleep impact notes",
      "Mobility or physical restriction notes",
      "Domestic task impact notes",
      "Social or relationship impact notes",
      "Work impact notes",
      "Flare-up or bad-day examples",
    ],
  },
  {
    title: "Service and connection notes",
    description:
      "Useful for organising the background information before speaking with an advocate, doctor, lawyer or support person.",
    items: [
      "Service dates and role details",
      "Deployment, posting or workplace exposure notes",
      "Incident or exposure description",
      "Buddy statement or witness notes if available",
      "Timeline from service event to symptoms",
    ],
  },
  {
    title: "Appointment preparation",
    description:
      "Useful before seeing a GP, specialist, advocate, lawyer or support person.",
    items: [
      "Questions for the doctor",
      "Symptoms to explain clearly",
      "Treatment changes to mention",
      "Evidence gaps to ask about",
      "Documents to request",
      "Follow-up actions after the appointment",
    ],
  },
];

export function EvidenceChecklistShell({ workspaceId }: EvidenceChecklistShellProps) {
  return (
    <div className="space-y-8">
      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Evidence checklist
        </p>

        <h1 className="mt-4 text-3xl font-bold">Evidence preparation checklist</h1>

        <p className="mt-4 max-w-3xl text-slate-300">
          Use this checklist to think through what documents, notes and evidence may be useful
          before speaking with a doctor, advocate, lawyer or support person.
        </p>

        <div className="mt-8 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-5 text-sm leading-6 text-yellow-100">
          This checklist is for preparation only. It does not say what DVA will require,
          does not guarantee claim success, and does not replace advice from a qualified person.
        </div>
      </section>

      <section className="grid gap-5">
        {evidenceGroups.map((group) => (
          <article
            key={group.title}
            className="rounded-2xl border border-white/10 bg-white/5 p-6"
          >
            <h2 className="text-xl font-semibold">{group.title}</h2>

            <p className="mt-3 text-sm leading-6 text-slate-300">
              {group.description}
            </p>

            <div className="mt-5 grid gap-3">
              {group.items.map((item) => (
                <label
                  key={item}
                  className="flex items-start gap-3 rounded-xl border border-white/10 bg-slate-900 p-4 text-sm text-slate-200"
                >
                  <input type="checkbox" className="mt-1" />
                  <span>{item}</span>
                </label>
              ))}
            </div>
          </article>
        ))}
      </section>

      <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
          Next evidence steps
        </p>

        <h2 className="mt-4 text-2xl font-bold">Coming next</h2>

        <p className="mt-4 text-slate-300">
          The next Phase 4 tasks will turn this shell into working evidence features.
        </p>

        <div className="mt-6 grid gap-4 md:grid-cols-3">
          <div className="rounded-xl border border-white/10 bg-slate-900 p-5">
            <h3 className="font-semibold">Evidence metadata</h3>
            <p className="mt-2 text-sm leading-6 text-slate-400">
              List evidence items, status, document date, provider and notes.
            </p>
          </div>

          <div className="rounded-xl border border-white/10 bg-slate-900 p-5">
            <h3 className="font-semibold">Evidence upload</h3>
            <p className="mt-2 text-sm leading-6 text-slate-400">
              Generate upload URLs and attach files to the workspace.
            </p>
          </div>

          <div className="rounded-xl border border-white/10 bg-slate-900 p-5">
            <h3 className="font-semibold">Evidence gaps</h3>
            <p className="mt-2 text-sm leading-6 text-slate-400">
              Review plain-English prompts for missing or incomplete evidence.
            </p>
          </div>
        </div>

        <Link
          href={`/claim-workspaces/${workspaceId}`}
          className="mt-8 inline-flex rounded-xl bg-cyan-300 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-cyan-200"
        >
          Back to workspace
        </Link>
      </section>

      <section className="rounded-2xl border border-white/10 bg-slate-900 p-6 text-sm leading-6 text-slate-400">
        Preparation support only. This evidence checklist does not provide legal advice,
        medical advice, a DVA decision, a compensation estimate, or a guarantee of claim success.
      </section>
    </div>
  );
}