import { apiFetch } from "./client";

export type Profile = {
  id: string;
  full_name: string | null;
  phone_number: string | null;
};

export async function getMyProfile(): Promise<Profile> {
  return apiFetch<Profile>("/api/profiles/me", { auth: true });
}

export async function updateMyProfile(input: {
  fullName?: string;
  phoneNumber?: string;
}) {
  return apiFetch("/api/profiles/me", {
    method: "PUT",
    auth: true,
    body: JSON.stringify({
      fullName: input.fullName,
      phoneNumber: input.phoneNumber,
    }),
  });
}
