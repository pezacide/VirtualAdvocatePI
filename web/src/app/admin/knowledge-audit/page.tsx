import Link from "next/link";
import { AdminPlaceholderPanel } from "@/components/AdminPlaceholderPanel";

export default function AdminPlaceholderPage() {
  return (
    <main className="min-h-screen bg-slate-950 px-6 py-10 text-white">
      <div className="mx-auto max-w-6xl space-y-8">
        <Link href="/admin" className="text-sm text-cyan-300 hover:text-cyan-200">
          ← Back to admin dashboard
        </Link>

        <AdminPlaceholderPanel
          title="Knowledge base audit review"
          description="Review knowledge base, source metadata, template and prompt admin changes."
          nextTask="Build knowledge base audit review view"
        />
      </div>
    </main>
  );
}