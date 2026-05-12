import Link from "next/link";
import { AppHeader } from "@/components/AppHeader";
import { ClaimWorkspaceDetailPanel } from "@/components/ClaimWorkspaceDetailPanel";

type WorkspacePageProps = {
  params: Promise<{
    workspaceId: string;
  }>;
};

export default async function ClaimWorkspaceDetailPage({ params }: WorkspacePageProps) {
  const { workspaceId } = await params;

  return (
    <main className="min-h-screen bg-slate-950 text-white">
      <AppHeader />

      <div className="mx-auto max-w-6xl px-6 py-12">
        <Link href="/dashboard" className="text-sm text-cyan-300 hover:text-cyan-200">
          ← Back to dashboard
        </Link>

        <div className="mt-10">
          <ClaimWorkspaceDetailPanel workspaceId={workspaceId} />
        </div>
      </div>
    </main>
  );
}