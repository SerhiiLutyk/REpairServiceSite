import { Link } from 'react-router-dom'
import { Wrench, Phone, MapPin } from 'lucide-react'

export default function Footer() {
  return (
    <footer className="mt-8 border-t border-slate-200 bg-white">
      <div className="mx-auto grid max-w-6xl gap-8 px-4 py-12 sm:grid-cols-3">
        <div>
          <Link to="/" className="flex items-center gap-2 font-bold text-slate-900">
            <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-600 text-white">
              <Wrench size={18} />
            </span>
            GadgetFix
          </Link>
          <p className="mt-3 max-w-xs text-sm text-slate-500">
            Сервісний центр з ремонту смартфонів, ноутбуків, планшетів та іншої електроніки.
          </p>
        </div>

        <div>
          <p className="text-sm font-semibold text-slate-900">Сервіс</p>
          <ul className="mt-3 space-y-2 text-sm text-slate-500">
            <li><Link to="/calculator" className="transition hover:text-brand-600">AI-калькулятор</Link></li>
            <li><Link to="/booking" className="transition hover:text-brand-600">Запис на ремонт</Link></li>
          </ul>
        </div>

        <div>
          <p className="text-sm font-semibold text-slate-900">Контакти</p>
          <ul className="mt-3 space-y-2 text-sm text-slate-500">
            <li className="flex items-center gap-2"><Phone size={15} /> +380 00 000 0000</li>
            <li className="flex items-center gap-2"><MapPin size={15} /> м. Чернівці</li>
          </ul>
        </div>
      </div>
      <div className="border-t border-slate-100 py-4 text-center text-xs text-slate-400">
        © {new Date().getFullYear()} GadgetFix. Усі права захищено.
      </div>
    </footer>
  )
}
