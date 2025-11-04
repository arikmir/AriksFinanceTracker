export interface SpendingCategoryOption {
  id: number;
  name: string;
  icon?: string;
  isCustom?: boolean;
  isEssential?: boolean;
  isEssentialDefault?: boolean;
}

export interface Expense {
  id?: number;
  date: Date | string;
  amount: number;
  categoryId: number;
  categoryName?: string;
  category?: SpendingCategoryOption;
  description: string;
  paymentMethod?: string;
  location?: string;
  tags?: string;
  isRecurring: boolean;
  createdAt?: Date | string;
  updatedAt?: Date | string;
}

export interface ExpenseAnalytics {
  totalAmount: number;
  transactionCount: number;
  averageAmount: number;
  categoryBreakdown: ExpenseCategoryBreakdown[];
  startDate: Date | string;
  endDate: Date | string;
}

export interface ExpenseCategoryBreakdown {
  categoryId: number;
  categoryName: string;
  totalAmount: number;
  transactionCount: number;
}

export interface DailyExpense {
  date: Date | string;
  totalAmount: number;
  transactionCount: number;
  expenses: Expense[];
}

export interface CategorySummary {
  categoryId: number;
  categoryName: string;
  totalAmount: number;
  transactionCount: number;
  percentage: number;
}

export interface PaymentMethodSummary {
  paymentMethod: string;
  totalAmount: number;
  transactionCount: number;
  percentage: number;
}
