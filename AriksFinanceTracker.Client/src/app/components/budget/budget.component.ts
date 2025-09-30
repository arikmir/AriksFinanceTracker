import { Component, OnInit } from '@angular/core';
import { FinanceService } from '../../services/finance.service';
import { BudgetStatus, FinancialHealth, SavingsCelebration, CategoryBudget, SavingsProgress } from '../../models/budget.model';
import { Observable, forkJoin } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

@Component({
  selector: 'app-budget',
  templateUrl: './budget.component.html',
  styleUrls: ['./budget.component.scss']
})
export class BudgetComponent implements OnInit {
  budgetStatus?: BudgetStatus;
  financialHealth?: FinancialHealth;
  savingsCelebration?: SavingsCelebration;
  isLoading = true;
  errorMessage?: string;

  constructor(private financeService: FinanceService) {}

  ngOnInit(): void {
    this.loadBudgetData();
  }

  loadBudgetData(): void {
    this.isLoading = true;
    this.errorMessage = undefined;

    // First ensure budget is initialized
    this.financeService.initializeBudget().subscribe({
      next: () => {
        // Then load all budget data
        forkJoin({
          budgetStatus: this.financeService.getBudgetStatus(),
          financialHealth: this.financeService.getFinancialHealth(),
          savingsCelebration: this.financeService.getSavingsCelebration()
        }).subscribe({
          next: (data) => {
            this.budgetStatus = data.budgetStatus;
            this.financialHealth = data.financialHealth;
            this.savingsCelebration = data.savingsCelebration;
            this.isLoading = false;
          },
          error: (error) => {
            console.error('Error loading budget data:', error);
            this.errorMessage = 'Failed to load budget data. Please try again.';
            this.isLoading = false;
          }
        });
      },
      error: (error) => {
        console.error('Error initializing budget:', error);
        this.errorMessage = 'Failed to initialize budget system.';
        this.isLoading = false;
      }
    });
  }

  refreshData(): void {
    this.loadBudgetData();
  }

  getCategoryIcon(categoryName: string): string {
    const iconMap: { [key: string]: string } = {
      'Mortgage': 'home',
      'Rent': 'apartment',
      'Groceries': 'shopping_cart',
      'Transport': 'directions_car',
      'Utilities': 'electrical_services',
      'Food & Drinks': 'restaurant',
      'Entertainment': 'movie',
      'Shopping': 'shopping_bag',
      'Miscellaneous': 'category',
      'Health & Fitness': 'fitness_center'
    };
    return iconMap[categoryName] || 'account_balance_wallet';
  }

  getSavingsIcon(goalName: string): string {
    if (goalName.includes('Emergency')) return 'security';
    if (goalName.includes('Investment')) return 'trending_up';
    if (goalName.includes('House')) return 'home_work';
    return 'savings';
  }

  getHealthGradeColor(grade: string): string {
    switch (grade) {
      case 'Excellent': return 'success';
      case 'Good': return 'primary';
      case 'Fair': return 'accent';
      default: return 'warn';
    }
  }

  getProgressBarColor(percentage: number): string {
    if (percentage >= 100) return 'success';
    if (percentage >= 75) return 'primary';
    if (percentage >= 50) return 'accent';
    return 'warn';
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
}
