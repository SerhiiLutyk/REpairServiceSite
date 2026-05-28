import { useEffect, useState } from "react";
import { Smartphone, Laptop, Gamepad2, Tablet, ArrowRight, Sparkles, Shield, Zap } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from "@/components/ui/dialog";
import { useI18n } from "@/lib/i18n";
import { useNavigate } from "@tanstack/react-router";
import { listServices } from "@/lib/api/services";

type ServiceCategory = {
  key: string;
  icon: typeof Smartphone;
  device: string;
  title: { en: string; ru: string; uk: string };
  desc: { en: string; ru: string; uk: string };
};

const CATEGORIES: ServiceCategory[] = [
  {
    key: "smartphone",
    icon: Smartphone,
    device: "Smartphone",
    title: { en: "Smartphones", ru: "Смартфоны", uk: "Смартфони" },
    desc: {
      en: "Screen, battery, charging port, water damage — iOS and Android, expertly repaired.",
      ru: "Экран, батарея, разъём зарядки, вода — iOS и Android, профессионально.",
      uk: "Екран, батарея, роз'єм зарядки, вода — iOS і Android, професійно.",
    },
  },
  {
    key: "laptop",
    icon: Laptop,
    device: "Laptop",
    title: { en: "Laptops", ru: "Ноутбуки", uk: "Ноутбуки" },
    desc: {
      en: "Keyboard, SSD, thermal repaste, screen — MacBook, ThinkPad, Dell and more.",
      ru: "Клавиатура, SSD, термопаста, экран — MacBook, ThinkPad, Dell и др.",
      uk: "Клавіатура, SSD, термопаста, екран — MacBook, ThinkPad, Dell тощо.",
    },
  },
  {
    key: "console",
    icon: Gamepad2,
    device: "Console",
    title: { en: "Consoles", ru: "Консоли", uk: "Консолі" },
    desc: {
      en: "PS5, Xbox, Switch — HDMI, cleaning, controller drift, all sorted.",
      ru: "PS5, Xbox, Switch — HDMI, чистка, дрейф стиков.",
      uk: "PS5, Xbox, Switch — HDMI, чистка, дрейф стіків.",
    },
  },
  {
    key: "tablet",
    icon: Tablet,
    device: "Tablet",
    title: { en: "Tablets", ru: "Планшеты", uk: "Планшети" },
    desc: {
      en: "iPad, Galaxy Tab — screens, batteries, ports. Original parts only.",
      ru: "iPad, Galaxy Tab — экраны, батареи, разъёмы. Оригинал.",
      uk: "iPad, Galaxy Tab — екрани, батареї, роз'єми. Оригінал.",
    },
  },
];

type Service = { id: string; device_type: string; service_name: string; estimated_price: number };

export function Services() {
  const { t, lang } = useI18n();
  const navigate = useNavigate();
  const [openCat, setOpenCat] = useState<ServiceCategory | null>(null);
  const [services, setServices] = useState<Service[]>([]);

  useEffect(() => {
    listServices(true).then(setServices).catch(() => setServices([]));
  }, []);

  const handleBook = () => {
    setOpenCat(null);
    navigate({ to: "/dashboard" });
  };

  return (
    <section id="services" className="py-20 sm:py-32 relative">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center max-w-2xl mx-auto mb-16">
          <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full glass text-xs font-medium text-muted-foreground mb-4">
            <Sparkles className="h-3 w-3" /> {t("hero.tagline")}
          </div>
          <h2 className="text-4xl sm:text-5xl font-bold tracking-tight">
            {t("services.title")}
          </h2>
          <p className="mt-4 text-lg text-muted-foreground">{t("services.subtitle")}</p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {CATEGORIES.map((cat) => {
            const Icon = cat.icon;
            return (
              <Card
                key={cat.key}
                className="group relative overflow-hidden p-6 hover:shadow-glow transition-all duration-500 hover:-translate-y-1 border-border/50 glass"
              >
                <div className="absolute inset-0 bg-gradient-primary opacity-0 group-hover:opacity-5 transition-opacity" />
                <div className="relative">
                  <div className="h-12 w-12 rounded-xl bg-gradient-primary flex items-center justify-center shadow-glow mb-4">
                    <Icon className="h-6 w-6 text-primary-foreground" />
                  </div>
                  <h3 className="text-xl font-bold mb-2">{cat.title[lang]}</h3>
                  <p className="text-sm text-muted-foreground mb-6 line-clamp-3">{cat.desc[lang]}</p>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="group/btn -ml-2"
                    onClick={() => setOpenCat(cat)}
                  >
                    {t("services.learn")}
                    <ArrowRight className="h-3.5 w-3.5 ml-1 group-hover/btn:translate-x-1 transition-transform" />
                  </Button>
                </div>
              </Card>
            );
          })}
        </div>

        <div className="grid sm:grid-cols-3 gap-4 mt-16 max-w-3xl mx-auto">
          {[
            { icon: Shield, en: "90-day warranty", ru: "Гарантия 90 дней", uk: "Гарантія 90 днів" },
            { icon: Zap, en: "Same-day fixes", ru: "Ремонт за день", uk: "Ремонт за день" },
            { icon: Sparkles, en: "Original parts only", ru: "Оригинальные запчасти", uk: "Оригінальні запчастини" },
          ].map((f, i) => {
            const Icon = f.icon;
            return (
              <div key={i} className="flex items-center gap-3 glass rounded-2xl p-4">
                <Icon className="h-5 w-5 text-primary" />
                <span className="text-sm font-medium">{f[lang]}</span>
              </div>
            );
          })}
        </div>
      </div>

      <Dialog open={!!openCat} onOpenChange={(o) => !o && setOpenCat(null)}>
        <DialogContent className="max-w-lg glass">
          {openCat && (
            <>
              <DialogHeader>
                <DialogTitle className="text-2xl flex items-center gap-3">
                  <div className="h-10 w-10 rounded-xl bg-gradient-primary flex items-center justify-center">
                    <openCat.icon className="h-5 w-5 text-primary-foreground" />
                  </div>
                  {openCat.title[lang]}
                </DialogTitle>
                <DialogDescription className="text-base pt-2">{openCat.desc[lang]}</DialogDescription>
              </DialogHeader>
              <div className="space-y-2 max-h-72 overflow-y-auto">
                {services.filter((s) => s.device_type === openCat.device).map((s) => (
                  <div key={s.id} className="flex items-center justify-between p-3 rounded-lg bg-secondary/50">
                    <span className="text-sm font-medium">{s.service_name}</span>
                    <span className="text-sm font-bold gradient-text">${Number(s.estimated_price).toFixed(0)}</span>
                  </div>
                ))}
                {services.filter((s) => s.device_type === openCat.device).length === 0 && (
                  <p className="text-sm text-muted-foreground text-center py-6">Loading…</p>
                )}
              </div>
              <DialogFooter>
                <Button onClick={handleBook} className="w-full bg-gradient-primary text-primary-foreground hover:opacity-90 shadow-glow">
                  {t("services.book")}
                </Button>
              </DialogFooter>
            </>
          )}
        </DialogContent>
      </Dialog>
    </section>
  );
}
