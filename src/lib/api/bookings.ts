import { apiFetch } from "./client";

export type BookingStatus = "Pending" | "Confirmed" | "In Progress" | "Completed" | "Cancelled";

export type UserBooking = {
  id: string;
  booking_date: string;
  booking_time: string;
  status: BookingStatus;
  master_notes: string | null;
  service: {
    service_name: string;
    device_type: string;
    estimated_price: number;
  } | null;
};

export type AdminBooking = {
  id: string;
  booking_date: string;
  booking_time: string;
  status: BookingStatus;
  master_notes: string | null;
  user_id: string;
  service: { service_name: string; estimated_price: number } | null;
  profile: { id: string; full_name: string | null; phone_number: string | null } | null;
};

export async function getTakenSlots(date: string): Promise<string[]> {
  const rows = await apiFetch<{ booking_time: string }[]>(
    `/api/bookings/slots?date=${encodeURIComponent(date)}`,
  );
  return rows.map((r) => r.booking_time.slice(0, 5));
}

export async function listMyBookings(): Promise<UserBooking[]> {
  return apiFetch<UserBooking[]>("/api/bookings/me", { auth: true });
}

export async function createBooking(input: {
  serviceId: string;
  bookingDate: string;
  bookingTime: string;
}) {
  return apiFetch("/api/bookings", {
    method: "POST",
    auth: true,
    body: JSON.stringify({
      serviceId: input.serviceId,
      bookingDate: input.bookingDate,
      bookingTime: input.bookingTime,
    }),
  });
}

export async function cancelBooking(id: string) {
  return apiFetch(`/api/bookings/${id}/cancel`, { method: "PATCH", auth: true });
}

export async function listAdminBookings(): Promise<AdminBooking[]> {
  return apiFetch<AdminBooking[]>("/api/bookings", { auth: true });
}

export async function updateBookingStatus(id: string, status: BookingStatus) {
  return apiFetch(`/api/bookings/${id}/status`, {
    method: "PATCH",
    auth: true,
    body: JSON.stringify({ status }),
  });
}

export async function updateMasterNotes(id: string, masterNotes: string | null) {
  return apiFetch(`/api/bookings/${id}/notes`, {
    method: "PATCH",
    auth: true,
    body: JSON.stringify({ masterNotes }),
  });
}
