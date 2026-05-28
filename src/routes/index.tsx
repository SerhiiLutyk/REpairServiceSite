import { createFileRoute } from "@tanstack/react-router";
import { useEffect } from "react";
import { Header } from "@/components/layout/Header";
import { Footer } from "@/components/layout/Footer";
import { Hero } from "@/components/landing/Hero";
import { Services } from "@/components/landing/Services";
import { Reviews } from "@/components/landing/Reviews";
import { Contact } from "@/components/landing/Contact";

export const Route = createFileRoute("/")({
  head: () => ({
    meta: [
      { title: "NeoFix — Premium Gadget Repair" },
      { name: "description", content: "Smartphones, laptops, consoles. Certified masters, original parts, lightning-fast turnaround." },
    ],
  }),
  component: Index,
});

function Index() {
  // Smooth-scroll to hash on load (e.g., navigated from another route)
  useEffect(() => {
    const hash = window.location.hash.replace("#", "");
    if (hash) {
      setTimeout(() => document.getElementById(hash)?.scrollIntoView({ behavior: "smooth" }), 100);
    }
  }, []);

  return (
    <div className="min-h-screen">
      <Header />
      <main>
        <Hero />
        <Services />
        <Reviews />
        <Contact />
      </main>
      <Footer />
    </div>
  );
}
