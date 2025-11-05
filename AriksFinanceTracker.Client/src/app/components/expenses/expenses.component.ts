import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, forkJoin, Subscription } from 'rxjs';
import { FinanceService } from '../../services/finance.service';
import { DataRefreshService } from '../../services/data-refresh.service';
import { MonthSelectionService } from '../../services/month-selection.service';
import {
  Expense,
  ExpenseAnalytics,
  DailyExpense,
  CategorySummary,
  PaymentMethodSummary,
  SpendingCategoryOption
} from '../../models/expense.model';
import { BudgetStatus, SpendingCheck, CheckSpendingRequest } from '../../models/budget.model';

@Component({
  selector: 'app-expenses',
  templateUrl: './expenses.component.html',
  styleUrls: ['./expenses.component.scss']
})
export class ExpensesComponent implements OnInit, OnDestroy {
  expenses: Expense[] = [];
  weeklyAnalytics!: ExpenseAnalytics;
  monthlyAnalytics!: ExpenseAnalytics;
  categorySummary: CategorySummary[] = [];
  
  expenseForm: FormGroup;
  showAddForm = false;
  editingExpense: Expense | null = null;
  
  // Budget guidance properties
  budgetStatus?: BudgetStatus;
  spendingCheck?: SpendingCheck;
  showBudgetGuidance = false;
  
  private subscriptions: Subscription = new Subscription();
  
  Math = Math;
  
  // Date navigation
  selectedDate: Date = new Date();
  monthNames = ['January', 'February', 'March', 'April', 'May', 'June', 
                 'July', 'August', 'September', 'October', 'November', 'December'];
  
  categoryOptions: SpendingCategoryOption[] = [];
  paymentMethodSummary: PaymentMethodSummary[] = [];
  private readonly defaultPaymentMethods: string[] = ['Cash', 'Credit Card', 'Debit Card', 'Bank Transfer', 'Digital Wallet'];
  paymentMethods: string[] = [...this.defaultPaymentMethods];
  
  selectedPeriod = 'week';
  filterCategory: number | 'all' = 'all';
  filterPaymentMethod: string | 'all' = 'all';
  startDate: Date | null = null;
  endDate: Date | null = null;

  constructor(
    private fb: FormBuilder,
    private financeService: FinanceService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private dataRefreshService: DataRefreshService,
    private monthSelectionService: MonthSelectionService
  ) {
    this.expenseForm = this.fb.group({
      amount: ['', [Validators.required, Validators.min(0.01)]],
      categoryId: [null, Validators.required],
      description: ['', [Validators.required, Validators.minLength(3)]],
      paymentMethod: [''],
      location: [''],
      tags: [''],
      isRecurring: [false],
      date: [new Date(), Validators.required]
    });
  }

  ngOnInit(): void {
    // Initialize with current month from service
    this.selectedDate = this.monthSelectionService.getCurrentSelectedDate();
    this.loadData();
    this.loadBudgetStatus();
    this.setupBudgetGuidance();
    
    // Subscribe to month selection changes
    this.subscriptions.add(
      this.monthSelectionService.selectedDate$.subscribe(date => {
        this.selectedDate = date;
        this.loadData();
      })
    );
    
    // Subscribe to data refresh events
    this.subscriptions.add(
      this.dataRefreshService.expenseRefresh$.subscribe(() => {
        this.loadData();
        this.loadBudgetStatus();
      })
    );
    
    this.subscriptions.add(
      this.dataRefreshService.incomeRefresh$.subscribe(() => {
        this.loadBudgetStatus(); // Income changes affect budget status
      })
    );
  }
  
  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadData(): void {
    const month = this.selectedDate.getMonth() + 1; // JavaScript months are 0-based
    const year = this.selectedDate.getFullYear();
    
    forkJoin({
      expenses: this.financeService.getExpenses(month, year),
      weeklyAnalytics: this.financeService.getWeeklyExpenseAnalytics(this.selectedDate),
      monthlyAnalytics: this.financeService.getMonthlyExpenseAnalytics(this.selectedDate),
      categorySummary: this.financeService.getCategorySummary(month, year),
      paymentMethodSummary: this.financeService.getPaymentMethodSummary(month, year)
    }).subscribe({
      next: (data) => {
        this.expenses = data.expenses.map(expense => ({
          ...expense,
          categoryName: expense.category?.name ?? expense.categoryName
        }));
        this.weeklyAnalytics = data.weeklyAnalytics;
        this.monthlyAnalytics = data.monthlyAnalytics;
        this.categorySummary = data.categorySummary;
        this.paymentMethodSummary = data.paymentMethodSummary;
        this.updatePaymentMethodOptions(data.expenses, data.paymentMethodSummary);
      },
      error: (error) => {
        console.error('Error loading data:', error);
        this.snackBar.open('Error loading expense data', 'Close', { duration: 3000 });
      }
    });
  }

  loadCategories(): void {
    this.financeService.getSpendingCategories().subscribe({
      next: (categories) => {
        this.categoryOptions = categories.map(category => ({
          id: category.id,
          name: category.name,
          icon: category.icon,
          isCustom: category.isCustom,
          isEssential: category.isEssentialDefault,
          isEssentialDefault: category.isEssentialDefault
        }));

        if (!this.expenseForm.get('categoryId')?.value && this.categoryOptions.length > 0) {
          this.expenseForm.patchValue({ categoryId: this.categoryOptions[0].id });
        }
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.categoryOptions = [];
      }
    });
  }

  private updatePaymentMethodOptions(expenses: Expense[], paymentSummary: PaymentMethodSummary[]): void {
    const methods = new Set<string>(this.defaultPaymentMethods);

    paymentSummary.forEach(item => methods.add(this.normalizePaymentMethod(item.paymentMethod)));
    expenses.forEach(expense => methods.add(this.normalizePaymentMethod(expense.paymentMethod)));

    this.paymentMethods = Array.from(methods);

    if (this.filterPaymentMethod !== 'all' && !methods.has(this.filterPaymentMethod)) {
      this.filterPaymentMethod = 'all';
    }
  }

  private normalizePaymentMethod(method?: string | null): string {
    const trimmed = method?.trim();
    return trimmed && trimmed.length > 0 ? trimmed : 'Unspecified';
  }

  formatPaymentMethod(method: string): string {
    return this.normalizePaymentMethod(method);
  }

  getPaymentMethodIcon(method: string): string {
    const normalized = this.normalizePaymentMethod(method);
    const iconMap: Record<string, string> = {
      'Cash': 'attach_money',
      'Credit Card': 'credit_card',
      'Debit Card': 'atm',
      'Bank Transfer': 'account_balance',
      'Digital Wallet': 'phone_iphone',
      'Check': 'description',
      'Unspecified': 'help_outline'
    };

    return iconMap[normalized] ?? 'account_balance_wallet';
  }

  onSubmit(): void {
    if (this.expenseForm.valid) {
      const formValue = this.expenseForm.value;
      const amount = Number(formValue.amount);
      const categoryId = Number(formValue.categoryId);
      const expense: Expense = {
        ...formValue,
        amount,
        categoryId,
        createdAt: new Date(),
        updatedAt: this.editingExpense ? new Date() : undefined
      };

      if (this.editingExpense) {
        expense.id = this.editingExpense.id;
        this.financeService.updateExpense(this.editingExpense.id!, expense).subscribe({
          next: () => {
            this.snackBar.open('Expense updated successfully', 'Close', { duration: 3000 });
            this.resetForm();
            this.loadData();
            this.dataRefreshService.triggerExpenseRefresh();
          },
          error: (error) => {
            console.error('Error updating expense:', error);
            this.snackBar.open('Error updating expense', 'Close', { duration: 3000 });
          }
        });
      } else {
        this.financeService.createExpense(expense).subscribe({
          next: () => {
            this.snackBar.open('Expense added successfully', 'Close', { duration: 3000 });
            this.resetForm();
            this.loadData();
            this.dataRefreshService.triggerExpenseRefresh();
          },
          error: (error) => {
            console.error('Error creating expense:', error);
            this.snackBar.open('Error adding expense', 'Close', { duration: 3000 });
          }
        });
      }
    }
  }

  editExpense(expense: Expense): void {
    this.editingExpense = expense;
    this.expenseForm.patchValue({
      amount: expense.amount,
      categoryId: expense.categoryId,
      description: expense.description,
      paymentMethod: expense.paymentMethod,
      location: expense.location,
      tags: expense.tags,
      isRecurring: expense.isRecurring,
      date: expense.date
    });
    this.showAddForm = true;
  }

  deleteExpense(expense: Expense): void {
    if (confirm('Are you sure you want to delete this expense?')) {
      this.financeService.deleteExpense(expense.id!).subscribe({
        next: () => {
          this.snackBar.open('Expense deleted successfully', 'Close', { duration: 3000 });
          this.loadData();
          this.dataRefreshService.triggerExpenseRefresh();
        },
        error: (error) => {
          console.error('Error deleting expense:', error);
          this.snackBar.open('Error deleting expense', 'Close', { duration: 3000 });
        }
      });
    }
  }

  resetForm(): void {
    this.expenseForm.reset({
      categoryId: this.categoryOptions.length ? this.categoryOptions[0].id : null,
      isRecurring: false,
      date: new Date()
    });
    this.showAddForm = false;
    this.editingExpense = null;
  }

  getCategoryIconById(categoryId: number): string {
    const category = this.categoryOptions.find(option => option.id === categoryId);
    if (!category) {
      return 'category';
    }
    if (category.icon && category.icon.trim().length > 0) {
      return category.icon;
    }
    return this.getFallbackIcon(category.name);
  }

  getCategoryNameById(categoryId: number): string {
    return this.categoryOptions.find(option => option.id === categoryId)?.name ?? 'Category';
  }

  private getFallbackIcon(categoryName: string): string {
    const iconMap: Record<string, string> = {
      'Mortgage': 'home',
      'Rent': 'apartment',
      'Groceries': 'shopping_cart',
      'Transport': 'directions_car',
      'Utilities': 'power',
      'Food & Drinks': 'restaurant',
      'Entertainment': 'movie',
      'Health & Fitness': 'fitness_center',
      'Home': 'home_repair_service',
      'Savings': 'savings',
      'Shopping': 'shopping_bag',
      'Repayment': 'payment',
      'Miscellaneous': 'category'
    };

    return iconMap[categoryName] ?? 'category';
  }

  getFilteredExpenses(): Expense[] {
    let filtered = this.expenses;
    
    if (this.filterCategory !== 'all') {
      filtered = filtered.filter(e => e.categoryId === this.filterCategory);
    }

    if (this.filterPaymentMethod !== 'all') {
      filtered = filtered.filter(e => this.normalizePaymentMethod(e.paymentMethod) === this.filterPaymentMethod);
    }
    
    if (this.startDate && this.endDate) {
      filtered = filtered.filter(e => {
        const expenseDate = new Date(e.date);
        return expenseDate >= this.startDate! && expenseDate <= this.endDate!;
      });
    }
    
    return filtered.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
  }

  getTodayTotal(): number {
    const today = new Date().toDateString();
    return this.expenses
      .filter(e => new Date(e.date).toDateString() === today)
      .reduce((sum, e) => sum + e.amount, 0);
  }

  getTodayTransactionCount(): number {
    const today = new Date().toDateString();
    return this.expenses
      .filter(e => new Date(e.date).toDateString() === today)
      .length;
  }

  // Budget guidance methods
  loadBudgetStatus(): void {
    this.financeService.getBudgetStatus().subscribe({
      next: (status) => {
        this.budgetStatus = status;
        this.loadCategories();
      },
      error: (error) => {
        console.error('Error loading budget status:', error);
      }
    });
  }

  setupBudgetGuidance(): void {
    // Watch for changes in amount and category to provide real-time guidance
    this.expenseForm.get('amount')?.valueChanges.subscribe(() => {
      this.updateSpendingGuidance();
    });
    
    this.expenseForm.get('categoryId')?.valueChanges.subscribe(() => {
      this.updateSpendingGuidance();
    });
  }

  updateSpendingGuidance(): void {
    const amountValue = this.expenseForm.get('amount')?.value;
    const amount = Number(amountValue);
    const categoryId = this.expenseForm.get('categoryId')?.value;

    if (amount > 0 && categoryId !== null && categoryId !== undefined) {
      const request: CheckSpendingRequest = {
        categoryId,
        amount
      };
      
      this.financeService.checkSpending(request).subscribe({
        next: (check) => {
          this.spendingCheck = check;
          this.showBudgetGuidance = true;
        },
        error: (error) => {
          console.error('Error checking spending:', error);
          this.showBudgetGuidance = false;
        }
      });
    } else {
      this.showBudgetGuidance = false;
    }
  }

  getCategoryBudgetInfo(categoryId: number) {
    if (!this.budgetStatus) return null;
    return this.budgetStatus.categoryBudgets.find(cb => cb.categoryId === categoryId);
  }

  getAlertLevelColor(alertLevel?: number): string {
    if (!alertLevel) return 'primary';
    switch (alertLevel) {
      case 1: return 'primary'; // Info
      case 2: return 'accent';  // Warning
      case 3: return 'warn';    // Critical
      case 4: return 'warn';    // Exceeded
      default: return 'primary';
    }
  }

  getAlertLevelIcon(alertLevel?: number): string {
    if (!alertLevel) return 'info';
    switch (alertLevel) {
      case 1: return 'info';
      case 2: return 'warning';
      case 3: return 'error';
      case 4: return 'block';
      default: return 'info';
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

  // Month navigation methods
  get currentMonthYear(): string {
    return `${this.monthNames[this.selectedDate.getMonth()]} ${this.selectedDate.getFullYear()}`;
  }

  previousMonth(): void {
    const newDate = new Date(this.selectedDate);
    newDate.setMonth(newDate.getMonth() - 1);
    this.selectedDate = newDate;
    this.monthSelectionService.setSelectedDate(newDate);
    this.loadData();
  }

  nextMonth(): void {
    const newDate = new Date(this.selectedDate);
    newDate.setMonth(newDate.getMonth() + 1);
    this.selectedDate = newDate;
    this.monthSelectionService.setSelectedDate(newDate);
    this.loadData();
  }

  openMonthPicker(): void {
    // For now, we'll use the month navigation buttons
    // In a future update, we can add a proper month picker dialog
  }

  isCurrentMonth(): boolean {
    const now = new Date();
    return this.selectedDate.getMonth() === now.getMonth() && 
           this.selectedDate.getFullYear() === now.getFullYear();
  }

  goToCurrentMonth(): void {
    const currentDate = new Date();
    this.selectedDate = currentDate;
    this.monthSelectionService.setSelectedDate(currentDate);
    this.loadData();
  }
}
