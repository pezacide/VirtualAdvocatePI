import { apiGet, apiPatch, apiPost } from "@/lib/api/client";

export type AdminTemplateRegistryEntry = {
  id: string;
  templateKey: string;
  templateType: "QUESTION" | "DOCUMENT";
  title: string;
  description: string;
  category: string;
  templateVersion: string;
  templateBody: string;
  outputFormat: string;
  approvalStatus: string;
  approvedBy?: string | null;
  reviewNotes?: string | null;
  isActive: boolean;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type AdminTemplateFilters = {
  search?: string;
  templateType?: string;
  category?: string;
  approvalStatus?: string;
  status?: string;
};

export type CreateAdminTemplateInput = {
  templateKey: string;
  templateType: "QUESTION" | "DOCUMENT";
  title: string;
  description?: string;
  category?: string;
  templateVersion?: string;
  templateBody?: string;
  outputFormat?: string;
  approvalStatus?: string;
  approvedBy?: string;
  reviewNotes?: string;
  isActive?: boolean;
  status?: string;
};

export type UpdateAdminTemplateInput = {
  title?: string;
  description?: string;
  category?: string;
  templateVersion?: string;
  templateBody?: string;
  outputFormat?: string;
  approvalStatus?: string;
  approvedBy?: string;
  reviewNotes?: string;
  isActive?: boolean;
  status?: string;
};

export function getAdminTemplates(
  idToken: string,
  filters: AdminTemplateFilters = {},
) {
  const params = new URLSearchParams();

  Object.entries(filters).forEach(([key, value]) => {
    if (value) {
      params.set(key, value);
    }
  });

  const query = params.toString();

  return apiGet<AdminTemplateRegistryEntry[]>(
    idToken,
    `/api/v1/admin/templates${query ? `?${query}` : ""}`,
    "Could not load admin templates.",
  );
}

export function createAdminTemplate(
  idToken: string,
  input: CreateAdminTemplateInput,
) {
  return apiPost<AdminTemplateRegistryEntry, CreateAdminTemplateInput>(
    idToken,
    "/api/v1/admin/templates",
    input,
    "Could not create admin template.",
  );
}

export function updateAdminTemplate(
  idToken: string,
  id: string,
  input: UpdateAdminTemplateInput,
) {
  return apiPatch<AdminTemplateRegistryEntry, UpdateAdminTemplateInput>(
    idToken,
    `/api/v1/admin/templates/${id}`,
    input,
    "Could not update admin template.",
  );
}