import { useEffect, useState } from "react";
import { Card } from "@/components/ui/card";
import { useI18n } from "@/lib/i18n";
import { getAnalytics } from "@/lib/api/admin";
import { BarChart, Bar, XAxis, YAxis, ResponsiveContainer, Tooltip, CartesianGrid } from "recharts";
import { TrendingUp, Calendar, DollarSign } from "lucide-react";

export function AdminAnalytics() {
  const { t } = useI18n();
  const [totalBookings, setTotalBookings] = useState(0);
  const [revenue, setRevenue] = useState(0);
  const [active, setActive] = useState(0);
  const [days, setDays] = useState<{ day: string; bookings: number }[]>([]);

  useEffect(() => {
    getAnalytics().then((a) => {
      setTotalBookings(a.totalBookings);
      setRevenue(a.revenue);
      setActive(a.activeBookings);
      setDays(a.last7Days.map((d) => ({ day: d.day, bookings: d.bookings })));
    });
  }, []);

  return (
    <div className="space-y-6">
      <div className="grid sm:grid-cols-3 gap-4">
        <StatCard icon={Calendar} label={t("admin.total")} value={totalBookings.toString()} />
        <StatCard icon={DollarSign} label={t("admin.revenue")} value={`$${revenue.toFixed(0)}`} />
        <StatCard icon={TrendingUp} label="Active" value={active.toString()} />
      </div>

      <Card className="p-6 glass">
        <h3 className="font-semibold mb-4">{t("admin.last7")}</h3>
        <div className="h-72">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={days}>
              <defs>
                <linearGradient id="barGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="var(--primary)" stopOpacity={1} />
                  <stop offset="100%" stopColor="var(--primary-glow)" stopOpacity={0.6} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
              <XAxis dataKey="day" stroke="var(--muted-foreground)" />
              <YAxis stroke="var(--muted-foreground)" allowDecimals={false} />
              <Tooltip
                contentStyle={{ background: "var(--popover)", border: "1px solid var(--border)", borderRadius: 12 }}
                cursor={{ fill: "var(--muted)", opacity: 0.3 }}
              />
              <Bar dataKey="bookings" fill="url(#barGrad)" radius={[8, 8, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </Card>
    </div>
  );
}

function StatCard({ icon: Icon, label, value }: { icon: React.ComponentType<{ className?: string }>; label: string; value: string }) {
  return (
    <Card className="p-6 glass">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-muted-foreground">{label}</p>
          <p className="text-3xl font-bold mt-1 gradient-text">{value}</p>
        </div>
        <div className="h-12 w-12 rounded-xl bg-gradient-primary/15 flex items-center justify-center">
          <Icon className="h-6 w-6 text-primary" />
        </div>
      </div>
    </Card>
  );
}
