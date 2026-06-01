import { useEffect, useState } from 'react'
import { Loader2, RefreshCw } from 'lucide-react'
import { getOrders, updateOrderStatus, OrderStatusLabels, type Order } from '@/lib/api'

const statuses = [0, 1, 2, 3, 4, 5]

export default function Admin() {
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  async function load() {
    setError(null)
    try {
      setOrders(await getOrders())
    } catch {
      setError('Не вдалося завантажити замовлення. Перевірте, чи запущено бекенд.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function changeStatus(id: string, status: number) {
    const updated = await updateOrderStatus(id, status)
    setOrders((prev) => prev.map((o) => (o.id === id ? updated : o)))
  }

  return (
    <section className="mx-auto max-w-5xl px-4 py-12">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">Адмін-панель · Замовлення</h1>
        <button onClick={load} className="flex items-center gap-2 rounded-lg border border-slate-300 px-3 py-1.5 text-sm hover:border-brand-500">
          <RefreshCw size={15} /> Оновити
        </button>
      </div>

      {loading && <Loader2 className="mt-8 animate-spin text-brand-600" />}
      {error && <p className="mt-6 rounded-xl bg-red-50 p-4 text-sm text-red-700">{error}</p>}

      {!loading && !error && (
        <div className="mt-6 overflow-x-auto rounded-2xl border border-slate-200">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 text-slate-500">
              <tr>
                <th className="px-4 py-3">Клієнт</th>
                <th className="px-4 py-3">Телефон</th>
                <th className="px-4 py-3">Несправність</th>
                <th className="px-4 py-3">Статус</th>
              </tr>
            </thead>
            <tbody>
              {orders.length === 0 && (
                <tr><td colSpan={4} className="px-4 py-6 text-center text-slate-400">Замовлень поки немає</td></tr>
              )}
              {orders.map((o) => (
                <tr key={o.id} className="border-t border-slate-100">
                  <td className="px-4 py-3 font-medium text-slate-900">{o.customerName}</td>
                  <td className="px-4 py-3 text-slate-600">{o.phone}</td>
                  <td className="px-4 py-3 text-slate-600">{o.problemDescription}</td>
                  <td className="px-4 py-3">
                    <select
                      value={o.status}
                      onChange={(e) => changeStatus(o.id, Number(e.target.value))}
                      className="rounded-lg border border-slate-300 px-2 py-1 outline-none focus:border-brand-500"
                    >
                      {statuses.map((s) => (
                        <option key={s} value={s}>{OrderStatusLabels[s]}</option>
                      ))}
                    </select>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      <p className="mt-4 text-xs text-slate-400">
        Зміна статусу на «Готово» надсилає Telegram-повідомлення (якщо налаштовано бота).
      </p>
    </section>
  )
}
