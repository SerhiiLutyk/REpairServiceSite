import { apiFetch } from "./client";

export type Analytics = {
  totalBookings: number;
  revenue: number;
  activeBookings: number;
  last7Days: { day: string; date: string; bookings: number }[];
};

export async function getAnalytics(): Promise<Analytics> {
  return apiFetch<Analytics>("/api/admin/analytics", { auth: true });
}
