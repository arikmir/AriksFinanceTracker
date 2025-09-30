import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Subscription } from 'rxjs';
import { FinanceService } from '../../services/finance.service';
import { DataRefreshService } from '../../services/data-refresh.service';
import { MonthSelectionService } from '../../services/month-selection.service';
import { DashboardData } from '../../models/dashboard.model';
import { Income } from '../../models/income.model';
import { Expense, ExpenseCategory, ExpenseCategoryLabels, ExpenseCategoryIcons } from '../../models/expense.model';
import { MonthPickerDialogComponent } from './month-picker-dialog.component';

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

  budgetCategories = [
    { name: 'Mortgage', amount: 1746, icon: 'home' },
    { name: 'Rent', amount: 2340, icon: 'apartment' },
    { name: 'Groceries', amount: 450, icon: 'shopping_cart' },
    { name: 'Transport', amount: 350, icon: 'directions_car' },
    { name: 'Utilities', amount: 375, icon: 'power' },
    { name: 'Subscriptions', amount: 63, icon: 'subscriptions' }
  ];

  private subscriptions: Subscription = new Subscription();

  // Form states
  showExpenseForm: boolean = false;
  showIncomeForm: boolean = false;
  expenseForm: FormGroup;
  incomeForm: FormGroup;
  isSubmitting: boolean = false;
  
  // Date navigation
  selectedDate: Date = new Date();
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
      date: [new Date(), Validators.required],
      category: [ExpenseCategory.Miscellaneous, Validators.required],
      description: ['', [Validators.required, Validators.minLength(3)]],
      paymentMethod: ['Credit Card'],
      location: [''],
      tags: [''],
      isRecurring: [false]
    });

    this.incomeForm = this.fb.group({
      amount: [null, [Validators.required, Validators.min(0.01)]],
      date: [new Date(), Validators.required],
      source: ['Primary Income', Validators.required],
      notes: ['']
    });
  }

  ngOnInit(): void {
    // Initialize with current month from service
    this.selectedDate = this.monthSelectionService.getCurrentSelectedDate();
    this.loadDashboard();
    
    this.subscriptions.add(
      this.dataRefreshService.incomeUpdated$.subscribe(() => {
        this.loadDashboard();
      })
    );
    
    this.subscriptions.add(
      this.dataRefreshService.expenseUpdated$.subscribe(() => {
        this.loadDashboard();
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadDashboard(): void {
    const month = this.selectedDate.getMonth() + 1; // JavaScript months are 0-based
    const year = this.selectedDate.getFullYear();
    
    this.financeService.getDashboardData(month, year).subscribe({
      next: (data) => {
        this.dashboardData = data;
      },
      error: (error) => {
        console.log('Using default data due to API error:', error);
      }
    });
  }

  getSavingsColor(): string {
    if (this.dashboardData.savingsRate >= 20) return 'var(--kiwi-medium-green)';
    if (this.dashboardData.savingsRate >= 10) return 'var(--kiwi-warning)';
    return 'var(--kiwi-error)';
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
      date: new Date(),
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
      date: new Date(),
      source: 'Primary Income',
      notes: ''
    });
  }

  onSubmitExpense(): void {
    if (this.expenseForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      
      const expenseData: Expense = {
        ...this.expenseForm.value,
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
      
      const incomeData: Income = {
        ...this.incomeForm.value
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

  getCategoryIcon(category: ExpenseCategory): string {
    return ExpenseCategoryIcons[category];
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
  }

  nextMonth(): void {
    const newDate = new Date(this.selectedDate);
    newDate.setMonth(newDate.getMonth() + 1);
    this.selectedDate = newDate;
    this.monthSelectionService.setSelectedDate(newDate);
    this.loadDashboard();
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
  }
}
