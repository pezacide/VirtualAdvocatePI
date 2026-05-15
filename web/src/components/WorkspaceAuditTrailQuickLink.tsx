"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

export function WorkspaceAuditTrailQuickLink() {
  const pathname = usePathname();

  const match = pathname.match(/^\/claim-workspaces\/([^/]+)/);

  if (!match) {
    return null;
  }

  const workspaceId = match[1];

  if (!workspaceId) {
    return null;
  }

  const isAuditTrailPage = pathname.includes("/audit-trail");

  if (isAuditTrailPage) {
    return null;
  }

  return (
    <Link
      href={`/claim-workspaces/${workspaceId}/audit-trail`}
      className="fixed right-4 top-4 z-50 rounded-full border border-cyan-300/50 bg-slate-950/95 px-4 py-2 text-sm font-semibold text-cyan-100 shadow-lg shadow-cyan-950/30 backdrop-blur transition hover:bg-cyan-300 hover:text-slate-950"
    >
      Audit trail →
    </Link>
  );
}