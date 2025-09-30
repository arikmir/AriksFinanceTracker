import { Component, OnInit } from '@angular/core';
import { FinanceService } from '../../services/finance.service';
import { BudgetStatus, FinancialHealth, SavingsCelebration } from '../../models/budget.model';
import { ExpenseAnalytics, CategorySummary } from '../../models/expense.model';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent implements OnInit {
  budgetStatus?: BudgetStatus;
  financialHealth?: FinancialHealth;
  weeklyAnalytics?: ExpenseAnalytics;
  monthlyAnalytics?: ExpenseAnalytics;
  categorySummary: CategorySummary[] = [];
  
  isLoading = true;
  errorMessage?: string;
  Math = Math;
  
  // Chart data
  savingsRateHistory: any[] = [];
  categorySpendingData: any[] = [];
  savingsGoalsData: any[] = [];
  monthlyTrendsData: any[] = [];

  constructor(private financeService: FinanceService) {}

  ngOnInit(): void {
    this.loadReportsData();
  }

  loadReportsData(): void {
    this.isLoading = true;
    this.errorMessage = undefined;

    forkJoin({
      budgetStatus: this.financeService.getBudgetStatus(),
      financialHealth: this.financeService.getFinancialHealth(),
      weeklyAnalytics: this.financeService.getWeeklyExpenseAnalytics(),
      monthlyAnalytics: this.financeService.getMonthlyExpenseAnalytics(),
      categorySummary: this.financeService.getCategorySummary()
    }).subscribe({
      next: (data) => {
        this.budgetStatus = data.budgetStatus;
        this.financialHealth = data.financialHealth;
        this.weeklyAnalytics = data.weeklyAnalytics;
        this.monthlyAnalytics = data.monthlyAnalytics;
        this.categorySummary = data.categorySummary;
        
        this.prepareChartData();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading reports data:', error);
        this.errorMessage = 'Failed to load reports data. Please try again.';
        this.isLoading = false;
      }
    });
  }

  prepareChartData(): void {
    if (!this.budgetStatus || !this.monthlyAnalytics) return;

    // Prepare savings rate history (simulated for demo)
    this.savingsRateHistory = [
      { month: 'Jan', rate: 18 },
      { month: 'Feb', rate: 20 },
      { month: 'Mar', rate: 22 },
      { month: 'Apr', rate: 19 },
      { month: 'May', rate: 21 },
      { month: 'Current', rate: this.budgetStatus.savingsRate }
    ];

    // Prepare category spending data
    this.categorySpendingData = this.budgetStatus.categoryBudgets.map(cb => ({
      name: cb.categoryName,
      spent: cb.spent,
      budget: cb.limit,
      percentage: cb.percentageUsed
    }));

    // Prepare savings goals data
    this.savingsGoalsData = this.budgetStatus.savingsProgress.map(sp => ({
      name: sp.name,
      target: sp.target,
      actual: sp.actual,
      progress: sp.progress,
      achieved: sp.isAchieved
    }));

    // Prepare monthly trends (simulated)
    this.monthlyTrendsData = [
      { month: 'Jan', income: 8000, expenses: 6400, savings: 1600 },
      { month: 'Feb', income: 8000, expenses: 6200, savings: 1800 },
      { month: 'Mar', income: 8000, expenses: 6100, savings: 1900 },
      { month: 'Apr', income: 8000, expenses: 6500, savings: 1500 },
      { month: 'May', income: 8000, expenses: 6300, savings: 1700 },
      { month: 'Current', income: this.budgetStatus.monthlyIncome, expenses: this.budgetStatus.totalSpent, savings: this.budgetStatus.actualSavings }
    ];
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount);
  }

  formatPercentage(value: number): string {
    return `${value.toFixed(1)}%`;
  }

  getHealthGradeColor(grade: string): string {
    switch (grade) {
      case 'Excellent': return 'success';
      case 'Good': return 'primary';
      case 'Fair': return 'accent';
      default: return 'warn';
    }
  }

  getSavingsRateColor(rate: number): string {
    if (rate >= 25) return '#4caf50';
    if (rate >= 20) return '#2196f3';
    if (rate >= 15) return '#ff9800';
    return '#f44336';
  }

  getCategoryColor(index: number): string {
    const colors = ['#2196f3', '#4caf50', '#ff9800', '#f44336', '#9c27b0', '#00bcd4', '#ffeb3b', '#795548'];
    return colors[index % colors.length];
  }

  refreshData(): void {
    this.loadReportsData();
  }

  exportReport(): void {
    // Generate a simple text report
    const reportData = {
      generatedAt: new Date().toISOString(),
      financialPeriod: this.budgetStatus?.currentPeriod,
      savingsRate: this.budgetStatus?.savingsRate,
      totalSavings: this.budgetStatus?.actualSavings,
      financialGrade: this.financialHealth?.grade,
      categoryBreakdown: this.categorySpendingData,
      savingsGoals: this.savingsGoalsData
    };
    
    const dataStr = JSON.stringify(reportData, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = `financial-report-${new Date().toISOString().split('T')[0]}.json`;
    link.click();
    
    URL.revokeObjectURL(url);
  }
}
