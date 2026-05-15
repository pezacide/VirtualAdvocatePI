import Link from "next/link";
import { AdminAccessCheckPanel } from "@/components/AdminAccessCheckPanel";

export default function AdminAccessCheckPage() {
  return (
    <main className="min-h-screen bg-slate-950 px-6 py-10 text-white">
      <div className="mx-auto max-w-5xl space-y-8">
        <Link href="/dashboard" className="text-sm text-cyan-300 hover:text-cyan-200">
          ← Back to dashboard
        </Link>

        <AdminAccessCheckPanel />
      </div>
    </main>
  );
}