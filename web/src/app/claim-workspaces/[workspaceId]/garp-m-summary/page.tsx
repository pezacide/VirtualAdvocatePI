import Link from "next/link";
import { AppHeader } from "@/components/AppHeader";
import { GarpMStructuredSummaryPanel } from "@/components/garpM";

type GarpMSummaryPageProps = {
  params: Promise<{
    workspaceId: string;
  }>;
};

export default async function GarpMSummaryPage({ params }: GarpMSummaryPageProps) {
  const { workspaceId } = await params;

  return (
    <main className="min-h-screen bg-slate-950 text-white">
      <AppHeader />

      <div className="mx-auto max-w-6xl px-6 py-12">
        <Link
          href={`/claim-workspaces/${workspaceId}`}
          className="text-sm text-cyan-300 hover:text-cyan-200"
        >
          ← Back to workspace
        </Link>

        <div className="mt-10">
          <GarpMStructuredSummaryPanel workspaceId={workspaceId} />
        </div>
      </div>
    </main>
  );
}