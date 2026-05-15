import Link from "next/link";
import { AdminDatabaseMaintenancePanel } from "@/components/AdminDatabaseMaintenancePanel";

export default function AdminDatabaseMaintenancePage() {
  return (
    <main className="min-h-screen bg-slate-950 px-6 py-10 text-white">
      <div className="mx-auto max-w-5xl space-y-8">
        <Link href="/admin" className="text-sm text-cyan-300 hover:text-cyan-200">
          ← Back to admin dashboard
        </Link>

        <AdminDatabaseMaintenancePanel />
      </div>
    </main>
  );
}