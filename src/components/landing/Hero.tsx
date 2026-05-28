import { useNavigate } from "@tanstack/react-router";
import { Button } from "@/components/ui/button";
import { ArrowRight, Sparkles } from "lucide-react";
import { useI18n } from "@/lib/i18n";

export function Hero() {
  const { t } = useI18n();
  const navigate = useNavigate();

  const scrollTo = (id: string) => document.getElementById(id)?.scrollIntoView({ behavior: "smooth" });

  return (
    <section className="relative min-h-screen flex items-center justify-center overflow-hidden pt-24 pb-16 bg-mesh">
      {/* Floating glow blobs */}
      <div className="absolute top-1/4 -left-32 h-96 w-96 rounded-full bg-primary/30 blur-3xl animate-pulse" />
      <div className="absolute bottom-1/4 -right-32 h-96 w-96 rounded-full bg-primary-glow/30 blur-3xl animate-pulse" style={{ animationDelay: "2s" }} />

      <div className="container relative mx-auto px-4 sm:px-6 lg:px-8 text-center">
        <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full glass text-xs font-medium mb-8 shadow-soft">
          <Sparkles className="h-3 w-3 text-primary" />
          <span>{t("hero.tagline")}</span>
        </div>

        <h1 className="text-5xl sm:text-6xl lg:text-7xl font-extrabold tracking-tight leading-[1.05] max-w-4xl mx-auto">
          {t("hero.title").split(",").map((part, i) => (
            <span key={i} className={i === 1 ? "block gradient-text" : "block"}>
              {part.trim()}{i === 0 ? "," : ""}
            </span>
          ))}
        </h1>

        <p className="mt-6 text-lg sm:text-xl text-muted-foreground max-w-2xl mx-auto">
          {t("hero.subtitle")}
        </p>

        <div className="mt-10 flex flex-col sm:flex-row items-center justify-center gap-3">
          <Button
            size="lg"
            onClick={() => navigate({ to: "/dashboard" })}
            className="bg-gradient-primary text-primary-foreground hover:opacity-90 shadow-glow text-base h-12 px-8"
          >
            {t("hero.cta")}
            <ArrowRight className="h-4 w-4 ml-1" />
          </Button>
          <Button
            size="lg"
            variant="outline"
            onClick={() => scrollTo("services")}
            className="glass h-12 px-8 text-base"
          >
            {t("hero.secondary")}
          </Button>
        </div>

        <div className="mt-20 flex items-center justify-center gap-8 text-xs text-muted-foreground">
          <div className="flex items-center gap-2"><div className="h-2 w-2 rounded-full bg-success animate-pulse" /> Open now</div>
          <div>★ 4.9 / 5 · 2,400+ repairs</div>
        </div>
      </div>
    </section>
  );
}
