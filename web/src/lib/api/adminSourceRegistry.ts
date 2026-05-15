import { apiGet, apiPatch } from "@/lib/api/client";

export type AdminSourceRegistryEntry = {
  id: string;
  sourceKey: string;
  title: string;
  category: string;
  sourceType: string;
  jurisdiction: string;
  sourceVersion?: string | null;
  citationLabel: string;
  sourceUrl?: string | null;
  storagePath?: string | null;
  contentHash?: string | null;
  approvalStatus: string;
  approvedBy?: string | null;
  reviewNotes?: string | null;
  isActive: boolean;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type UpdateAdminSourceRegistryEntryInput = {
  title?: string;
  category?: string;
  sourceType?: string;
  jurisdiction?: string;
  sourceVersion?: string;
  citationLabel?: string;
  sourceUrl?: string;
  storagePath?: string;
  contentHash?: string;
  approvalStatus?: string;
  approvedBy?: string;
  reviewNotes?: string;
  isActive?: boolean;
  status?: string;
};

export type AdminSourceRegistryFilters = {
  search?: string;
  category?: string;
  sourceType?: string;
  approvalStatus?: string;
  status?: string;
  isActive?: string;
};

export function getAdminSourceRegistryEntries(
  idToken: string,
  filters: AdminSourceRegistryFilters = {},
) {
  const params = new URLSearchParams();

  Object.entries(filters).forEach(([key, value]) => {
    if (value) {
      params.set(key, value);
    }
  });

  const query = params.toString();

  return apiGet<AdminSourceRegistryEntry[]>(
    idToken,
    `/api/v1/admin/source-registry${query ? `?${query}` : ""}`,
    "Could not load source registry entries.",
  );
}

export function getAdminSourceRegistryEntry(idToken: string, id: string) {
  return apiGet<AdminSourceRegistryEntry>(
    idToken,
    `/api/v1/admin/source-registry/${id}`,
    "Could not load source registry entry.",
  );
}

export function updateAdminSourceRegistryEntry(
  idToken: string,
  id: string,
  input: UpdateAdminSourceRegistryEntryInput,
) {
  return apiPatch<AdminSourceRegistryEntry, UpdateAdminSourceRegistryEntryInput>(
    idToken,
    `/api/v1/admin/source-registry/${id}`,
    input,
    "Could not update source registry entry.",
  );
}