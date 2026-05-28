import { API_URL, TOKEN_KEY } from "./config";

export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
  ) {
    super(message);
  }
}

export function getToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string) {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
}

type ApiFetchOptions = RequestInit & { auth?: boolean };

export async function apiFetch<T>(path: string, options: ApiFetchOptions = {}): Promise<T> {
  const { auth = false, headers, ...rest } = options;
  const reqHeaders = new Headers(headers);

  if (!reqHeaders.has("Content-Type") && rest.body)
    reqHeaders.set("Content-Type", "application/json");

  if (auth) {
    const token = getToken();
    if (token) reqHeaders.set("Authorization", `Bearer ${token}`);
  }

  let res: Response;
  try {
    res = await fetch(`${API_URL}${path}`, { ...rest, headers: reqHeaders });
  } catch {
    throw new ApiError(
      `Cannot reach API at ${API_URL}. Start the backend: dotnet run --project NeoFix.Api`,
      0,
    );
  }

  if (!res.ok) {
    let message = res.statusText;
    try {
      const body = (await res.json()) as {
        message?: string;
        title?: string;
        errors?: Record<string, string[]>;
      };
      if (body.message) message = body.message;
      else if (body.errors) {
        message = Object.entries(body.errors)
          .flatMap(([field, msgs]) => msgs.map((m) => `${field}: ${m}`))
          .join("; ");
      } else if (body.title) message = body.title;
    } catch {
      /* ignore */
    }
    throw new ApiError(message, res.status);
  }

  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}
