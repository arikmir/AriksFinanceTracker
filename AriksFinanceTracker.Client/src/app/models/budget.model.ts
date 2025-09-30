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
  Miscellaneous = 10,
  Mortgage = 11,
  Rent = 12
}

export enum SavingsGoalType {
  EmergencyFund = 0,
  Investments = 1,
  HouseFuture = 2
}

export enum AlertType {
  Info = 0,
  Warning = 1,
  Critical = 2,
  Exceeded = 3,
  Achievement = 4
}

export interface BudgetStatus {
  currentPeriod: string;
  periodDescription: string;
  monthlyIncome: number;
  totalBudgeted: number;
  totalSpent: number;
  remainingBudget: number;
  savingsTarget: number;
  actualSavings: number;
  savingsRate: number;
  daysLeftInMonth: number;
  categoryBudgets: CategoryBudget[];
  savingsProgress: SavingsProgress[];
  motivationalMessage: string;
}

export interface CategoryBudget {
  category: ExpenseCategory;
  categoryName: string;
  limit: number;
  spent: number;
  remaining: number;
  percentageUsed: number;
  dailyRecommendation: number;
  isEssential: boolean;
  status: string;
  statusColor: string;
}

export interface SavingsProgress {
  type: SavingsGoalType;
  name: string;
  target: number;
  actual: number;
  progress: number;
  isAchieved: boolean;
  motivationalMessage: string;
}

export interface FinancialHealth {
  grade: string;
  savingsRate: number;
  score: number;
  message: string;
  achievements: string[];
  recommendations: string[];
  isOnTrack: boolean;
}

export interface SpendingCheck {
  isAllowed: boolean;
  message: string;
  alertLevel?: AlertType;
  remainingBudget: number;
  newPercentageUsed: number;
  encouragement: string;
}

export interface CheckSpendingRequest {
  category: ExpenseCategory;
  amount: number;
}

export interface SavingsCelebration {
  message: string;
  savingsRate: number;
  savingsAmount: number;
  achievements: string[];
  encouragement: string;
}