import { useEffect, useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { useI18n } from "@/lib/i18n";
import { ApiError } from "@/lib/api/client";
import {
  createService,
  deleteService,
  listServices,
  updateService,
  type Service,
} from "@/lib/api/services";
import { toast } from "sonner";
import { Plus, Pencil, Trash2 } from "lucide-react";

export function AdminServices() {
  const { t } = useI18n();
  const [items, setItems] = useState<Service[]>([]);
  const [edit, setEdit] = useState<Partial<Service> | null>(null);

  const load = () =>
    listServices()
      .then(setItems)
      .catch((e) => toast.error(e instanceof ApiError ? e.message : "Load failed"));

  useEffect(() => {
    load();
  }, []);

  const save = async () => {
    if (!edit?.device_type || !edit?.service_name) return toast.error("Fill required fields");
    const payload = {
      device_type: edit.device_type,
      service_name: edit.service_name,
      estimated_price: Number(edit.estimated_price ?? 0),
      estimated_duration_minutes: Number(edit.estimated_duration_minutes ?? 60),
    };
    try {
      if (edit.id) await updateService(edit.id, payload);
      else await createService(payload);
      toast.success("Saved");
      setEdit(null);
      load();
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "Save failed");
    }
  };

  const del = async (id: string) => {
    if (!confirm("Delete this service?")) return;
    try {
      await deleteService(id);
      toast.success("Deleted");
      load();
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "Delete failed");
    }
  };

  return (
    <>
      <Card className="p-4 glass">
        <div className="flex justify-between items-center mb-4">
          <h2 className="font-semibold">{t("admin.services")}</h2>
          <Button onClick={() => setEdit({})} className="bg-gradient-primary text-primary-foreground hover:opacity-90">
            <Plus className="h-4 w-4 mr-1" /> {t("admin.add")}
          </Button>
        </div>
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Device</TableHead>
                <TableHead>Service</TableHead>
                <TableHead>Price</TableHead>
                <TableHead>Duration</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {items.map((s) => (
                <TableRow key={s.id}>
                  <TableCell className="font-medium">{s.device_type}</TableCell>
                  <TableCell>{s.service_name}</TableCell>
                  <TableCell className="font-semibold">${Number(s.estimated_price).toFixed(0)}</TableCell>
                  <TableCell>{s.estimated_duration_minutes}m</TableCell>
                  <TableCell className="text-right">
                    <Button size="icon" variant="ghost" onClick={() => setEdit(s)}><Pencil className="h-4 w-4" /></Button>
                    <Button size="icon" variant="ghost" onClick={() => del(s.id)}><Trash2 className="h-4 w-4 text-destructive" /></Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </Card>

      <Dialog open={!!edit} onOpenChange={(o) => !o && setEdit(null)}>
        <DialogContent className="glass">
          <DialogHeader><DialogTitle>{edit?.id ? t("admin.edit") : t("admin.add")}</DialogTitle></DialogHeader>
          <div className="space-y-3">
            <div className="space-y-1.5"><Label>Device type</Label><Input value={edit?.device_type ?? ""} onChange={(e) => setEdit({ ...edit, device_type: e.target.value })} /></div>
            <div className="space-y-1.5"><Label>Service name</Label><Input value={edit?.service_name ?? ""} onChange={(e) => setEdit({ ...edit, service_name: e.target.value })} /></div>
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1.5"><Label>Price ($)</Label><Input type="number" value={edit?.estimated_price ?? ""} onChange={(e) => setEdit({ ...edit, estimated_price: Number(e.target.value) })} /></div>
              <div className="space-y-1.5"><Label>Duration (min)</Label><Input type="number" value={edit?.estimated_duration_minutes ?? ""} onChange={(e) => setEdit({ ...edit, estimated_duration_minutes: Number(e.target.value) })} /></div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setEdit(null)}>{t("common.cancel")}</Button>
            <Button onClick={save} className="bg-gradient-primary text-primary-foreground hover:opacity-90">{t("dash.save")}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
