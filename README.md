# Arik's Finance Tracker

A full-stack finance tracking application built with .NET Core Web API and Angular.

## Features

- Dashboard with financial overview
- Income tracking
- Expense management
- Budget planning
- Financial reports
- Responsive design with Angular Material

## Tech Stack

**Backend:**
- .NET Core Web API
- Entity Framework Core
- SQLite database

**Frontend:**
- Angular
- Angular Material
- TypeScript
- SCSS

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js and npm
- Angular CLI

### Installation

1. Clone the repository
```bash
git clone https://github.com/arikmir/AriksFinanceTracker.git
cd AriksFinanceTracker
```

2. Run the backend API
```bash
cd AriksFinanceTracker.Api
dotnet restore
dotnet run
```

3. Run the frontend client
```bash
cd AriksFinanceTracker.Client
npm install
ng serve
```

4. Open your browser and navigate to `http://localhost:4200`

## Project Structure

```
AriksFinanceTracker/
├── AriksFinanceTracker.Api/          # .NET Core Web API
│   ├── Controllers/                  # API controllers
│   ├── Migrations/                   # EF migrations
│   └── Program.cs                    # API entry point
├── AriksFinanceTracker.Client/       # Angular frontend
│   ├── src/app/components/           # Angular components
│   ├── src/app/models/              # TypeScript models
│   └── src/app/services/            # Angular services
└── README.md
```

## API Endpoints

- `GET/POST /api/dashboard` - Dashboard data
- `GET/POST /api/income` - Income management
- `GET/POST /api/expenses` - Expense management

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request