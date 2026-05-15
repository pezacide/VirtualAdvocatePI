import { apiGet } from "@/lib/api/client";

export type AdminMeResponse = {
  userId: string;
  firebaseUid: string;
  email: string;
  displayName?: string | null;
  role: string;
  accountStatus: string;
  isAdmin: boolean;
  reason: string;
};

export type AdminPingResponse = {
  ok: boolean;
  message: string;
  userId: string;
  email: string;
  role: string;
};

export function getAdminMe(idToken: string) {
  return apiGet<AdminMeResponse>(
    idToken,
    "/api/v1/admin/me",
    "Could not load admin access status.",
  );
}

export function getAdminPing(idToken: string) {
  return apiGet<AdminPingResponse>(
    idToken,
    "/api/v1/admin/ping",
    "Admin endpoint access was denied.",
  );
}