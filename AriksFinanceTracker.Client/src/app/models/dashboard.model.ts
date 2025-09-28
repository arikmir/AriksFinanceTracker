export interface DashboardData {
  totalIncome: number;
  totalExpenses: number;
  netSavings: number;
  savingsRate: number;
  expensesByCategory: ExpenseByCategory[];
}

export interface ExpenseByCategory {
  category: string;
  amount: number;
}
