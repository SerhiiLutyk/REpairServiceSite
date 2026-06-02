import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Loader2 } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [loginValue, setLoginValue] = useState('')
  const [password, setPassword] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      await login(loginValue, password)
      navigate('/')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Помилка входу')
    } finally {
      setLoading(false)
    }
  }

  return (
    <section className="mx-auto max-w-md px-4 py-16">
      <h1 className="text-center text-3xl font-bold text-slate-900">Вхід</h1>
      <form onSubmit={onSubmit} className="mt-8 grid gap-4">
        <input
          required
          value={loginValue}
          onChange={(e) => setLoginValue(e.target.value)}
          className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500"
          placeholder="Телефон або email"
        />
        <input
          required
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500"
          placeholder="Пароль"
        />
        <button
          disabled={loading}
          className="flex items-center justify-center gap-2 rounded-xl bg-brand-600 px-6 py-3 font-medium text-white transition hover:bg-brand-700 disabled:opacity-60"
        >
          {loading && <Loader2 className="animate-spin" size={18} />}
          Увійти
        </button>
      </form>
      {error && <p className="mt-4 rounded-xl bg-red-50 p-4 text-sm text-red-700">{error}</p>}
      <p className="mt-4 text-center text-sm text-slate-500">
        Немає акаунта?{' '}
        <Link to="/register" className="font-medium text-brand-600 hover:underline">Зареєструватися</Link>
      </p>
    </section>
  )
}
