import Link from "next/link";
import { AppHeader } from "@/components/AppHeader";
import { EvidenceAuditTrailPanel } from "@/components/EvidenceAuditTrailPanel";

type AuditTrailPageProps = {
  params: Promise<{
    workspaceId: string;
  }>;
};

export default async function AuditTrailPage({ params }: AuditTrailPageProps) {
  const { workspaceId } = await params;

  return (
    <main className="min-h-screen bg-slate-950 text-white">
      <AppHeader />

      <div className="mx-auto max-w-6xl px-6 py-10">
        <Link
          href={`/claim-workspaces/${workspaceId}`}
          className="text-sm font-semibold text-cyan-300 hover:text-cyan-200"
        >
          ← Back to workspace
        </Link>

        <div className="mt-8">
          <EvidenceAuditTrailPanel workspaceId={workspaceId} />
        </div>
      </div>
    </main>
  );
}