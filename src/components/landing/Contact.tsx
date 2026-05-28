import { Card } from "@/components/ui/card";
import { Phone, Mail, MapPin, Instagram, Facebook, Twitter } from "lucide-react";
import { useI18n } from "@/lib/i18n";

export function Contact() {
  const { t } = useI18n();
  const address = "123 Tech Avenue, Kyiv";
  const mapsUrl = `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(address)}`;

  return (
    <section id="contact" className="py-20 sm:py-32">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center max-w-2xl mx-auto mb-12">
          <h2 className="text-4xl sm:text-5xl font-bold tracking-tight">{t("contact.title")}</h2>
          <p className="mt-4 text-lg text-muted-foreground">{t("contact.subtitle")}</p>
        </div>

        <div className="grid sm:grid-cols-3 gap-4 max-w-4xl mx-auto">
          <a href={`tel:${t("contact.phone").replace(/\s/g, "")}`}>
            <Card className="p-6 glass hover:shadow-glow transition-all hover:-translate-y-1 cursor-pointer h-full">
              <Phone className="h-6 w-6 text-primary mb-3" />
              <p className="text-sm text-muted-foreground mb-1">Phone</p>
              <p className="font-semibold">{t("contact.phone")}</p>
            </Card>
          </a>
          <a href={`mailto:${t("contact.email")}`}>
            <Card className="p-6 glass hover:shadow-glow transition-all hover:-translate-y-1 cursor-pointer h-full">
              <Mail className="h-6 w-6 text-primary mb-3" />
              <p className="text-sm text-muted-foreground mb-1">Email</p>
              <p className="font-semibold">{t("contact.email")}</p>
            </Card>
          </a>
          <a href={mapsUrl} target="_blank" rel="noopener noreferrer">
            <Card className="p-6 glass hover:shadow-glow transition-all hover:-translate-y-1 cursor-pointer h-full">
              <MapPin className="h-6 w-6 text-primary mb-3" />
              <p className="text-sm text-muted-foreground mb-1">Address</p>
              <p className="font-semibold">{t("contact.address")}</p>
            </Card>
          </a>
        </div>

        <div className="mt-10 flex justify-center gap-3">
          {[
            { Icon: Instagram, href: "https://instagram.com" },
            { Icon: Facebook, href: "https://facebook.com" },
            { Icon: Twitter, href: "https://twitter.com" },
          ].map(({ Icon, href }, i) => (
            <a
              key={i}
              href={href}
              target="_blank"
              rel="noopener noreferrer"
              className="h-11 w-11 rounded-full glass flex items-center justify-center hover:shadow-glow transition-all hover:-translate-y-0.5"
            >
              <Icon className="h-4 w-4" />
            </a>
          ))}
        </div>
      </div>
    </section>
  );
}
