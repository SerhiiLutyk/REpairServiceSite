import { apiFetch } from "./client";

export type Service = {
  id: string;
  device_type: string;
  service_name: string;
  estimated_price: number;
  estimated_duration_minutes: number;
  created_at?: string;
};

export async function listServices(summary = false): Promise<Service[]> {
  const q = summary ? "?summary=true" : "";
  return apiFetch<Service[]>(`/api/services${q}`);
}

export async function createService(payload: Omit<Service, "id" | "created_at">) {
  return apiFetch(`/api/services`, {
    method: "POST",
    auth: true,
    body: JSON.stringify({
      deviceType: payload.device_type,
      serviceName: payload.service_name,
      estimatedPrice: payload.estimated_price,
      estimatedDurationMinutes: payload.estimated_duration_minutes,
    }),
  });
}

export async function updateService(
  id: string,
  payload: Omit<Service, "id" | "created_at">,
) {
  return apiFetch(`/api/services/${id}`, {
    method: "PUT",
    auth: true,
    body: JSON.stringify({
      deviceType: payload.device_type,
      serviceName: payload.service_name,
      estimatedPrice: payload.estimated_price,
      estimatedDurationMinutes: payload.estimated_duration_minutes,
    }),
  });
}

export async function deleteService(id: string) {
  return apiFetch(`/api/services/${id}`, { method: "DELETE", auth: true });
}
