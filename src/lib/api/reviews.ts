import { apiFetch } from "./client";

export type Review = {
  id: string;
  rating: number;
  comment: string | null;
  created_at: string;
  user_id: string;
};

export async function listReviews(): Promise<Review[]> {
  return apiFetch<Review[]>("/api/reviews");
}

export async function createReview(rating: number, comment: string | null) {
  return apiFetch("/api/reviews", {
    method: "POST",
    auth: true,
    body: JSON.stringify({ rating, comment }),
  });
}
