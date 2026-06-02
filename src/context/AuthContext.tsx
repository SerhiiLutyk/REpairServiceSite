import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import {
  TOKEN_KEY,
  getMe,
  login as apiLogin,
  register as apiRegister,
  updateProfile as apiUpdateProfile,
  type UpdateProfile,
  type User,
} from '@/lib/api'

interface AuthContextValue {
  user: User | null
  loading: boolean
  isAdmin: boolean
  login: (login: string, password: string) => Promise<void>
  register: (fullName: string, phone: string, password: string, email?: string) => Promise<void>
  updateProfile: (data: UpdateProfile) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!localStorage.getItem(TOKEN_KEY)) {
      setLoading(false)
      return
    }
    getMe()
      .then(setUser)
      .catch(() => localStorage.removeItem(TOKEN_KEY))
      .finally(() => setLoading(false))
  }, [])

  async function login(login: string, password: string) {
    const res = await apiLogin(login, password)
    localStorage.setItem(TOKEN_KEY, res.token)
    setUser(res.user)
  }

  async function register(fullName: string, phone: string, password: string, email?: string) {
    const res = await apiRegister(fullName, phone, password, email)
    localStorage.setItem(TOKEN_KEY, res.token)
    setUser(res.user)
  }

  async function updateProfile(data: UpdateProfile) {
    const updated = await apiUpdateProfile(data)
    setUser(updated)
  }

  function logout() {
    localStorage.removeItem(TOKEN_KEY)
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, loading, isAdmin: user?.role === 1, login, register, updateProfile, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
