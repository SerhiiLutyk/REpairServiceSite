import { useEffect, useState } from "react";
import { Link, useNavigate } from "@tanstack/react-router";
import { Moon, Sun, Menu, X, Globe, Wrench } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useI18n, type Lang } from "@/lib/i18n";
import { useTheme } from "@/lib/theme";
import { useAuth } from "@/hooks/use-auth";

const LANGS: { code: Lang; flag: string; label: string }[] = [
  { code: "en", flag: "🇬🇧", label: "English" },
  
  { code: "uk", flag: "🇺🇦", label: "Українська" },
];

export function Header() {
  const { t, lang, setLang } = useI18n();
  const { theme, toggle } = useTheme();
  const { user, isAdmin, signOut } = useAuth();
  const navigate = useNavigate();
  const [scrolled, setScrolled] = useState(false);
  const [open, setOpen] = useState(false);

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 20);
    onScroll();
    window.addEventListener("scroll", onScroll);
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  const current = LANGS.find((l) => l.code === lang)!;

  const sections = [
    { id: "services", label: t("nav.services") },
    { id: "reviews", label: t("nav.reviews") },
    { id: "contact", label: t("nav.contact") },
  ];

  const scrollTo = (id: string) => {
    setOpen(false);
    if (window.location.pathname !== "/") {
      navigate({ to: "/", hash: id });
      return;
    }
    document.getElementById(id)?.scrollIntoView({ behavior: "smooth" });
  };

  return (
    <header
      className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 ${
        scrolled ? "glass shadow-soft" : "bg-transparent"
      }`}
    >
      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex h-16 items-center justify-between">
          <Link to="/" className="flex items-center gap-2 group">
            <div className="h-9 w-9 rounded-xl bg-gradient-primary flex items-center justify-center shadow-glow group-hover:scale-110 transition-transform">
              <Wrench className="h-5 w-5 text-primary-foreground" />
            </div>
            <span className="text-xl font-bold tracking-tight">
              Neo<span className="gradient-text">Fix</span>
            </span>
          </Link>

          <nav className="hidden md:flex items-center gap-8">
            {sections.map((s) => (
              <button
                key={s.id}
                onClick={() => scrollTo(s.id)}
                className="text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
              >
                {s.label}
              </button>
            ))}
          </nav>

          <div className="flex items-center gap-2">
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="sm" className="gap-1.5">
                  <Globe className="h-4 w-4" />
                  <span className="hidden sm:inline">{current.flag}</span>
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="glass">
                {LANGS.map((l) => (
                  <DropdownMenuItem key={l.code} onClick={() => setLang(l.code)}>
                    <span className="mr-2">{l.flag}</span> {l.label}
                  </DropdownMenuItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>

            <Button variant="ghost" size="icon" onClick={toggle} aria-label="theme">
              {theme === "dark" ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
            </Button>

            {user ? (
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button size="sm" className="hidden sm:inline-flex bg-gradient-primary text-primary-foreground hover:opacity-90 shadow-glow">
                    {t("nav.cabinet")}
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="glass">
                  <DropdownMenuItem onClick={() => navigate({ to: "/dashboard" })}>
                    {t("nav.cabinet")}
                  </DropdownMenuItem>
                  {isAdmin && (
                    <DropdownMenuItem onClick={() => navigate({ to: "/admin" })}>
                      {t("nav.admin")}
                    </DropdownMenuItem>
                  )}
                  <DropdownMenuItem onClick={() => signOut()}>{t("nav.signout")}</DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            ) : (
              <Button
                size="sm"
                onClick={() => navigate({ to: "/auth" })}
                className="hidden sm:inline-flex bg-gradient-primary text-primary-foreground hover:opacity-90 shadow-glow"
              >
                {t("nav.signin")}
              </Button>
            )}

            <Button
              variant="ghost"
              size="icon"
              className="md:hidden"
              onClick={() => setOpen(!open)}
              aria-label="menu"
            >
              {open ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
            </Button>
          </div>
        </div>

        {open && (
          <div className="md:hidden pb-4 animate-in slide-in-from-top-4 fade-in duration-200">
            <div className="flex flex-col gap-1 glass rounded-xl p-3">
              {sections.map((s) => (
                <button
                  key={s.id}
                  onClick={() => scrollTo(s.id)}
                  className="text-left px-3 py-2 rounded-lg hover:bg-accent text-sm font-medium"
                >
                  {s.label}
                </button>
              ))}
              {user ? (
                <>
                  <button
                    onClick={() => { setOpen(false); navigate({ to: "/dashboard" }); }}
                    className="text-left px-3 py-2 rounded-lg hover:bg-accent text-sm font-medium"
                  >
                    {t("nav.cabinet")}
                  </button>
                  {isAdmin && (
                    <button
                      onClick={() => { setOpen(false); navigate({ to: "/admin" }); }}
                      className="text-left px-3 py-2 rounded-lg hover:bg-accent text-sm font-medium"
                    >
                      {t("nav.admin")}
                    </button>
                  )}
                  <button
                    onClick={() => { setOpen(false); signOut(); }}
                    className="text-left px-3 py-2 rounded-lg hover:bg-accent text-sm font-medium"
                  >
                    {t("nav.signout")}
                  </button>
                </>
              ) : (
                <button
                  onClick={() => { setOpen(false); navigate({ to: "/auth" }); }}
                  className="text-left px-3 py-2 rounded-lg bg-gradient-primary text-primary-foreground text-sm font-semibold"
                >
                  {t("nav.signin")}
                </button>
              )}
            </div>
          </div>
        )}
      </div>
    </header>
  );
}
