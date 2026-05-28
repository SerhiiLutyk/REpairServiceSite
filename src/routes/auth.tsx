import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { ApiError } from "@/lib/api/client";
import { login, register as signUp } from "@/lib/api/auth";
import { useAuth } from "@/hooks/use-auth";
import { useI18n } from "@/lib/i18n";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Header } from "@/components/layout/Header";
import { toast } from "sonner";
import { Loader2, Wrench } from "lucide-react";

export const Route = createFileRoute("/auth")({
  head: () => ({ meta: [{ title: "Sign In — NeoFix" }] }),
  component: AuthPage,
});

const signInSchema = z.object({
  email: z.string().email(),
  password: z.string().min(6),
});
const signUpSchema = signInSchema.extend({
  full_name: z.string().min(2).max(100),
  phone_number: z.string().min(5).max(30),
});

function AuthPage() {
  const { t } = useI18n();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [tab, setTab] = useState<"signin" | "signup">("signin");

  useEffect(() => {
    if (user) navigate({ to: "/dashboard" });
  }, [user, navigate]);

  return (
    <div className="min-h-screen bg-mesh">
      <Header />
      <div className="flex items-center justify-center min-h-screen px-4 pt-24 pb-12">
        <Card className="w-full max-w-md p-8 glass shadow-soft">
          <div className="text-center mb-6">
            <div className="inline-flex h-14 w-14 rounded-2xl bg-gradient-primary items-center justify-center shadow-glow mb-4">
              <Wrench className="h-7 w-7 text-primary-foreground" />
            </div>
            <h1 className="text-2xl font-bold">{t("auth.title")}</h1>
          </div>

          <Tabs value={tab} onValueChange={(v) => setTab(v as "signin" | "signup")}>
            <TabsList className="grid grid-cols-2 w-full mb-4">
              <TabsTrigger value="signin">{t("auth.signin")}</TabsTrigger>
              <TabsTrigger value="signup">{t("auth.signup")}</TabsTrigger>
            </TabsList>
            <TabsContent value="signin"><SignInForm /></TabsContent>
            <TabsContent value="signup"><SignUpForm onDone={() => setTab("signin")} /></TabsContent>
          </Tabs>
        </Card>
      </div>
    </div>
  );
}

function SignInForm() {
  const { t } = useI18n();
  const navigate = useNavigate();
  const { refreshUser } = useAuth();
  const [loading, setLoading] = useState(false);
  const { register, handleSubmit, formState: { errors } } = useForm({ resolver: zodResolver(signInSchema) });

  const onSubmit = async (data: z.infer<typeof signInSchema>) => {
    setLoading(true);
    try {
      const user = await login(data.email, data.password);
      await refreshUser(user);
      navigate({ to: "/dashboard" });
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "Sign in failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-2">
        <Label>{t("auth.email")}</Label>
        <Input type="email" {...register("email")} />
        {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
      </div>
      <div className="space-y-2">
        <Label>{t("auth.password")}</Label>
        <Input type="password" {...register("password")} />
        {errors.password && <p className="text-xs text-destructive">{errors.password.message}</p>}
      </div>
      <Button type="submit" disabled={loading} className="w-full bg-gradient-primary text-primary-foreground hover:opacity-90 shadow-glow">
        {loading && <Loader2 className="h-4 w-4 animate-spin mr-2" />}
        {t("auth.signin")}
      </Button>
    </form>
  );
}

function SignUpForm({ onDone }: { onDone: () => void }) {
  const { t } = useI18n();
  const navigate = useNavigate();
  const { refreshUser } = useAuth();
  const [loading, setLoading] = useState(false);
  const { register: registerField, handleSubmit, formState: { errors } } = useForm({
    resolver: zodResolver(signUpSchema),
  });

  const onSubmit = async (data: z.infer<typeof signUpSchema>) => {
    setLoading(true);
    try {
      const user = await signUp({
        email: data.email,
        password: data.password,
        fullName: data.full_name,
        phoneNumber: data.phone_number,
      });
      await refreshUser(user);
      toast.success("Account created!");
      navigate({ to: "/dashboard" });
      onDone();
    } catch (e) {
      const msg =
        e instanceof ApiError
          ? e.message
          : e instanceof Error
            ? e.message
            : "Registration failed";
      toast.error(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-2">
        <Label>{t("auth.fullname")}</Label>
        <Input {...registerField("full_name")} />
        {errors.full_name && <p className="text-xs text-destructive">{errors.full_name.message}</p>}
      </div>
      <div className="space-y-2">
        <Label>{t("auth.phone")}</Label>
        <Input {...registerField("phone_number")} />
        {errors.phone_number && <p className="text-xs text-destructive">{errors.phone_number.message}</p>}
      </div>
      <div className="space-y-2">
        <Label>{t("auth.email")}</Label>
        <Input type="email" {...registerField("email")} />
        {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
      </div>
      <div className="space-y-2">
        <Label>{t("auth.password")}</Label>
        <Input type="password" {...registerField("password")} />
        {errors.password && <p className="text-xs text-destructive">{errors.password.message}</p>}
      </div>
      <Button type="submit" disabled={loading} className="w-full bg-gradient-primary text-primary-foreground hover:opacity-90 shadow-glow">
        {loading && <Loader2 className="h-4 w-4 animate-spin mr-2" />}
        {t("auth.signup")}
      </Button>
    </form>
  );
}
