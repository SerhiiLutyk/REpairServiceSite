import { useEffect, useState } from 'react'
import { Loader2, Check, Send } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'
import { getMyOrders, generateTelegramCode, OrderStatusLabels, type Order } from '@/lib/api'

const statusColor: Record<number, string> = {
  0: 'bg-slate-100 text-slate-600',
  1: 'bg-amber-100 text-amber-700',
  2: 'bg-blue-100 text-blue-700',
  3: 'bg-green-100 text-green-700',
  4: 'bg-emerald-100 text-emerald-700',
  5: 'bg-red-100 text-red-700',
}

export default function Cabinet() {
  const { user, updateProfile } = useAuth()
  const [form, setForm] = useState({ fullName: '', email: '', telegramChatId: '' })
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [orders, setOrders] = useState<Order[]>([])
  const [ordersLoading, setOrdersLoading] = useState(true)
  const [tgCode, setTgCode] = useState<string | null>(null)

  useEffect(() => {
    if (user) setForm({ fullName: user.fullName, email: user.email ?? '', telegramChatId: user.telegramChatId ?? '' })
  }, [user])

  useEffect(() => {
    getMyOrders()
      .then(setOrders)
      .catch(() => setError('Не вдалося завантажити замовлення.'))
      .finally(() => setOrdersLoading(false))
  }, [])

  async function onSave(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setSaved(false)
    setSaving(true)
    try {
      await updateProfile({
        fullName: form.fullName,
        email: form.email || undefined,
        telegramChatId: form.telegramChatId || undefined,
      })
      setSaved(true)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Помилка збереження')
    } finally {
      setSaving(false)
    }
  }

  return (
    <section className="mx-auto max-w-4xl px-4 py-12">
      <h1 className="text-3xl font-bold text-slate-900">Особистий кабінет</h1>

      <div className="mt-8 grid gap-8 lg:grid-cols-2">
        {/* Профіль */}
        <div className="card p-6">
          <h2 className="text-lg font-semibold text-slate-900">Мої дані</h2>
          <form onSubmit={onSave} className="mt-4 grid gap-3">
            <label className="grid gap-1 text-sm font-medium text-slate-700">
              Ім’я
              <input
                value={form.fullName}
                onChange={(e) => setForm({ ...form, fullName: e.target.value })}
                className="rounded-xl border border-slate-300 px-4 py-2.5 outline-none focus:border-brand-500"
              />
            </label>
            <label className="grid gap-1 text-sm font-medium text-slate-700">
              Email
              <input
                type="email"
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
                className="rounded-xl border border-slate-300 px-4 py-2.5 outline-none focus:border-brand-500"
              />
            </label>
            <label className="grid gap-1 text-sm font-medium text-slate-700">
              Телефон
              <input
                value={user?.phone ?? ''}
                disabled
                className="rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-slate-400"
              />
            </label>

            <div className="rounded-xl border border-brand-100 bg-brand-50 p-4">
              <p className="flex items-center gap-2 text-sm font-medium text-brand-700">
                <Send size={15} /> Telegram-сповіщення
              </p>
              <p className="mt-1 text-xs text-slate-500">
                Вкажіть ваш Telegram Chat ID — і бот надішле повідомлення, коли замовлення буде готове.
                Дізнатися ID: напишіть боту <span className="font-medium">@userinfobot</span>.
              </p>
              <input
                value={form.telegramChatId}
                onChange={(e) => setForm({ ...form, telegramChatId: e.target.value })}
                placeholder="напр. 123456789"
                className="mt-2 w-full rounded-xl border border-slate-300 px-4 py-2.5 outline-none focus:border-brand-500"
              />

              <div className="mt-3 border-t border-brand-100 pt-3">
                <p className="text-xs text-slate-500">або прив'яжіть через бота автоматично:</p>
                <button
                  type="button"
                  onClick={async () => {
                    try {
                      const { code } = await generateTelegramCode()
                      setTgCode(code)
                    } catch {
                      setError('Не вдалося згенерувати код')
                    }
                  }}
                  className="mt-2 rounded-lg border border-brand-300 px-3 py-1.5 text-sm font-medium text-brand-700 transition hover:bg-brand-100"
                >
                  Прив'язати Telegram
                </button>
                {tgCode && (
                  <p className="mt-2 text-sm text-slate-600">
                    Надішліть боту: <code className="rounded bg-white px-2 py-0.5 font-mono text-brand-700">/link {tgCode}</code>
                  </p>
                )}
              </div>
            </div>

            <button
              disabled={saving}
              className="mt-1 flex items-center justify-center gap-2 rounded-xl bg-brand-600 px-6 py-3 font-medium text-white transition hover:bg-brand-700 disabled:opacity-60"
            >
              {saving ? <Loader2 className="animate-spin" size={18} /> : saved ? <Check size={18} /> : null}
              {saved ? 'Збережено' : 'Зберегти'}
            </button>
            {error && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{error}</p>}
          </form>
        </div>

        {/* Мої замовлення */}
        <div className="card p-6">
          <h2 className="text-lg font-semibold text-slate-900">Мої замовлення</h2>
          {ordersLoading ? (
            <Loader2 className="mt-4 animate-spin text-brand-600" />
          ) : orders.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">У вас поки немає замовлень.</p>
          ) : (
            <ul className="mt-4 space-y-3">
              {orders.map((o) => (
                <li key={o.id} className="rounded-xl border border-slate-200 p-4">
                  <div className="flex items-center justify-between">
                    <p className="font-medium text-slate-900">{o.problemDescription}</p>
                    <span className={`rounded-full px-2.5 py-1 text-xs font-medium ${statusColor[o.status]}`}>
                      {OrderStatusLabels[o.status]}
                    </span>
                  </div>
                  <p className="mt-1 text-xs text-slate-400">
                    {new Date(o.createdAt).toLocaleDateString('uk-UA')}
                    {o.estimatedPrice ? ` · ~${o.estimatedPrice} грн` : ''}
                  </p>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </section>
  )
}
