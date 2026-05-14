import { EvidenceGap } from "@/lib/api";
import {
  getEvidenceGapStatusLabel,
  getEvidenceGapTypeLabel,
} from "@/lib/evidenceGapUi";

type EvidenceGapReminderPanelProps = {
  workspaceGaps: EvidenceGap[];
  conditionGaps: EvidenceGap[];
  conditionName?: string;
};

export function EvidenceGapReminderPanel({
  workspaceGaps,
  conditionGaps,
  conditionName,
}: EvidenceGapReminderPanelProps) {
  const activeWorkspaceGaps = workspaceGaps.filter((gap) => gap.status !== "ARCHIVED");
  const activeConditionGaps = conditionGaps.filter((gap) => gap.status !== "ARCHIVED");

  const openConditionGaps = activeConditionGaps.filter((gap) => gap.gapStatus === "OPEN");
  const highPriorityGaps = activeConditionGaps.filter(
    (gap) => gap.severity === "HIGH" && gap.gapStatus !== "RESOLVED",
  );
  const inProgressGaps = activeConditionGaps.filter((gap) => gap.gapStatus === "IN_PROGRESS");
  const resolvedGaps = activeConditionGaps.filter((gap) => gap.gapStatus === "RESOLVED");

  const reminders = buildReminderPrompts({
    conditionName,
    openConditionGaps,
    highPriorityGaps,
    inProgressGaps,
    resolvedGaps,
  });

  return (
    <section className="rounded-2xl border border-white/10 bg-white/5 p-6">
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
        Gap dashboard
      </p>

      <h2 className="mt-4 text-2xl font-bold text-white">
        Reminder prompts for {conditionName || "the selected condition"}
      </h2>

      <p className="mt-3 text-sm leading-6 text-slate-300">
        Use these prompts to decide what to review next. They are preparation reminders only
        and do not mean evidence is required, sufficient, accepted or rejected by DVA.
      </p>

      <div className="mt-6 grid gap-4 md:grid-cols-4">
        <DashboardCard label="Workspace gaps" value={activeWorkspaceGaps.length} />
        <DashboardCard label="Open condition gaps" value={openConditionGaps.length} />
        <DashboardCard label="High priority" value={highPriorityGaps.length} />
        <DashboardCard label="In progress" value={inProgressGaps.length} />
      </div>

      <div className="mt-6 rounded-xl border border-white/10 bg-slate-950 p-5">
        <p className="text-sm font-semibold text-white">Suggested next reminders</p>

        <div className="mt-4 grid gap-3">
          {reminders.map((reminder) => (
            <div
              key={reminder.title}
              className="rounded-xl border border-white/10 bg-slate-900 p-4"
            >
              <p className="text-sm font-semibold text-cyan-100">{reminder.title}</p>
              <p className="mt-2 text-sm leading-6 text-slate-300">{reminder.body}</p>
            </div>
          ))}
        </div>
      </div>

      {highPriorityGaps.length > 0 && (
        <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-5">
          <p className="text-sm font-semibold text-yellow-100">High-priority gap focus</p>

          <ul className="mt-3 list-inside list-disc space-y-1 text-sm text-yellow-100">
            {highPriorityGaps.slice(0, 5).map((gap) => (
              <li key={gap.id}>
                {getEvidenceGapTypeLabel(gap.gapType)} — {getEvidenceGapStatusLabel(gap.gapStatus)}
              </li>
            ))}
          </ul>
        </div>
      )}
    </section>
  );
}

function DashboardCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl border border-white/10 bg-slate-900 p-4">
      <p className="text-xs text-slate-400">{label}</p>
      <p className="mt-2 text-2xl font-bold text-white">{value}</p>
    </div>
  );
}

function buildReminderPrompts({
  conditionName,
  openConditionGaps,
  highPriorityGaps,
  inProgressGaps,
  resolvedGaps,
}: {
  conditionName?: string;
  openConditionGaps: EvidenceGap[];
  highPriorityGaps: EvidenceGap[];
  inProgressGaps: EvidenceGap[];
  resolvedGaps: EvidenceGap[];
}) {
  const name = conditionName || "this condition";
  const reminders: Array<{ title: string; body: string }> = [];

  if (openConditionGaps.length === 0 && inProgressGaps.length === 0) {
    reminders.push({
      title: "No open reminder prompts",
      body: `There are no open evidence gaps showing for ${name}. Review the listed evidence and recalculate gaps again after adding new information.`,
    });
  }

  if (highPriorityGaps.length > 0) {
    reminders.push({
      title: "Start with high-priority gaps",
      body: `${name} has ${highPriorityGaps.length} high-priority gap(s). Review these first and decide whether to upload evidence, list evidence, mark the gap in progress, resolve it, or mark it not applicable.`,
    });
  }

  if (inProgressGaps.length > 0) {
    reminders.push({
      title: "Follow up in-progress gaps",
      body: `${inProgressGaps.length} gap(s) are marked in progress. Check whether each one now has enough notes or uploaded/listed evidence for your preparation workflow.`,
    });
  }

  if (resolvedGaps.length > 0) {
    reminders.push({
      title: "Review resolved gaps after changes",
      body: `${resolvedGaps.length} gap(s) are marked resolved. If you add new evidence or change condition details, recalculate gaps and confirm the resolved status still makes sense.`,
    });
  }

  if (openConditionGaps.some((gap) => gap.gapType.includes("DIAGNOSIS"))) {
    reminders.push({
      title: "Diagnosis evidence reminder",
      body: "Consider whether a GP report, specialist report or other clinical evidence has been listed or uploaded for the diagnosis or current clinical picture.",
    });
  }

  if (openConditionGaps.some((gap) => gap.gapType.includes("TREATMENT"))) {
    reminders.push({
      title: "Treatment evidence reminder",
      body: "Consider whether treatment summaries, GP notes, specialist letters or medication information should be listed or uploaded.",
    });
  }

  if (openConditionGaps.some((gap) => gap.gapType.includes("WORSENING"))) {
    reminders.push({
      title: "Worsening evidence reminder",
      body: "Consider whether there is evidence explaining what changed, when it changed, current severity, treatment changes and functional impact.",
    });
  }

  return reminders.slice(0, 6);
}