import { useEffect, useState } from "react";
import { Card } from "@/components/ui/card";
import { Star, Quote } from "lucide-react";
import { useI18n } from "@/lib/i18n";
import { listReviews } from "@/lib/api/reviews";

type Review = { id: string; rating: number; comment: string | null; created_at: string; user_id: string };

export function Reviews() {
  const { t } = useI18n();
  const [reviews, setReviews] = useState<Review[]>([]);
  const [idx, setIdx] = useState(0);

  useEffect(() => {
    listReviews()
      .then((data) =>
        setReviews(
          data.filter((r) => r.rating >= 5).sort((a, b) => b.created_at.localeCompare(a.created_at)).slice(0, 12),
        ),
      )
      .catch(() => setReviews([]));
  }, []);

  useEffect(() => {
    if (reviews.length < 2) return;
    const i = setInterval(() => setIdx((p) => (p + 1) % reviews.length), 5000);
    return () => clearInterval(i);
  }, [reviews.length]);

  return (
    <section id="reviews" className="py-20 sm:py-32 relative bg-secondary/30">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center max-w-2xl mx-auto mb-12">
          <h2 className="text-4xl sm:text-5xl font-bold tracking-tight">{t("reviews.title")}</h2>
          <p className="mt-4 text-lg text-muted-foreground">{t("reviews.subtitle")}</p>
        </div>

        <div className="max-w-3xl mx-auto">
          {reviews.length === 0 ? (
            <Card className="p-12 text-center glass">
              <Quote className="h-8 w-8 mx-auto text-muted-foreground/40 mb-4" />
              <p className="text-muted-foreground">{t("reviews.empty")}</p>
            </Card>
          ) : (
            <div className="relative">
              <Card className="p-8 sm:p-12 glass shadow-soft min-h-[240px] flex flex-col justify-center">
                <Quote className="h-10 w-10 text-primary/30 mb-4" />
                <div className="flex gap-1 mb-4">
                  {Array.from({ length: reviews[idx].rating }).map((_, i) => (
                    <Star key={i} className="h-5 w-5 fill-warning text-warning" />
                  ))}
                </div>
                <p className="text-lg sm:text-xl font-medium leading-relaxed">
                  {reviews[idx].comment || "Outstanding service."}
                </p>
                <p className="mt-6 text-sm text-muted-foreground">
                  — Verified customer · {new Date(reviews[idx].created_at).toLocaleDateString()}
                </p>
              </Card>
              <div className="flex justify-center gap-2 mt-6">
                {reviews.map((_, i) => (
                  <button
                    key={i}
                    onClick={() => setIdx(i)}
                    className={`h-1.5 rounded-full transition-all ${i === idx ? "w-8 bg-primary" : "w-1.5 bg-border"}`}
                    aria-label={`Review ${i + 1}`}
                  />
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </section>
  );
}
