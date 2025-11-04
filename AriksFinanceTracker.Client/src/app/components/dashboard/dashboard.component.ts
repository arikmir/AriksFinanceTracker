import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Subscription } from 'rxjs';
import { FinanceService, TotalSavings } from '../../services/finance.service';
import { DataRefreshService } from '../../services/data-refresh.service';
import { MonthSelectionService } from '../../services/month-selection.service';
import { DashboardData } from '../../models/dashboard.model';
import { Income } from '../../models/income.model';
import { Expense, ExpenseCategory, ExpenseCategoryLabels, ExpenseCategoryIcons } from '../../models/expense.model';
import { MonthPickerDialogComponent } from './month-picker-dialog.component';
import { DateUtils } from '../../utils/date.utils';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  dashboardData: DashboardData = {
    totalIncome: 8000,
    totalExpenses: 6464,
    netSavings: 1536,
    savingsRate: 19.2,
    expensesByCategory: []
  };

  budgetCategories: any[] = [];

  private subscriptions: Subscription = new Subscription();

  // Form states
  showExpenseForm: boolean = false;
  showIncomeForm: boolean = false;
  showTotalSavingsForm: boolean = false;
  expenseForm: FormGroup;
  incomeForm: FormGroup;
  totalSavingsForm: FormGroup;
  isSubmitting: boolean = false;
  isSubmittingSavings: boolean = false;
  
  // Total Savings
  selectedSavingsTab: number = 0;
  totalSavingsList: TotalSavings[] = [];
  totalSavingsAmount: number = 0;
  editingSavingsId: number | null = null;
  
  // Date navigation
  selectedDate: Date = DateUtils.getCurrentAustralianDate();
  monthNames = ['January', 'February', 'March', 'April', 'May', 'June', 
                 'July', 'August', 'September', 'October', 'November', 'December'];
  
  // Categories
  categories = Object.values(ExpenseCategory).filter(v => typeof v === 'number') as ExpenseCategory[];
  paymentMethods = ['Credit Card', 'Debit Card', 'Cash', 'Bank Transfer', 'Digital Wallet'];

  constructor(
    private router: Router,
    private fb: FormBuilder,
    private dialog: MatDialog,
    private financeService: FinanceService,
    private dataRefreshService: DataRefreshService,
    private monthSelectionService: MonthSelectionService
  ) {
    this.expenseForm = this.fb.group({
      amount: [null, [Validators.required, Validators.min(0.01)]],
      date: [DateUtils.getCurrentAustralianDate(), Validators.required],
      category: [ExpenseCategory.Miscellaneous, Validators.required],
      description: ['', [Validators.required, Validators.minLength(3)]],
      paymentMethod: ['Credit Card'],
      location: [''],
      tags: [''],
      isRecurring: [false]
    });

    this.incomeForm = this.fb.group({
      amount: [null, [Validators.required, Validators.min(0.01)]],
      date: [DateUtils.getCurrentAustralianDate(), Validators.required],
      source: ['Primary Income', Validators.required],
      notes: ['']
    });

    this.totalSavingsForm = this.fb.group({
      amount: [null, [Validators.required, Validators.min(0.01)]],
      date: [DateUtils.getCurrentAustralianDate(), Validators.required],
      category: ['Emergency Fund', Validators.required],
      description: ['', [Validators.required, Validators.minLength(3)]]
    });
  }

  ngOnInit(): void {
    // Initialize with current month from service
    this.selectedDate = this.monthSelectionService.getCurrentSelectedDate();
    this.loadDashboard();
    this.loadTotalSavings();
    this.loadBudgetCategories();
    
    this.subscriptions.add(
      this.dataRefreshService.incomeUpdated$.subscribe(() => {
        this.loadDashboard();
      })
    );
    
    this.subscriptions.add(
      this.dataRefreshService.expenseUpdated$.subscribe(() => {
        this.loadDashboard();
        this.loadTotalSavings();
        this.loadBudgetCategories();
      })
    );
    
    this.subscriptions.add(
      this.dataRefreshService.dashboardRefresh$.subscribe(() => {
        this.loadDashboard();
        this.loadTotalSavings();
        this.loadBudgetCategories();
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadDashboard(): void {
    const month = this.selectedDate.getMonth() + 1; // JavaScript months are 0-based
    const year = this.selectedDate.getFullYear();
    
    console.log('Dashboard loading data for:', { month, year, selectedDate: this.selectedDate });
    
    this.financeService.getDashboardData(month, year).subscribe({
      next: (data) => {
        console.log('Dashboard data received:', data);
        this.dashboardData = data;
      },
      error: (error) => {
        console.log('Using default data due to API error:', error);
      }
    });
  }

  loadBudgetCategories(): void {
    this.financeService.getBudgetStatus().subscribe({
      next: (budgetStatus) => {
        // Convert API budget data to dashboard format with icons
        this.budgetCategories = budgetStatus.categoryBudgets.map(category => ({
          name: category.categoryName,
          amount: category.limit,
          icon: this.getBudgetCategoryIcon(category.categoryName)
        }));
        console.log('Budget categories loaded:', this.budgetCategories);
      },
      error: (error) => {
        console.log('Error loading budget categories:', error);
        // Fallback to empty array
        this.budgetCategories = [];
      }
    });
  }

  getBudgetCategoryIcon(categoryName: string): string {
    const iconMap: { [key: string]: string } = {
      'Mortgage': 'home',
      'Rent': 'apartment', 
      'Groceries': 'shopping_cart',
      'Transport': 'directions_car',
      'Utilities': 'power',
      'Food & Drinks': 'restaurant',
      'Shopping': 'shopping_bag',
      'Entertainment': 'movie',
      'Health & Fitness': 'fitness_center',
      'Home': 'home_repair_service',
      'Miscellaneous': 'category',
      'Savings': 'savings',
      'Repayment': 'payment'
    };
    return iconMap[categoryName] || 'category';
  }

  getCategoryIcon(category: ExpenseCategory): string {
    return ExpenseCategoryIcons[category];
  }

  getSavingsColor(): string {
    if (this.dashboardData.savingsRate >= 20) return 'var(--kiwi-medium-green)';
    if (this.dashboardData.savingsRate >= 10) return 'var(--kiwi-warning)';
    return 'var(--kiwi-error)';
  }

  getIncomePercentage(): number {
    const monthlyTarget = 8000; // Arik's monthly income target
    return Math.min((this.dashboardData.totalIncome / monthlyTarget) * 100, 100);
  }

  getExpensePercentage(): number {
    const totalBudget = this.budgetCategories.reduce((sum, category) => sum + category.amount, 0);
    return this.dashboardData.totalExpenses > 0 ? Math.min((this.dashboardData.totalExpenses / totalBudget) * 100, 100) : 0;
  }

  getCategorySpentAmount(categoryName: string): number {
    // Map budget category names to expense category enum names (from backend)
    const categoryMapping: { [key: string]: string[] } = {
      'Mortgage': ['Mortgage'],
      'Rent': ['Rent'],
      'Groceries': ['Groceries'],
      'Transport': ['Transport'],
      'Utilities': ['Utilities'],
      'Subscriptions': ['Entertainment'], // Map subscriptions to Entertainment enum
      'Food & Drinks': ['FoodAndDrinks'], // Backend uses enum name without spaces
      'Shopping': ['Shopping'],
      'Entertainment': ['Entertainment'],
      'Health & Fitness': ['HealthAndFitness'], // Backend uses enum name without spaces
      'Home': ['Home'],
      'Miscellaneous': ['Miscellaneous']
    };

    const mappedCategories = categoryMapping[categoryName] || [categoryName];
    
    return this.dashboardData.expensesByCategory
      .filter(expense => mappedCategories.includes(expense.category))
      .reduce((sum, expense) => sum + expense.amount, 0);
  }

  getCategorySpentPercentage(category: any): number {
    const spentAmount = this.getCategorySpentAmount(category.name);
    return category.amount > 0 ? Math.min((spentAmount / category.amount) * 100, 100) : 0;
  }

  navigateToExpenses(): void {
    this.router.navigate(['/expenses']);
  }

  navigateToIncome(): void {
    this.router.navigate(['/income']);
  }

  navigateToReports(): void {
    this.router.navigate(['/reports']);
  }

  toggleExpenseForm(): void {
    this.showExpenseForm = !this.showExpenseForm;
    this.showIncomeForm = false;
    if (this.showExpenseForm) {
      this.resetExpenseForm();
    }
  }

  toggleIncomeForm(): void {
    this.showIncomeForm = !this.showIncomeForm;
    this.showExpenseForm = false;
    if (this.showIncomeForm) {
      this.resetIncomeForm();
    }
  }

  resetExpenseForm(): void {
    this.expenseForm.reset({
      amount: null,
      date: DateUtils.getCurrentAustralianDate(),
      category: ExpenseCategory.Miscellaneous,
      description: '',
      paymentMethod: 'Credit Card',
      location: '',
      tags: '',
      isRecurring: false
    });
  }

  resetIncomeForm(): void {
    this.incomeForm.reset({
      amount: null,
      date: DateUtils.getCurrentAustralianDate(),
      source: 'Primary Income',
      notes: ''
    });
  }

  onSubmitExpense(): void {
    if (this.expenseForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      
      const rawExpenseData = this.expenseForm.value;
      
      // Format date for API using Australian timezone
      const expenseData: Expense = {
        ...rawExpenseData,
        date: DateUtils.formatForAPI(rawExpenseData.date),
        createdAt: new Date()
      };
      

      this.financeService.createExpense(expenseData).subscribe({
        next: (response) => {
          this.showExpenseForm = false;
          this.resetExpenseForm();
          this.loadDashboard();
          this.dataRefreshService.triggerExpenseRefresh();
          this.isSubmitting = false;
        },
        error: (error) => {
          console.error('Error creating expense:', error);
          this.isSubmitting = false;
        }
      });
    }
  }

  onSubmitIncome(): void {
    if (this.incomeForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      
      const rawIncomeData = this.incomeForm.value;
      
      // Format date for API using Australian timezone
      const incomeData: Income = {
        ...rawIncomeData,
        date: DateUtils.formatForAPI(rawIncomeData.date)
      };

      this.financeService.createIncome(incomeData).subscribe({
        next: (response) => {
          this.showIncomeForm = false;
          this.resetIncomeForm();
          this.loadDashboard();
          this.dataRefreshService.triggerIncomeRefresh();
          this.isSubmitting = false;
        },
        error: (error) => {
          console.error('Error creating income:', error);
          this.isSubmitting = false;
        }
      });
    }
  }

  getCategoryLabel(category: ExpenseCategory): string {
    return ExpenseCategoryLabels[category];
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
    this.loadDashboard();
    this.loadTotalSavings();
  }

  nextMonth(): void {
    const newDate = new Date(this.selectedDate);
    newDate.setMonth(newDate.getMonth() + 1);
    this.selectedDate = newDate;
    this.monthSelectionService.setSelectedDate(newDate);
    this.loadDashboard();
    this.loadTotalSavings();
  }

  openMonthPicker(): void {
    const dialogRef = this.dialog.open(MonthPickerDialogComponent, {
      width: '400px',
      data: { 
        month: this.selectedDate.getMonth(), 
        year: this.selectedDate.getFullYear() 
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        const newDate = new Date(result.year, result.month, 1);
        this.selectedDate = newDate;
        this.monthSelectionService.setSelectedDate(newDate);
        this.loadDashboard();
        this.loadTotalSavings();
      }
    });
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
    this.loadDashboard();
    this.loadTotalSavings();
  }

  // Total Savings Methods
  loadTotalSavings(): void {
    const month = this.selectedDate.getMonth() + 1;
    const year = this.selectedDate.getFullYear();
    
    this.financeService.getMonthlySavings(month, year).subscribe({
      next: (data) => {
        this.totalSavingsList = data.savings || [];
        this.totalSavingsAmount = data.totalAmount || 0;
      },
      error: (error) => {
        console.log('Error loading total savings:', error);
        this.totalSavingsList = [];
        this.totalSavingsAmount = 0;
      }
    });
  }

  toggleTotalSavingsForm(): void {
    this.showTotalSavingsForm = !this.showTotalSavingsForm;
    if (this.showTotalSavingsForm) {
      this.resetTotalSavingsForm();
    }
  }

  resetTotalSavingsForm(): void {
    this.editingSavingsId = null;
    this.totalSavingsForm.reset({
      amount: null,
      date: DateUtils.getCurrentAustralianDate(),
      category: 'Emergency Fund',
      description: ''
    });
  }

  cancelTotalSavingsForm(): void {
    this.showTotalSavingsForm = false;
    this.editingSavingsId = null;
    this.resetTotalSavingsForm();
  }

  onSubmitTotalSavings(): void {
    if (this.totalSavingsForm.valid && !this.isSubmittingSavings) {
      this.isSubmittingSavings = true;
      
      const rawSavingsData = this.totalSavingsForm.value;
      const savingsData: TotalSavings = {
        ...rawSavingsData,
        date: DateUtils.formatForAPI(rawSavingsData.date)
      };

      if (this.editingSavingsId) {
        // Update existing savings entry
        savingsData.id = this.editingSavingsId;
        this.financeService.updateTotalSavings(this.editingSavingsId, savingsData).subscribe({
          next: () => {
            this.showTotalSavingsForm = false;
            this.resetTotalSavingsForm();
            this.loadTotalSavings();
            this.isSubmittingSavings = false;
          },
          error: (error) => {
            console.error('Error updating total savings:', error);
            this.isSubmittingSavings = false;
          }
        });
      } else {
        // Create new savings entry
        this.financeService.createTotalSavings(savingsData).subscribe({
          next: () => {
            this.showTotalSavingsForm = false;
            this.resetTotalSavingsForm();
            this.loadTotalSavings();
            this.isSubmittingSavings = false;
          },
          error: (error) => {
            console.error('Error creating total savings:', error);
            this.isSubmittingSavings = false;
          }
        });
      }
    }
  }

  editTotalSavings(savings: TotalSavings): void {
    this.editingSavingsId = savings.id!;
    this.showTotalSavingsForm = true;
    
    this.totalSavingsForm.patchValue({
      amount: savings.amount,
      date: new Date(savings.date),
      category: savings.category,
      description: savings.description
    });
  }

  deleteTotalSavings(id: number): void {
    if (confirm('Are you sure you want to delete this savings entry?')) {
      this.financeService.deleteTotalSavings(id).subscribe({
        next: () => {
          this.loadTotalSavings();
        },
        error: (error) => {
          console.error('Error deleting total savings:', error);
        }
      });
    }
  }
}
