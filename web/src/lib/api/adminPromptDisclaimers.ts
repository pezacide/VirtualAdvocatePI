import { apiGet, apiPatch, apiPost } from "@/lib/api/client";

export type AdminPromptDisclaimerVersionEntry = {
  id: string;
  versionKey: string;
  versionType: "PROMPT" | "DISCLAIMER";
  title: string;
  description: string;
  category: string;
  versionLabel: string;
  appliesTo: string;
  content: string;
  approvalStatus: string;
  approvedBy?: string | null;
  reviewNotes?: string | null;
  effectiveFrom?: string | null;
  retiredAt?: string | null;
  isActive: boolean;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type AdminPromptDisclaimerFilters = {
  search?: string;
  versionType?: string;
  category?: string;
  appliesTo?: string;
  approvalStatus?: string;
  status?: string;
};

export type CreateAdminPromptDisclaimerVersionInput = {
  versionKey: string;
  versionType: "PROMPT" | "DISCLAIMER";
  title: string;
  description?: string;
  category?: string;
  versionLabel?: string;
  appliesTo?: string;
  content?: string;
  approvalStatus?: string;
  approvedBy?: string;
  reviewNotes?: string;
  effectiveFrom?: string | null;
  retiredAt?: string | null;
  isActive?: boolean;
  status?: string;
};

export type UpdateAdminPromptDisclaimerVersionInput = {
  title?: string;
  description?: string;
  category?: string;
  versionLabel?: string;
  appliesTo?: string;
  content?: string;
  approvalStatus?: string;
  approvedBy?: string;
  reviewNotes?: string;
  effectiveFrom?: string | null;
  effectiveFromSet?: boolean;
  retiredAt?: string | null;
  retiredAtSet?: boolean;
  isActive?: boolean;
  status?: string;
};

export function getAdminPromptDisclaimerVersions(
  idToken: string,
  filters: AdminPromptDisclaimerFilters = {},
) {
  const params = new URLSearchParams();

  Object.entries(filters).forEach(([key, value]) => {
    if (value) {
      params.set(key, value);
    }
  });

  const query = params.toString();

  return apiGet<AdminPromptDisclaimerVersionEntry[]>(
    idToken,
    `/api/v1/admin/prompt-disclaimer-versions${query ? `?${query}` : ""}`,
    "Could not load prompt/disclaimer versions.",
  );
}

export function createAdminPromptDisclaimerVersion(
  idToken: string,
  input: CreateAdminPromptDisclaimerVersionInput,
) {
  return apiPost<
    AdminPromptDisclaimerVersionEntry,
    CreateAdminPromptDisclaimerVersionInput
  >(
    idToken,
    "/api/v1/admin/prompt-disclaimer-versions",
    input,
    "Could not create prompt/disclaimer version.",
  );
}

export function updateAdminPromptDisclaimerVersion(
  idToken: string,
  id: string,
  input: UpdateAdminPromptDisclaimerVersionInput,
) {
  return apiPatch<
    AdminPromptDisclaimerVersionEntry,
    UpdateAdminPromptDisclaimerVersionInput
  >(
    idToken,
    `/api/v1/admin/prompt-disclaimer-versions/${id}`,
    input,
    "Could not update prompt/disclaimer version.",
  );
}