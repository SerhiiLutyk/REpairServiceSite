import { useEffect, useMemo, useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Calendar } from "@/components/ui/calendar";
import { useAuth } from "@/hooks/use-auth";
import { useI18n } from "@/lib/i18n";
import { ApiError } from "@/lib/api/client";
import { createBooking, getTakenSlots } from "@/lib/api/bookings";
import { listServices, type Service } from "@/lib/api/services";
import { toast } from "sonner";
import { Loader2, Check } from "lucide-react";
import { format } from "date-fns";

const HOURS = Array.from({ length: 10 }, (_, i) => `${String(9 + i).padStart(2, "0")}:00`);

export function NewBooking({ onCreated }: { onCreated: () => void }) {
  const { user } = useAuth();
  const { t } = useI18n();
  const [services, setServices] = useState<Service[]>([]);
  const [device, setDevice] = useState<string>("");
  const [serviceId, setServiceId] = useState<string>("");
  const [date, setDate] = useState<Date | undefined>(new Date());
  const [time, setTime] = useState<string>("");
  const [taken, setTaken] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    listServices().then(setServices).catch(() => toast.error("Failed to load services"));
  }, []);

  const devices = useMemo(() => Array.from(new Set(services.map((s) => s.device_type))), [services]);
  const filtered = useMemo(() => services.filter((s) => s.device_type === device), [services, device]);
  const selected = services.find((s) => s.id === serviceId);

  useEffect(() => {
    if (!date) return;
    const d = format(date, "yyyy-MM-dd");
    getTakenSlots(d).then(setTaken).catch(() => setTaken([]));
  }, [date]);

  const submit = async () => {
    if (!user || !serviceId || !date || !time) return;
    setLoading(true);
    try {
      await createBooking({
        serviceId,
        bookingDate: format(date, "yyyy-MM-dd"),
        bookingTime: time + ":00",
      });
      toast.success(t("toast.bookingCreated"));
      setServiceId("");
      setTime("");
      onCreated();
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "Booking failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card className="p-6 sm:p-8 glass">
      <div className="grid lg:grid-cols-2 gap-8">
        <div className="space-y-5">
          <div>
            <label className="text-sm font-semibold mb-2 block">{t("dash.device")}</label>
            <Select value={device} onValueChange={(v) => { setDevice(v); setServiceId(""); }}>
              <SelectTrigger><SelectValue placeholder="—" /></SelectTrigger>
              <SelectContent>{devices.map((d) => <SelectItem key={d} value={d}>{d}</SelectItem>)}</SelectContent>
            </Select>
          </div>

          {device && (
            <div>
              <label className="text-sm font-semibold mb-2 block">{t("dash.service")}</label>
              <Select value={serviceId} onValueChange={setServiceId}>
                <SelectTrigger><SelectValue placeholder="—" /></SelectTrigger>
                <SelectContent>
                  {filtered.map((s) => (
                    <SelectItem key={s.id} value={s.id}>
                      {s.service_name} — ${Number(s.estimated_price).toFixed(0)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          )}

          {selected && (
            <Card className="p-4 bg-gradient-primary text-primary-foreground shadow-glow">
              <p className="text-xs opacity-80">{selected.service_name}</p>
              <p className="text-3xl font-bold mt-1">${Number(selected.estimated_price).toFixed(0)}</p>
              <p className="text-xs opacity-80 mt-1">~{selected.estimated_duration_minutes} min</p>
            </Card>
          )}
        </div>

        <div className="space-y-5">
          <div>
            <label className="text-sm font-semibold mb-2 block">{t("dash.date")}</label>
            <div className="rounded-lg border bg-background p-2">
              <Calendar
                mode="single"
                selected={date}
                onSelect={(d) => { setDate(d); setTime(""); }}
                disabled={(d) => d < new Date(new Date().toDateString())}
                className="mx-auto"
              />
            </div>
          </div>

          {date && (
            <div>
              <label className="text-sm font-semibold mb-2 block">{t("dash.time")}</label>
              <div className="grid grid-cols-5 gap-2">
                {HOURS.map((h) => {
                  const disabled = taken.includes(h);
                  const active = time === h;
                  return (
                    <button
                      key={h}
                      type="button"
                      disabled={disabled}
                      onClick={() => setTime(h)}
                      className={`relative h-10 rounded-lg text-sm font-medium border transition-all ${
                        active
                          ? "bg-gradient-primary text-primary-foreground border-transparent shadow-glow"
                          : disabled
                          ? "bg-muted text-muted-foreground/40 line-through cursor-not-allowed border-border"
                          : "bg-background hover:border-primary border-border"
                      }`}
                    >
                      {h}
                    </button>
                  );
                })}
              </div>
            </div>
          )}

          <Button
            disabled={!serviceId || !date || !time || loading}
            onClick={submit}
            className="w-full bg-gradient-primary text-primary-foreground hover:opacity-90 shadow-glow h-12"
          >
            {loading ? <Loader2 className="h-4 w-4 animate-spin mr-2" /> : <Check className="h-4 w-4 mr-2" />}
            {t("dash.confirm")}
          </Button>
        </div>
      </div>
    </Card>
  );
}
