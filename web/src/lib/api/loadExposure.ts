import {
  apiGet,
  apiPatch,
  apiPost,
  getApiBaseUrl,
  getAuthHeaders,
  handleApiError,
} from "@/lib/api/client";

export const loadExposureRecordTypes = [
  "LIFTING_CARRYING",
  "STAIRS_LADDERS_RUNGS",
  "KNEELING_SQUATTING",
  "NECK_SHOULDER_CARRIAGE",
  "HEAVY_LOAD_CARRYING",
  "HAZARD_EXPOSURE",
] as const;

export type LoadExposureRecordType = (typeof loadExposureRecordTypes)[number];

export const loadExposureRecordTypeLabels: Record<LoadExposureRecordType, string> = {
  LIFTING_CARRYING: "Lifting and carrying",
  STAIRS_LADDERS_RUNGS: "Stairs, ladders and rungs",
  KNEELING_SQUATTING: "Kneeling and squatting",
  NECK_SHOULDER_CARRIAGE: "Neck and shoulder load carriage",
  HEAVY_LOAD_CARRYING: "Heavy load carrying",
  HAZARD_EXPOSURE: "Hazard exposure",
};

export const loadExposureBodyAreas = [
  "LUMBAR_SPINE",
  "CERVICAL_SPINE",
  "KNEES",
  "SHOULDERS",
  "HIPS",
  "MULTIPLE",
  "OTHER",
] as const;

export const loadExposureFrequencies = [
  "DAILY",
  "MOST_DAYS",
  "WEEKLY",
  "MONTHLY",
  "OCCASIONAL",
  "UNSURE",
] as const;

export const loadExposureHazardTypes = [
  "WHOLE_BODY_VIBRATION",
  "AWKWARD_SUSTAINED_POSTURE",
  "SUDDEN_UNEXPECTED_LOAD",
  "SLIP_TRIP_FALL",
  "CONFINED_SPACE",
  "REPETITIVE_STRAIN",
  "OTHER",
] as const;

export type LoadExposureRecord = {
  id: string;
  claimWorkspaceId: string;
  conditionId?: string | null;
  recordType: LoadExposureRecordType;
  activityDescription: string;
  bodyAreaAffected: string;
  typicalWeightKg?: number | null;
  maxWeightKg?: number | null;
  frequency: string;
  durationPerOccasion?: string | null;
  repetitionsDescription?: string | null;
  servicePeriodFrom?: string | null;
  servicePeriodTo?: string | null;
  yearsExposed?: number | null;
  equipmentOrContext?: string | null;
  hazardType?: string | null;
  notes?: string | null;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type CreateLoadExposureRecordInput = {
  conditionId?: string;
  recordType: LoadExposureRecordType;
  activityDescription: string;
  bodyAreaAffected?: string;
  typicalWeightKg?: number;
  maxWeightKg?: number;
  frequency?: string;
  durationPerOccasion?: string;
  repetitionsDescription?: string;
  servicePeriodFrom?: string;
  servicePeriodTo?: string;
  yearsExposed?: number;
  equipmentOrContext?: string;
  hazardType?: string;
  notes?: string;
};

export type UpdateLoadExposureRecordInput = Partial<CreateLoadExposureRecordInput>;

export function getLoadExposureRecords(
  idToken: string,
  workspaceId: string,
  conditionId?: string,
) {
  const query = conditionId ? `?conditionId=${conditionId}` : "";

  return apiGet<LoadExposureRecord[]>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/load-exposure-records${query}`,
    "Could not load load exposure records.",
  );
}

export function createLoadExposureRecord(
  idToken: string,
  workspaceId: string,
  input: CreateLoadExposureRecordInput,
) {
  return apiPost<LoadExposureRecord, CreateLoadExposureRecordInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/load-exposure-records`,
    input,
    "Could not add load exposure record.",
  );
}

export function updateLoadExposureRecord(
  idToken: string,
  workspaceId: string,
  recordId: string,
  input: UpdateLoadExposureRecordInput,
) {
  return apiPatch<LoadExposureRecord, UpdateLoadExposureRecordInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/load-exposure-records/${recordId}`,
    input,
    "Could not update load exposure record.",
  );
}

export async function archiveLoadExposureRecord(
  idToken: string,
  workspaceId: string,
  recordId: string,
) {
  const response = await fetch(
    `${getApiBaseUrl()}/api/v1/claim-workspaces/${workspaceId}/load-exposure-records/${recordId}`,
    { method: "DELETE", headers: getAuthHeaders(idToken) },
  );

  if (!response.ok) {
    await handleApiError(response, "Could not remove load exposure record.");
  }

  return (await response.json()) as { id: string; status: string; archived: boolean };
}
