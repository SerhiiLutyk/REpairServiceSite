import { useI18n } from "@/lib/i18n";
import { Wrench } from "lucide-react";

export function Footer() {
  const { t } = useI18n();
  return (
    <footer className="border-t border-border/40 mt-20">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-10 flex flex-col md:flex-row items-center justify-between gap-4">
        <div className="flex items-center gap-2">
          <div className="h-7 w-7 rounded-lg bg-gradient-primary flex items-center justify-center">
            <Wrench className="h-3.5 w-3.5 text-primary-foreground" />
          </div>
          <span className="font-bold">Neo<span className="gradient-text">Fix</span></span>
        </div>
        <p className="text-sm text-muted-foreground">© {new Date().getFullYear()} NeoFix. {t("hero.tagline")}.</p>
      </div>
    </footer>
  );
}
