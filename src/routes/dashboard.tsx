import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { useAuth } from "@/hooks/use-auth";
import { useI18n } from "@/lib/i18n";
import { Header } from "@/components/layout/Header";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { NewBooking } from "@/components/dashboard/NewBooking";
import { MyOrders } from "@/components/dashboard/MyOrders";
import { ProfileForm } from "@/components/dashboard/ProfileForm";
import { Calendar, ListOrdered, User } from "lucide-react";

export const Route = createFileRoute("/dashboard")({
  head: () => ({ meta: [{ title: "Dashboard — NeoFix" }] }),
  component: Dashboard,
});

function Dashboard() {
  const { user, loading } = useAuth();
  const { t } = useI18n();
  const navigate = useNavigate();
  const [tab, setTab] = useState("new");

  useEffect(() => {
    if (!loading && !user) navigate({ to: "/auth" });
  }, [user, loading, navigate]);

  if (loading || !user) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="h-10 w-10 rounded-full border-4 border-primary/30 border-t-primary animate-spin" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-mesh">
      <Header />
      <main className="container mx-auto px-4 sm:px-6 lg:px-8 pt-28 pb-16">
        <div className="mb-8">
          <h1 className="text-3xl sm:text-4xl font-bold">{t("nav.cabinet")}</h1>
          <p className="text-muted-foreground mt-1">{user.email}</p>
        </div>

        <Tabs value={tab} onValueChange={setTab}>
          <TabsList className="grid grid-cols-3 w-full max-w-xl glass">
            <TabsTrigger value="new" className="gap-2"><Calendar className="h-4 w-4" /><span className="hidden sm:inline">{t("dash.new")}</span></TabsTrigger>
            <TabsTrigger value="orders" className="gap-2"><ListOrdered className="h-4 w-4" /><span className="hidden sm:inline">{t("dash.orders")}</span></TabsTrigger>
            <TabsTrigger value="profile" className="gap-2"><User className="h-4 w-4" /><span className="hidden sm:inline">{t("dash.profile")}</span></TabsTrigger>
          </TabsList>

          <TabsContent value="new" className="mt-6"><NewBooking onCreated={() => setTab("orders")} /></TabsContent>
          <TabsContent value="orders" className="mt-6"><MyOrders /></TabsContent>
          <TabsContent value="profile" className="mt-6"><ProfileForm /></TabsContent>
        </Tabs>
      </main>
    </div>
  );
}
