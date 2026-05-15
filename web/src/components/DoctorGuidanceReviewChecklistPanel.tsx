export function DoctorGuidanceReviewChecklistPanel() {
  return (
    <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
        Doctor-specific review checklist
      </p>

      <h2 className="mt-4 text-2xl font-bold text-white">
        Review before using this with a doctor
      </h2>

      <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
        This checklist helps keep doctor guidance material respectful, accurate and
        preparation-focused. It is not a medical instruction, legal submission, DVA form
        or request for a guaranteed opinion.
      </p>

      <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm font-semibold leading-6 text-yellow-100">
        Doctor guidance material must not tell a doctor what opinion to provide, must
        not pressure a doctor to support a claim, and must not ask a doctor to make a
        DVA decision.
      </div>

      <div className="mt-6 grid gap-4 md:grid-cols-2">
        <ChecklistCard
          title="Accuracy check"
          items={[
            "The condition name is correct.",
            "Symptoms are described in plain language.",
            "Treatment and medication details are accurate.",
            "Side effects are only included if they are real and relevant.",
            "The material does not exaggerate or add facts that were not provided.",
          ]}
        />

        <ChecklistCard
          title="Doctor respect check"
          items={[
            "The questions are polite and neutral.",
            "The wording does not pressure the doctor.",
            "The wording does not tell the doctor what opinion to give.",
            "The wording asks for clinically appropriate information only.",
            "The wording accepts that the doctor may only record what they can clinically support.",
          ]}
        />

        <ChecklistCard
          title="DVA boundary check"
          items={[
            "The material does not ask the doctor to make a DVA decision.",
            "The material does not claim a DVA outcome is guaranteed.",
            "The material does not calculate impairment points.",
            "The material does not estimate compensation.",
            "The material does not say the evidence is legally or medically sufficient.",
          ]}
        />

        <ChecklistCard
          title="Before appointment check"
          items={[
            "Bring relevant reports or notes if available.",
            "Ask which documents or summaries may be useful to request.",
            "Write down medication names, dose changes and side effects if relevant.",
            "Write down examples of daily impact, flare-ups or worsening if relevant.",
            "Keep a copy of any documents requested or received.",
          ]}
        />
      </div>

      <div className="mt-6 rounded-xl border border-cyan-300/20 bg-cyan-300/5 p-4 text-sm leading-6 text-cyan-50">
        Review note: This doctor guidance workflow is preparation support only. It does
        not provide medical advice, legal advice, diagnosis, DVA decisions, impairment
        calculations, compensation estimates or claim outcome guarantees.
      </div>
    </section>
  );
}

function ChecklistCard({
  title,
  items,
}: {
  title: string;
  items: string[];
}) {
  return (
    <div className="rounded-xl border border-white/10 bg-slate-900 p-5">
      <h3 className="text-lg font-bold text-white">{title}</h3>

      <div className="mt-4 grid gap-3">
        {items.map((item) => (
          <label key={item} className="flex gap-3 text-sm leading-6 text-slate-300">
            <input
              type="checkbox"
              className="mt-1 h-4 w-4 rounded border-white/20 bg-slate-950"
            />
            <span>{item}</span>
          </label>
        ))}
      </div>
    </div>
  );
}