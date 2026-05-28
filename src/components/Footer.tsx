export default function Footer() {
  return (
    <footer className="border-t border-slate-200 bg-white">
      <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-2 px-4 py-6 text-sm text-slate-500 sm:flex-row">
        <span>© {new Date().getFullYear()} GadgetFix — сервісний центр з ремонту гаджетів</span>
        <span>м. Львів · +380 00 000 0000</span>
      </div>
    </footer>
  )
}
