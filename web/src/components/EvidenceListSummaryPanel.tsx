import { EvidenceItem } from "@/lib/api";
import { getEvidenceStatusLabel } from "@/lib/evidenceUi";

type EvidenceListSummaryPanelProps = {
  evidenceItems: EvidenceItem[];
  conditionName?: string;
};

export function EvidenceListSummaryPanel({
  evidenceItems,
  conditionName,
}: EvidenceListSummaryPanelProps) {
  const totalCount = evidenceItems.length;
  const uploadedCount = evidenceItems.filter((item) => Boolean(item.uploadedAt)).length;
  const notUploadedCount = evidenceItems.filter((item) => !item.uploadedAt).length;
  const reviewedCount = evidenceItems.filter((item) =>
    ["REVIEWED", "CONFIRMED"].includes(item.evidenceStatus),
  ).length;
  const missingOrNotApplicableCount = evidenceItems.filter((item) =>
    ["MISSING", "NOT_APPLICABLE"].includes(item.evidenceStatus),
  ).length;

  const statusCounts = evidenceItems.reduce<Record<string, number>>((counts, item) => {
    counts[item.evidenceStatus] = (counts[item.evidenceStatus] ?? 0) + 1;
    return counts;
  }, {});

  return (
    <section className="rounded-2xl border border-white/10 bg-white/5 p-6">
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
        Evidence list summary
      </p>

      <h2 className="mt-4 text-2xl font-bold text-white">
        {conditionName ? `Evidence for ${conditionName}` : "Evidence for selected condition"}
      </h2>

      <p className="mt-3 text-sm leading-6 text-slate-300">
        This snapshot helps check what has been listed, uploaded, reviewed or marked as missing.
        It is a preparation view only and does not mean DVA has reviewed or accepted any evidence.
      </p>

      <div className="mt-6 grid gap-4 md:grid-cols-5">
        <SummaryCard label="Total items" value={totalCount} />
        <SummaryCard label="Uploaded" value={uploadedCount} />
        <SummaryCard label="Not uploaded" value={notUploadedCount} />
        <SummaryCard label="Reviewed / confirmed" value={reviewedCount} />
        <SummaryCard label="Missing / not applicable" value={missingOrNotApplicableCount} />
      </div>

      {totalCount > 0 && (
        <div className="mt-6 rounded-xl border border-white/10 bg-slate-950 p-4">
          <p className="text-sm font-semibold text-white">Status breakdown</p>

          <div className="mt-3 flex flex-wrap gap-2">
            {Object.entries(statusCounts).map(([status, count]) => (
              <span
                key={status}
                className="rounded-full border border-white/10 bg-slate-900 px-3 py-1 text-xs text-slate-300"
              >
                {getEvidenceStatusLabel(status)}: {count}
              </span>
            ))}
          </div>
        </div>
      )}
    </section>
  );
}

function SummaryCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl border border-white/10 bg-slate-900 p-4">
      <p className="text-xs text-slate-400">{label}</p>
      <p className="mt-2 text-2xl font-bold text-white">{value}</p>
    </div>
  );
}