import { apiFetch, clearToken, setToken } from "./client";

export type AuthUser = {
  id: string;
  email: string;
  isAdmin: boolean;
  fullName?: string | null;
  phoneNumber?: string | null;
};

type AuthResponse = {
  accessToken: string;
  expiresAt: string;
  user: AuthUser;
};

export async function login(email: string, password: string): Promise<AuthUser> {
  const data = await apiFetch<AuthResponse>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password }),
  });
  setToken(data.accessToken);
  return data.user;
}

export async function register(input: {
  email: string;
  password: string;
  fullName: string;
  phoneNumber: string;
}): Promise<AuthUser> {
  const data = await apiFetch<AuthResponse>("/api/auth/register", {
    method: "POST",
    body: JSON.stringify({
      email: input.email,
      password: input.password,
      fullName: input.fullName,
      phoneNumber: input.phoneNumber,
    }),
  });
  setToken(data.accessToken);
  return data.user;
}

export async function fetchMe(): Promise<AuthUser | null> {
  try {
    return await apiFetch<AuthUser>("/api/auth/me", { auth: true });
  } catch {
    clearToken();
    return null;
  }
}

export function logout() {
  clearToken();
}
