import { Component, OnInit, OnDestroy, TemplateRef, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FinanceService } from '../../services/finance.service';
import { DataRefreshService } from '../../services/data-refresh.service';
import { BudgetStatus, FinancialHealth, SavingsCelebration, CategoryBudget } from '../../models/budget.model';
import { forkJoin, Subscription } from 'rxjs';

@Component({
  selector: 'app-budget',
  templateUrl: './budget.component.html',
  styleUrls: ['./budget.component.scss']
})
export class BudgetComponent implements OnInit, OnDestroy {
  @ViewChild('budgetEditDialog') budgetEditDialog!: TemplateRef<any>;
  @ViewChild('healthDialog') healthDialog!: TemplateRef<any>;
  @ViewChild('categoryCreateDialog') categoryCreateDialog!: TemplateRef<any>;

  budgetStatus?: BudgetStatus;
  financialHealth?: FinancialHealth;
  isLoading = true;
  errorMessage?: string;
  
  // Achievement tracking
  achievements: string[] = [];
  achievementMessage = '';
  showAchievements = true;
  
  // Budget editing
  editingCategory?: CategoryBudget;
  newBudgetLimit = 0;
  newCategoryName = '';
  newCategoryLimit = 0;
  newCategoryIsEssential = false;
  isSavingNewCategory = false;
  
  Math = Math; // Make Math available in template
  
  private subscriptions: Subscription = new Subscription();

  constructor(
    private financeService: FinanceService,
    private dataRefreshService: DataRefreshService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadBudgetData();
    
    // Subscribe to data refresh events
    this.subscriptions.add(
      this.dataRefreshService.expenseRefresh$.subscribe(() => {
        this.loadBudgetData();
      })
    );
    
    this.subscriptions.add(
      this.dataRefreshService.incomeRefresh$.subscribe(() => {
        this.loadBudgetData();
      })
    );
  }
  
  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
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
            this.updateAchievements(data.savingsCelebration);
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

  openCreateCategoryDialog(): void {
    this.newCategoryName = '';
    this.newCategoryLimit = 0;
    this.newCategoryIsEssential = false;
    this.dialog.open(this.categoryCreateDialog, {
      width: '400px',
      disableClose: true
    });
  }

  editBudgetLimit(category: CategoryBudget): void {
    this.editingCategory = category;
    this.newBudgetLimit = category.limit;
    this.dialog.open(this.budgetEditDialog, {
      width: '400px',
      disableClose: true
    });
  }

  saveBudgetLimit(): void {
    if (!this.editingCategory) return;

    this.financeService.updateCategoryLimit(
      this.editingCategory.categoryId,
      { newLimit: this.newBudgetLimit }
    ).subscribe({
      next: () => {
        this.snackBar.open('Budget limit updated successfully!', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top'
        });
        this.dialog.closeAll();
        this.loadBudgetData();
      },
      error: (error) => {
        console.error('Error updating budget limit:', error);
        this.snackBar.open('Failed to update budget limit', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  viewFinancialHealth(): void {
    this.dialog.open(this.healthDialog, {
      width: '500px',
      maxWidth: '90vw'
    });
  }

  saveNewCategory(): void {
    const trimmedName = this.newCategoryName.trim();

    if (!trimmedName || this.newCategoryLimit <= 0) {
      this.snackBar.open('Please provide a name and positive limit for the new category.', 'Close', {
        duration: 3000,
        panelClass: ['error-snackbar']
      });
      return;
    }

    this.isSavingNewCategory = true;

    this.financeService.createBudgetCategory({
      name: trimmedName,
      monthlyLimit: this.newCategoryLimit,
      isEssential: this.newCategoryIsEssential
    }).subscribe({
      next: () => {
        this.snackBar.open('Category created successfully!', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top'
        });
        this.dialog.closeAll();
        this.isSavingNewCategory = false;
        this.loadBudgetData();
      },
      error: (error) => {
        console.error('Error creating category:', error);
        this.snackBar.open('Failed to create category', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        this.isSavingNewCategory = false;
      }
    });
  }

  viewAchievements(): void {
    this.showAchievements = true;
  }

  dismissAchievements(): void {
    this.showAchievements = false;
  }

  updateAchievements(celebration: SavingsCelebration): void {
    if (celebration && celebration.achievements.length > 0) {
      this.achievements = celebration.achievements;
      this.achievementMessage = celebration.message;
    }
  }

  getMotivationalMessage(): string {
    if (!this.budgetStatus) return '';
    
    const rate = this.budgetStatus.savingsRate;
    if (rate >= 30) return "Incredible! You're a financial rockstar!";
    if (rate >= 24) return "Perfect! You've hit your target!";
    if (rate >= 20) return "Excellent savings rate!";
    if (rate >= 15) return "Great progress on your savings!";
    return "Keep going, you're doing well!";
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
      'Health & Fitness': 'fitness_center',
      'Home': 'home_work',
      'Savings': 'savings',
      'Repayment': 'payment'
    };
    return iconMap[categoryName] || 'account_balance_wallet';
  }

  getProgressColor(percentage: number): 'primary' | 'accent' | 'warn' {
    if (percentage < 70) return 'primary';
    if (percentage < 90) return 'accent';
    return 'warn';
  }

  getHealthGradeColor(grade: string): string {
    switch (grade) {
      case 'Excellent': return 'excellent';
      case 'Good': return 'good';
      case 'Fair': return 'fair';
      default: return 'improving';
    }
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
