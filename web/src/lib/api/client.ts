import { env } from "@/lib/env";

export function getApiBaseUrl() {
  if (!env.apiBaseUrl) {
    throw new Error("NEXT_PUBLIC_API_BASE_URL is not configured.");
  }

  return env.apiBaseUrl;
}

export function getAuthHeaders(idToken: string) {
  return {
    Authorization: `Bearer ${idToken}`,
    Accept: "application/json",
  };
}

export function getJsonAuthHeaders(idToken: string) {
  return {
    ...getAuthHeaders(idToken),
    "Content-Type": "application/json",
  };
}

export async function handleApiError(response: Response, defaultMessage: string) {
  if (response.status === 401) {
    throw new Error("You are not signed in or your session has expired.");
  }

  if (response.status === 403) {
    throw new Error("You do not have permission to access this resource.");
  }

  if (response.status === 404) {
    throw new Error("The requested resource was not found.");
  }

  const errorText = await response.text();

  throw new Error(`${defaultMessage} HTTP ${response.status}. ${errorText}`);
}

export async function apiGet<TResponse>(
  idToken: string,
  path: string,
  errorMessage: string,
) {
  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    method: "GET",
    headers: getAuthHeaders(idToken),
    cache: "no-store",
  });

  if (!response.ok) {
    await handleApiError(response, errorMessage);
  }

  return (await response.json()) as TResponse;
}

export async function apiPost<TResponse, TInput>(
  idToken: string,
  path: string,
  input: TInput,
  errorMessage: string,
) {
  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    method: "POST",
    headers: getJsonAuthHeaders(idToken),
    body: JSON.stringify(input),
  });

  if (!response.ok) {
    await handleApiError(response, errorMessage);
  }

  return (await response.json()) as TResponse;
}

export async function apiPostNoBody<TResponse>(
  idToken: string,
  path: string,
  errorMessage: string,
) {
  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    method: "POST",
    headers: getAuthHeaders(idToken),
  });

  if (!response.ok) {
    await handleApiError(response, errorMessage);
  }

  return (await response.json()) as TResponse;
}

export async function apiPatch<TResponse, TInput>(
  idToken: string,
  path: string,
  input: TInput,
  errorMessage: string,
) {
  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    method: "PATCH",
    headers: getJsonAuthHeaders(idToken),
    body: JSON.stringify(input),
  });

  if (!response.ok) {
    await handleApiError(response, errorMessage);
  }

  return (await response.json()) as TResponse;
}