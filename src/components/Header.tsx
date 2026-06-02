import { Link } from 'react-router-dom'
import { Wrench, LogOut } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'

export default function Header() {
  const { user, isAdmin, logout } = useAuth()

  return (
    <header className="sticky top-0 z-50 border-b border-slate-200 bg-white/80 backdrop-blur">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
        <Link to="/" className="flex items-center gap-2 font-bold text-slate-900">
          <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-600 text-white">
            <Wrench size={18} />
          </span>
          GadgetFix
        </Link>

        <nav className="hidden items-center gap-6 md:flex">
          <Link to="/calculator" className="text-sm text-slate-600 transition hover:text-brand-600">AI-калькулятор</Link>
          <Link to="/booking" className="text-sm text-slate-600 transition hover:text-brand-600">Запис на ремонт</Link>
          {isAdmin && (
            <Link to="/admin" className="text-sm text-slate-600 transition hover:text-brand-600">Адмінка</Link>
          )}
        </nav>

        <div className="flex items-center gap-3">
          {user ? (
            <>
              <Link to="/cabinet" className="hidden text-sm font-medium text-slate-600 transition hover:text-brand-600 sm:inline">
                {user.fullName}
              </Link>
              <button
                onClick={logout}
                className="flex items-center gap-1 rounded-xl border border-slate-300 px-3 py-2 text-sm font-medium text-slate-700 transition hover:border-brand-500 hover:text-brand-600"
              >
                <LogOut size={15} /> Вийти
              </button>
            </>
          ) : (
            <>
              <Link to="/login" className="text-sm font-medium text-slate-600 transition hover:text-brand-600">
                Вхід
              </Link>
              <Link
                to="/register"
                className="rounded-xl bg-brand-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-brand-700"
              >
                Реєстрація
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  )
}
