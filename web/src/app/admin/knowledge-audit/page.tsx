import Link from "next/link";
import { AdminKnowledgeAuditReviewPanel } from "@/components/AdminKnowledgeAuditReviewPanel";

export default function KnowledgeAuditPage() {
  return (
    <main className="min-h-screen bg-slate-950 px-6 py-10 text-white">
      <div className="mx-auto max-w-7xl space-y-8">
        <Link href="/admin" className="text-sm text-cyan-300 hover:text-cyan-200">
          ← Back to admin dashboard
        </Link>

        <AdminKnowledgeAuditReviewPanel />
      </div>
    </main>
  );
}