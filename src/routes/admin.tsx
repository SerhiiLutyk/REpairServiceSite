import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { useAuth } from "@/hooks/use-auth";
import { useI18n } from "@/lib/i18n";
import { Header } from "@/components/layout/Header";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { AdminOrders } from "@/components/admin/AdminOrders";
import { AdminServices } from "@/components/admin/AdminServices";
import { AdminAnalytics } from "@/components/admin/AdminAnalytics";
import { ShieldAlert, ClipboardList, Wrench, BarChart3 } from "lucide-react";

export const Route = createFileRoute("/admin")({
  head: () => ({ meta: [{ title: "Admin — NeoFix" }] }),
  component: AdminPage,
});

function AdminPage() {
  const { user, isAdmin, loading } = useAuth();
  const { t } = useI18n();
  const navigate = useNavigate();

  useEffect(() => {
    if (!loading && !user) navigate({ to: "/auth" });
  }, [user, loading, navigate]);

  if (loading) {
    return <div className="min-h-screen flex items-center justify-center"><div className="h-10 w-10 rounded-full border-4 border-primary/30 border-t-primary animate-spin" /></div>;
  }

  if (!isAdmin) {
    return (
      <div className="min-h-screen bg-mesh">
        <Header />
        <div className="min-h-screen flex items-center justify-center px-4">
          <Card className="p-10 text-center max-w-md glass">
            <div className="inline-flex h-16 w-16 rounded-2xl bg-destructive/15 items-center justify-center mb-4">
              <ShieldAlert className="h-8 w-8 text-destructive" />
            </div>
            <h1 className="text-2xl font-bold">403 — {t("403.title")}</h1>
            <p className="text-muted-foreground mt-2">{t("403.text")}</p>
            <Button className="mt-6" onClick={() => navigate({ to: "/dashboard" })}>{t("nav.cabinet")}</Button>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-mesh">
      <Header />
      <main className="container mx-auto px-4 sm:px-6 lg:px-8 pt-28 pb-16">
        <div className="mb-8">
          <h1 className="text-3xl sm:text-4xl font-bold">{t("admin.title")}</h1>
        </div>
        <Tabs defaultValue="orders">
          <TabsList className="grid grid-cols-3 max-w-xl glass">
            <TabsTrigger value="orders" className="gap-2"><ClipboardList className="h-4 w-4" /><span className="hidden sm:inline">{t("admin.orders")}</span></TabsTrigger>
            <TabsTrigger value="services" className="gap-2"><Wrench className="h-4 w-4" /><span className="hidden sm:inline">{t("admin.services")}</span></TabsTrigger>
            <TabsTrigger value="analytics" className="gap-2"><BarChart3 className="h-4 w-4" /><span className="hidden sm:inline">{t("admin.analytics")}</span></TabsTrigger>
          </TabsList>
          <TabsContent value="orders" className="mt-6"><AdminOrders /></TabsContent>
          <TabsContent value="services" className="mt-6"><AdminServices /></TabsContent>
          <TabsContent value="analytics" className="mt-6"><AdminAnalytics /></TabsContent>
        </Tabs>
      </main>
    </div>
  );
}
