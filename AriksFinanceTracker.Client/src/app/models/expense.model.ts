export enum ExpenseCategory {
  FoodAndDrinks = 0,
  Groceries = 1,
  Shopping = 2,
  Transport = 3,
  Entertainment = 4,
  Utilities = 5,
  HealthAndFitness = 6,
  Home = 7,
  Savings = 8,
  Repayment = 9,
  Miscellaneous = 10
}

export interface Expense {
  id?: number;
  date: Date;
  amount: number;
  category: ExpenseCategory;
  description: string;
  paymentMethod?: string;
  location?: string;
  tags?: string;
  isRecurring: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface ExpenseAnalytics {
  totalAmount: number;
  transactionCount: number;
  averageAmount: number;
  categoryBreakdown: { [key in ExpenseCategory]?: number };
  startDate: Date;
  endDate: Date;
}

export interface DailyExpense {
  date: Date;
  totalAmount: number;
  transactionCount: number;
  expenses: Expense[];
}

export interface CategorySummary {
  category: ExpenseCategory;
  categoryName: string;
  totalAmount: number;
  transactionCount: number;
  percentage: number;
}

export const ExpenseCategoryLabels = {
  [ExpenseCategory.FoodAndDrinks]: 'Food & Drinks',
  [ExpenseCategory.Groceries]: 'Groceries',
  [ExpenseCategory.Shopping]: 'Shopping',
  [ExpenseCategory.Transport]: 'Transport',
  [ExpenseCategory.Entertainment]: 'Entertainment',
  [ExpenseCategory.Utilities]: 'Utilities',
  [ExpenseCategory.HealthAndFitness]: 'Health & Fitness',
  [ExpenseCategory.Home]: 'Home',
  [ExpenseCategory.Savings]: 'Savings',
  [ExpenseCategory.Repayment]: 'Repayment',
  [ExpenseCategory.Miscellaneous]: 'Miscellaneous'
};

export const ExpenseCategoryIcons = {
  [ExpenseCategory.FoodAndDrinks]: 'restaurant',
  [ExpenseCategory.Groceries]: 'shopping_cart',
  [ExpenseCategory.Shopping]: 'shopping_bag',
  [ExpenseCategory.Transport]: 'directions_car',
  [ExpenseCategory.Entertainment]: 'movie',
  [ExpenseCategory.Utilities]: 'bolt',
  [ExpenseCategory.HealthAndFitness]: 'fitness_center',
  [ExpenseCategory.Home]: 'home',
  [ExpenseCategory.Savings]: 'savings',
  [ExpenseCategory.Repayment]: 'payment',
  [ExpenseCategory.Miscellaneous]: 'category'
};
