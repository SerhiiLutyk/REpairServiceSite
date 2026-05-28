import { useCallback, useEffect, useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import { useAuth } from "@/hooks/use-auth";
import { useI18n } from "@/lib/i18n";
import { ApiError } from "@/lib/api/client";
import { cancelBooking, listMyBookings, type BookingStatus, type UserBooking } from "@/lib/api/bookings";
import { createReview } from "@/lib/api/reviews";
import { toast } from "sonner";
import { Star, Calendar as Cal, Clock } from "lucide-react";

const STATUS_COLOR: Record<BookingStatus, string> = {
  Pending: "bg-warning/15 text-warning-foreground border-warning/30",
  Confirmed: "bg-primary/15 text-primary border-primary/30",
  "In Progress": "bg-accent/40 text-accent-foreground border-accent",
  Completed: "bg-success/15 text-success border-success/30",
  Cancelled: "bg-destructive/15 text-destructive border-destructive/30",
};

export function MyOrders() {
  const { user } = useAuth();
  const { t } = useI18n();
  const [items, setItems] = useState<UserBooking[]>([]);
  const [reviewFor, setReviewFor] = useState<UserBooking | null>(null);

  const load = useCallback(async () => {
    if (!user) return;
    try {
      setItems(await listMyBookings());
    } catch {
      /* ignore */
    }
  }, [user]);

  useEffect(() => {
    load();
    if (!user) return;
    const id = setInterval(load, 15000);
    return () => clearInterval(id);
  }, [user, load]);

  const cancel = async (id: string) => {
    try {
      await cancelBooking(id);
      toast.success("Cancelled");
      load();
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "Cancel failed");
    }
  };

  if (items.length === 0) {
    return (
      <Card className="p-12 text-center glass">
        <p className="text-muted-foreground">{t("dash.empty")}</p>
      </Card>
    );
  }

  return (
    <>
      <div className="space-y-3">
        {items.map((b) => {
          const canCancel = b.status === "Pending" || b.status === "Confirmed";
          const canReview = b.status === "Completed";
          return (
            <Card key={b.id} className="p-5 glass">
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap">
                    <h3 className="font-semibold text-lg">{b.service?.service_name}</h3>
                    <Badge variant="outline" className={STATUS_COLOR[b.status]}>{t(`status.${b.status}` as const)}</Badge>
                  </div>
                  <p className="text-sm text-muted-foreground mt-1">{b.service?.device_type}</p>
                  <div className="flex gap-4 mt-3 text-sm text-muted-foreground">
                    <span className="flex items-center gap-1"><Cal className="h-3.5 w-3.5" />{b.booking_date}</span>
                    <span className="flex items-center gap-1"><Clock className="h-3.5 w-3.5" />{b.booking_time.slice(0, 5)}</span>
                    <span className="font-bold gradient-text">${Number(b.service?.estimated_price ?? 0).toFixed(0)}</span>
                  </div>
                  {b.master_notes && (
                    <p className="mt-3 text-sm p-3 rounded-lg bg-secondary/60 border-l-2 border-primary">
                      <span className="font-semibold">Master: </span>{b.master_notes}
                    </p>
                  )}
                </div>
                <div className="flex gap-2">
                  {canCancel && (
                    <Button size="sm" variant="outline" onClick={() => cancel(b.id)}>{t("dash.cancel")}</Button>
                  )}
                  {canReview && (
                    <Button size="sm" onClick={() => setReviewFor(b)} className="bg-gradient-primary text-primary-foreground hover:opacity-90">
                      <Star className="h-3.5 w-3.5 mr-1" />{t("dash.review")}
                    </Button>
                  )}
                </div>
              </div>
            </Card>
          );
        })}
      </div>

      <ReviewDialog booking={reviewFor} onClose={() => setReviewFor(null)} />
    </>
  );
}

function ReviewDialog({ booking, onClose }: { booking: UserBooking | null; onClose: () => void }) {
  const { t } = useI18n();
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState("");
  const [loading, setLoading] = useState(false);

  const submit = async () => {
    setLoading(true);
    try {
      await createReview(rating, comment || null);
      toast.success(t("toast.reviewSubmitted"));
      setComment("");
      setRating(5);
      onClose();
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "Review failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={!!booking} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="glass">
        <DialogHeader>
          <DialogTitle>{t("dash.review")}</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <div>
            <p className="text-sm font-medium mb-2">{t("dash.rating")}</p>
            <div className="flex gap-1">
              {[1, 2, 3, 4, 5].map((n) => (
                <button key={n} type="button" onClick={() => setRating(n)}>
                  <Star className={`h-8 w-8 transition-colors ${n <= rating ? "fill-warning text-warning" : "text-muted-foreground/30"}`} />
                </button>
              ))}
            </div>
          </div>
          <div>
            <p className="text-sm font-medium mb-2">{t("dash.comment")}</p>
            <Textarea value={comment} onChange={(e) => setComment(e.target.value)} rows={4} />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>{t("common.close")}</Button>
          <Button onClick={submit} disabled={loading} className="bg-gradient-primary text-primary-foreground hover:opacity-90">
            {t("dash.submit")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
