import { useEffect, useState } from "react";
import { Card } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import { useI18n } from "@/lib/i18n";
import { ApiError } from "@/lib/api/client";
import {
  listAdminBookings,
  updateBookingStatus,
  updateMasterNotes,
  type AdminBooking,
  type BookingStatus,
} from "@/lib/api/bookings";
import { toast } from "sonner";

const STATUSES: BookingStatus[] = ["Pending", "Confirmed", "In Progress", "Completed", "Cancelled"];

export function AdminOrders() {
  const { t } = useI18n();
  const [orders, setOrders] = useState<AdminBooking[]>([]);
  const [filter, setFilter] = useState<string>("all");
  const [editing, setEditing] = useState<AdminBooking | null>(null);
  const [notes, setNotes] = useState("");

  const load = async () => {
    try {
      setOrders(await listAdminBookings());
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "Failed to load orders");
    }
  };

  useEffect(() => {
    load();
  }, []);

  const updateStatus = async (id: string, status: BookingStatus) => {
    try {
      await updateBookingStatus(id, status);
      toast.success("Status updated");
      load();
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "Update failed");
    }
  };

  const saveNotes = async () => {
    if (!editing) return;
    try {
      await updateMasterNotes(editing.id, notes);
      toast.success("Notes saved");
      setEditing(null);
      load();
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "Save failed");
    }
  };

  const filtered = filter === "all" ? orders : orders.filter((o) => o.status === filter);

  return (
    <>
      <Card className="p-4 glass">
        <div className="flex items-center justify-between mb-4 gap-4 flex-wrap">
          <div className="flex items-center gap-2">
            <span className="text-sm text-muted-foreground">{t("admin.filter")}:</span>
            <Select value={filter} onValueChange={setFilter}>
              <SelectTrigger className="w-44"><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="all">{t("admin.all")}</SelectItem>
                {STATUSES.map((s) => <SelectItem key={s} value={s}>{t(`status.${s}` as const)}</SelectItem>)}
              </SelectContent>
            </Select>
          </div>
          <Badge variant="outline">{filtered.length} {filtered.length === 1 ? "order" : "orders"}</Badge>
        </div>

        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Date / Time</TableHead>
                <TableHead>{t("admin.client")}</TableHead>
                <TableHead>Service</TableHead>
                <TableHead>Price</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>{t("admin.notes")}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtered.map((o) => (
                <TableRow key={o.id}>
                  <TableCell className="whitespace-nowrap text-sm">
                    {o.booking_date}<br /><span className="text-muted-foreground">{o.booking_time.slice(0, 5)}</span>
                  </TableCell>
                  <TableCell>
                    <div className="text-sm">{o.profile?.full_name || "—"}</div>
                    <div className="text-xs text-muted-foreground">{o.profile?.phone_number || "—"}</div>
                  </TableCell>
                  <TableCell className="text-sm">{o.service?.service_name}</TableCell>
                  <TableCell className="font-semibold">${Number(o.service?.estimated_price ?? 0).toFixed(0)}</TableCell>
                  <TableCell>
                    <Select value={o.status} onValueChange={(v) => updateStatus(o.id, v as BookingStatus)}>
                      <SelectTrigger className="w-36 h-8 text-xs"><SelectValue /></SelectTrigger>
                      <SelectContent>
                        {STATUSES.map((s) => <SelectItem key={s} value={s}>{t(`status.${s}` as const)}</SelectItem>)}
                      </SelectContent>
                    </Select>
                  </TableCell>
                  <TableCell>
                    <Button size="sm" variant="ghost" onClick={() => { setEditing(o); setNotes(o.master_notes ?? ""); }}>
                      {o.master_notes ? "Edit" : "Add"}
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {filtered.length === 0 && (
                <TableRow><TableCell colSpan={6} className="text-center py-12 text-muted-foreground">No orders</TableCell></TableRow>
              )}
            </TableBody>
          </Table>
        </div>
      </Card>

      <Dialog open={!!editing} onOpenChange={(o) => !o && setEditing(null)}>
        <DialogContent className="glass">
          <DialogHeader><DialogTitle>{t("admin.notes")}</DialogTitle></DialogHeader>
          <Textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={6} />
          <DialogFooter>
            <Button variant="outline" onClick={() => setEditing(null)}>{t("common.cancel")}</Button>
            <Button onClick={saveNotes} className="bg-gradient-primary text-primary-foreground hover:opacity-90">{t("dash.save")}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
