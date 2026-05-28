import { useEffect, useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAuth } from "@/hooks/use-auth";
import { useI18n } from "@/lib/i18n";
import { ApiError } from "@/lib/api/client";
import { getMyProfile, updateMyProfile } from "@/lib/api/profiles";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";

export function ProfileForm() {
  const { user } = useAuth();
  const { t } = useI18n();
  const [full_name, setFullName] = useState("");
  const [phone_number, setPhone] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!user) return;
    getMyProfile()
      .then((data) => {
        setFullName(data.full_name ?? "");
        setPhone(data.phone_number ?? "");
      })
      .catch(() => {
        setFullName(user.fullName ?? "");
        setPhone(user.phoneNumber ?? "");
      });
  }, [user]);

  const save = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return;
    setLoading(true);
    try {
      await updateMyProfile({ fullName: full_name, phoneNumber: phone_number });
      toast.success(t("toast.profileSaved"));
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "Save failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card className="p-6 sm:p-8 glass max-w-lg">
      <form onSubmit={save} className="space-y-4">
        <div className="space-y-2">
          <Label>{t("auth.email")}</Label>
          <Input value={user?.email ?? ""} disabled />
        </div>
        <div className="space-y-2">
          <Label>{t("auth.fullname")}</Label>
          <Input value={full_name} onChange={(e) => setFullName(e.target.value)} />
        </div>
        <div className="space-y-2">
          <Label>{t("auth.phone")}</Label>
          <Input value={phone_number} onChange={(e) => setPhone(e.target.value)} />
        </div>
        <Button type="submit" disabled={loading} className="bg-gradient-primary text-primary-foreground hover:opacity-90 shadow-glow">
          {loading && <Loader2 className="h-4 w-4 animate-spin mr-2" />}
          {t("dash.save")}
        </Button>
      </form>
    </Card>
  );
}
