import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from "react";
import type { AuthUser } from "@/lib/api/auth";
import { fetchMe, logout as apiLogout } from "@/lib/api/auth";

type Ctx = {
  user: AuthUser | null;
  isAdmin: boolean;
  loading: boolean;
  signOut: () => Promise<void>;
  refreshUser: (knownUser?: AuthUser) => Promise<void>;
};

const AuthCtx = createContext<Ctx>({
  user: null,
  isAdmin: false,
  loading: true,
  signOut: async () => {},
  refreshUser: async () => {},
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);

  const refreshUser = useCallback(async (knownUser?: AuthUser) => {
    if (knownUser) {
      setUser(knownUser);
      return;
    }
    const me = await fetchMe();
    setUser(me);
  }, []);

  useEffect(() => {
    refreshUser().finally(() => setLoading(false));
  }, [refreshUser]);

  const signOut = async () => {
    apiLogout();
    setUser(null);
  };

  return (
    <AuthCtx.Provider
      value={{
        user,
        isAdmin: user?.isAdmin ?? false,
        loading,
        signOut,
        refreshUser,
      }}
    >
      {children}
    </AuthCtx.Provider>
  );
}

export const useAuth = () => useContext(AuthCtx);
