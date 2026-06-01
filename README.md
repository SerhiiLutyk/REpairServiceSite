# GadgetFix — сервісний центр з ремонту гаджетів

Веб-застосунок для сервісного центру: онлайн-запис на ремонт, AI-калькулятор
приблизної вартості, адмін-панель із Telegram-сповіщеннями про готовність.

## Архітектура

Мікросервісна архітектура на **.NET Aspire**:

```
Frontend (React)  ──►  API Gateway (YARP)  ──►  ┌─ Users         (auth)
                                                ├─ Catalog       (типи/послуги/ціни)
                                                ├─ Orders        (записи, статуси)
                                                ├─ AI            (оцінка вартості)
                                                └─ Notifications (Telegram)
```

- Кожен сервіс має шари **API / BLL / DAL**
- База даних — **PostgreSQL** (окрема БД на сервіс), EF Core + міграції
- Gateway маршрутизує запити через service discovery
- Orders викликає Notifications при статусі «Готово» → Telegram-пуш

## Стек

| Шар | Технології |
|-----|-----------|
| Frontend | React 19, TypeScript, Vite, TailwindCSS, React Router |
| Backend | ASP.NET Core, .NET Aspire, YARP |
| База даних | PostgreSQL, Entity Framework Core |
| Тести | xUnit |

## Запуск

### Frontend
```bash
npm install
npm run dev
```

### Backend (потрібен Docker Desktop)
```bash
cd backend
dotnet run --project GadgetFix.AppHost
```
Aspire-дашборд покаже порт API Gateway — впишіть його у `.env` (`VITE_API_URL`).

### Тести
```bash
cd backend
dotnet test
```
