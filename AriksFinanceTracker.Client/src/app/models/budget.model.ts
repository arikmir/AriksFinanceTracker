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
  categoryId: number;
  categoryName: string;
  limit: number;
  spent: number;
  remaining: number;
  percentageUsed: number;
  isEssential: boolean;
  status: string;
  statusColor: string;
  isCustom: boolean;
  icon?: string;
  dailyRecommendation?: number;
}

export interface BudgetLimit {
  categoryId: number;
  categoryName: string;
  monthlyLimit: number;
  isEssential: boolean;
  isCustom: boolean;
  icon?: string;
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
  categoryId: number;
  amount: number;
}

export interface SavingsCelebration {
  message: string;
  savingsRate: number;
  savingsAmount: number;
  achievements: string[];
  encouragement: string;
}

export interface CreateBudgetCategoryRequest {
  name: string;
  monthlyLimit: number;
  isEssential: boolean;
}

export interface UpdateBudgetLimitRequest {
  newLimit: number;
  isEssential?: boolean;
  name?: string;
}
