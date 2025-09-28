import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, forkJoin } from 'rxjs';
import { FinanceService } from '../../services/finance.service';
import { 
  Expense, 
  ExpenseCategory, 
  ExpenseCategoryLabels, 
  ExpenseCategoryIcons,
  ExpenseAnalytics,
  DailyExpense,
  CategorySummary 
} from '../../models/expense.model';

@Component({
  selector: 'app-expenses',
  templateUrl: './expenses.component.html',
  styleUrls: ['./expenses.component.scss']
})
export class ExpensesComponent implements OnInit {
  expenses: Expense[] = [];
  weeklyAnalytics!: ExpenseAnalytics;
  monthlyAnalytics!: ExpenseAnalytics;
  categorySummary: CategorySummary[] = [];
  
  expenseForm: FormGroup;
  showAddForm = false;
  editingExpense: Expense | null = null;
  
  ExpenseCategory = ExpenseCategory;
  ExpenseCategoryLabels = ExpenseCategoryLabels;
  ExpenseCategoryIcons = ExpenseCategoryIcons;
  
  categories = Object.values(ExpenseCategory).filter(value => typeof value === 'number') as ExpenseCategory[];
  paymentMethods = ['Cash', 'Credit Card', 'Debit Card', 'Bank Transfer', 'Digital Wallet'];
  
  selectedPeriod = 'week';
  filterCategory: ExpenseCategory | 'all' = 'all';
  startDate: Date | null = null;
  endDate: Date | null = null;

  constructor(
    private fb: FormBuilder,
    private financeService: FinanceService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    this.expenseForm = this.fb.group({
      amount: ['', [Validators.required, Validators.min(0.01)]],
      category: [ExpenseCategory.Miscellaneous, Validators.required],
      description: ['', [Validators.required, Validators.minLength(3)]],
      paymentMethod: [''],
      location: [''],
      tags: [''],
      isRecurring: [false],
      date: [new Date(), Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    forkJoin({
      expenses: this.financeService.getExpenses(),
      weeklyAnalytics: this.financeService.getWeeklyExpenseAnalytics(),
      monthlyAnalytics: this.financeService.getMonthlyExpenseAnalytics(),
      categorySummary: this.financeService.getCategorySummary()
    }).subscribe({
      next: (data) => {
        this.expenses = data.expenses;
        this.weeklyAnalytics = data.weeklyAnalytics;
        this.monthlyAnalytics = data.monthlyAnalytics;
        this.categorySummary = data.categorySummary;
      },
      error: (error) => {
        console.error('Error loading data:', error);
        this.snackBar.open('Error loading expense data', 'Close', { duration: 3000 });
      }
    });
  }

  onSubmit(): void {
    if (this.expenseForm.valid) {
      const formValue = this.expenseForm.value;
      const expense: Expense = {
        ...formValue,
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
      category: expense.category,
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
      category: ExpenseCategory.Miscellaneous,
      isRecurring: false,
      date: new Date()
    });
    this.showAddForm = false;
    this.editingExpense = null;
  }

  getCategoryIcon(category: ExpenseCategory): string {
    return this.ExpenseCategoryIcons[category];
  }

  getCategoryLabel(category: ExpenseCategory): string {
    return this.ExpenseCategoryLabels[category];
  }

  getFilteredExpenses(): Expense[] {
    let filtered = this.expenses;
    
    if (this.filterCategory !== 'all') {
      filtered = filtered.filter(e => e.category === this.filterCategory);
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
}
