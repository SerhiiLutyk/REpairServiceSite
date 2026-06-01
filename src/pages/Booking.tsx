import { useEffect, useState } from 'react'
import { CheckCircle2, Loader2 } from 'lucide-react'
import { createOrder, getDeviceTypes, type DeviceType } from '@/lib/api'

export default function Booking() {
  const [deviceTypes, setDeviceTypes] = useState<DeviceType[]>([])
  const [form, setForm] = useState({ customerName: '', phone: '', deviceTypeId: 0, problemDescription: '' })
  const [loading, setLoading] = useState(false)
  const [done, setDone] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    getDeviceTypes()
      .then((d) => {
        setDeviceTypes(d)
        if (d.length) setForm((f) => ({ ...f, deviceTypeId: d[0].id }))
      })
      .catch(() => setError('Не вдалося завантажити типи гаджетів. Перевірте, чи запущено бекенд.'))
  }, [])

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      await createOrder(form)
      setDone(true)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Сталася помилка')
    } finally {
      setLoading(false)
    }
  }

  if (done) {
    return (
      <section className="mx-auto max-w-md px-4 py-20 text-center">
        <CheckCircle2 className="mx-auto text-green-500" size={56} />
        <h1 className="mt-4 text-2xl font-bold text-slate-900">Заявку прийнято!</h1>
        <p className="mt-2 text-slate-600">Ми зв’яжемося з вами найближчим часом для підтвердження.</p>
      </section>
    )
  }

  return (
    <section className="mx-auto max-w-2xl px-4 py-16">
      <h1 className="text-center text-3xl font-bold text-slate-900">Запис на ремонт</h1>
      <p className="mt-2 text-center text-slate-600">Залиште заявку — і майстер зв’яжеться з вами.</p>

      <form onSubmit={onSubmit} className="mt-8 grid gap-4">
        <input
          required
          value={form.customerName}
          onChange={(e) => setForm({ ...form, customerName: e.target.value })}
          className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500"
          placeholder="Ваше ім’я"
        />
        <input
          required
          value={form.phone}
          onChange={(e) => setForm({ ...form, phone: e.target.value })}
          className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500"
          placeholder="Телефон"
        />
        <label className="grid gap-1 text-sm font-medium text-slate-700">
          Тип гаджета
          <select
            value={form.deviceTypeId}
            onChange={(e) => setForm({ ...form, deviceTypeId: Number(e.target.value) })}
            className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500"
          >
            {deviceTypes.map((d) => (
              <option key={d.id} value={d.id}>{d.name}</option>
            ))}
          </select>
        </label>
        <textarea
          required
          rows={3}
          value={form.problemDescription}
          onChange={(e) => setForm({ ...form, problemDescription: e.target.value })}
          className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500"
          placeholder="Опишіть несправність"
        />
        <button
          disabled={loading}
          className="flex items-center justify-center gap-2 rounded-xl bg-brand-600 px-6 py-3 font-medium text-white transition hover:bg-brand-700 disabled:opacity-60"
        >
          {loading && <Loader2 className="animate-spin" size={18} />}
          Надіслати заявку
        </button>
      </form>

      {error && <p className="mt-4 rounded-xl bg-red-50 p-4 text-sm text-red-700">{error}</p>}
    </section>
  )
}
