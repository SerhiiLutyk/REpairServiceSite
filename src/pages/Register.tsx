import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Loader2 } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'

export default function Register() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({ fullName: '', phone: '', email: '', password: '' })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      await register(form.fullName, form.phone, form.password, form.email || undefined)
      navigate('/')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Помилка реєстрації')
    } finally {
      setLoading(false)
    }
  }

  return (
    <section className="mx-auto max-w-md px-4 py-16">
      <h1 className="text-center text-3xl font-bold text-slate-900">Реєстрація</h1>
      <form onSubmit={onSubmit} className="mt-8 grid gap-4">
        <input
          required
          value={form.fullName}
          onChange={(e) => setForm({ ...form, fullName: e.target.value })}
          className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500"
          placeholder="Ім’я та прізвище"
        />
        <input
          required
          value={form.phone}
          onChange={(e) => setForm({ ...form, phone: e.target.value })}
          className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500"
          placeholder="Телефон"
        />
        <input
          type="email"
          value={form.email}
          onChange={(e) => setForm({ ...form, email: e.target.value })}
          className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500"
          placeholder="Email (опційно)"
        />
        <input
          required
          type="password"
          value={form.password}
          onChange={(e) => setForm({ ...form, password: e.target.value })}
          className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500"
          placeholder="Пароль"
        />
        <button
          disabled={loading}
          className="flex items-center justify-center gap-2 rounded-xl bg-brand-600 px-6 py-3 font-medium text-white transition hover:bg-brand-700 disabled:opacity-60"
        >
          {loading && <Loader2 className="animate-spin" size={18} />}
          Зареєструватися
        </button>
      </form>
      {error && <p className="mt-4 rounded-xl bg-red-50 p-4 text-sm text-red-700">{error}</p>}
      <p className="mt-4 text-center text-sm text-slate-500">
        Вже маєте акаунт?{' '}
        <Link to="/login" className="font-medium text-brand-600 hover:underline">Увійти</Link>
      </p>
    </section>
  )
}
