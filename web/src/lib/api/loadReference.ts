import { apiGet } from "@/lib/api/client";

export type LoadReferenceItem = {
  id: string;
  itemName: string;
  category: string;
  typicalWeightKg?: number | null;
  weightRangeKg?: string | null;
  serviceContext?: string | null;
  notes?: string | null;
  sourceLabel: string;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type LoadReferenceItemsResponse = {
  referenceGuidanceOnly: boolean;
  note: string;
  items: LoadReferenceItem[];
};

export function getLoadReferenceItems(idToken: string) {
  return apiGet<LoadReferenceItemsResponse>(
    idToken,
    `/api/v1/reference/load-reference-items`,
    "Could not load the equipment weight reference library.",
  );
}
