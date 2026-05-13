"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import { ClaimCondition, getClaimConditions } from "@/lib/api";

type WorkspaceConditionNamesProps = {
  workspaceId: string;
};

export function WorkspaceConditionNames({ workspaceId }: WorkspaceConditionNamesProps) {
  const { user, loading, getIdToken } = useAuth();
  const [conditions, setConditions] = useState<ClaimCondition[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [hasError, setHasError] = useState(false);

  useEffect(() => {
    async function loadConditions() {
      if (loading || !user) {
        return;
      }

      setIsLoading(true);
      setHasError(false);

      try {
        const token = await getIdToken();

        if (!token) {
          setHasError(true);
          return;
        }

        const rows = await getClaimConditions(token, workspaceId);
        setConditions(rows);
      } catch {
        setHasError(true);
      } finally {
        setIsLoading(false);
      }
    }

    loadConditions();
  }, [getIdToken, loading, user, workspaceId]);

  if (loading || isLoading) {
    return (
      <p className="mt-2 text-sm text-slate-400">
        Conditions: Loading...
      </p>
    );
  }

  if (hasError) {
    return (
      <p className="mt-2 text-sm text-yellow-200">
        Conditions: Could not load
      </p>
    );
  }

  if (conditions.length === 0) {
    return (
      <p className="mt-2 text-sm text-slate-400">
        Conditions: No conditions added yet
      </p>
    );
  }

  const visibleConditionNames = conditions
    .slice(0, 3)
    .map((condition) => condition.conditionName);

  const extraCount = conditions.length - visibleConditionNames.length;

  return (
    <p className="mt-2 text-sm text-cyan-100">
      Conditions: {visibleConditionNames.join(", ")}
      {extraCount > 0 ? ` +${extraCount} more` : ""}
    </p>
  );
}