import { DoctorGuidanceQuestionPanel } from "@/components/DoctorGuidanceQuestionPanel";
import { WorkspaceToolNavigationPanel } from "@/components/WorkspaceToolNavigationPanel";

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
        <WorkspaceToolNavigationPanel workspaceId={workspaceId} />

        <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Doctor guidance pack
          </p>

          <h1 className="mt-4 text-3xl font-bold">Clinical question workflow</h1>

          <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">
            Prepare respectful doctor questions, evidence discussion points and doctor
            request letters. This workflow is for appointment preparation only and does
            not tell a doctor what opinion to provide.
          </p>
        </section>

        <DoctorGuidanceQuestionPanel workspaceId={workspaceId} />
      </div>
    </main>
  );
}