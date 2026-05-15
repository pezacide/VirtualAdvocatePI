import Link from "next/link";
import { DoctorGuidanceQuestionPanel } from "@/components/DoctorGuidanceQuestionPanel";

type DoctorGuidancePageProps = {
  params: Promise<{
    workspaceId: string;
  }>;
};

export default async function DoctorGuidancePage({
  params,
}: DoctorGuidancePageProps) {
  const { workspaceId } = await params;

  return (
    <main className="min-h-screen bg-slate-950 px-6 py-10 text-white">
      <div className="mx-auto max-w-6xl space-y-8">
        <Link
          href={`/claim-workspaces/${workspaceId}`}
          className="inline-flex rounded-xl border border-cyan-300/40 px-4 py-2 text-sm font-semibold text-cyan-100 hover:bg-cyan-300/10"
        >
          ← Back to workspace tools
        </Link>

        <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Doctor guidance pack
          </p>

          <h1 className="mt-4 text-3xl font-bold">
            Clinical question workflow
          </h1>

          <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
            Prepare respectful doctor questions, evidence discussion points and
            doctor request letters. This workflow is for appointment preparation
            only and does not tell a doctor what opinion to provide.
          </p>

          <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm font-semibold leading-6 text-yellow-100">
            Preparation support only. This tool does not provide medical advice,
            legal advice, diagnosis, DVA decisions, impairment points,
            compensation estimates or claim outcome guarantees.
          </div>
        </section>

        <DoctorGuidanceQuestionPanel workspaceId={workspaceId} />
      </div>
    </main>
  );
}