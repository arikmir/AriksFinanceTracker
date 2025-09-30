import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { FinanceService } from '../../services/finance.service';
import { DataRefreshService } from '../../services/data-refresh.service';
import { MonthSelectionService } from '../../services/month-selection.service';
import { Income } from '../../models/income.model';

@Component({
  selector: 'app-income',
  templateUrl: './income.component.html',
  styleUrls: ['./income.component.scss']
})
export class IncomeComponent implements OnInit, OnDestroy {
  incomeData: Income = {
    date: new Date(),
    amount: 0,
    source: 'Salary',
    notes: ''
  };
  
  isLoading: boolean = false;
  feedbackMessage: string = '';
  feedbackType: 'success' | 'error' = 'success';
  totalIncome: number = 0;
  incomeCount: number = 0;
  primarySource: string = 'Salary';
  recentIncomes: Income[] = [];
  editingIncome: Income | null = null;
  
  private subscriptions: Subscription = new Subscription();
  private selectedMonth: number = new Date().getMonth() + 1;
  private selectedYear: number = new Date().getFullYear();

  constructor(
    private financeService: FinanceService,
    private dataRefreshService: DataRefreshService,
    private monthSelectionService: MonthSelectionService
  ) { }

  ngOnInit(): void {
    // Initialize with current selected month
    this.selectedMonth = this.monthSelectionService.getCurrentMonth();
    this.selectedYear = this.monthSelectionService.getCurrentYear();
    
    this.loadTotalIncome();
    this.loadIncomeHistory();
    
    // Subscribe to month changes
    this.subscriptions.add(
      this.monthSelectionService.selectedDate$.subscribe(date => {
        this.selectedMonth = date.getMonth() + 1;
        this.selectedYear = date.getFullYear();
        this.loadTotalIncome();
        this.loadIncomeHistory();
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadTotalIncome(): void {
    this.financeService.getIncomes().subscribe({
      next: (incomes) => {
        // Filter incomes for selected month/year
        const monthlyIncomes = incomes.filter(income => {
          const incomeDate = new Date(income.date);
          return incomeDate.getMonth() + 1 === this.selectedMonth && 
                 incomeDate.getFullYear() === this.selectedYear;
        });
        
        this.totalIncome = monthlyIncomes.reduce((sum, income) => sum + income.amount, 0);
        this.incomeCount = monthlyIncomes.length;
        
        // Find most common source
        if (monthlyIncomes.length > 0) {
          const sourceCounts = monthlyIncomes.reduce((acc: any, income) => {
            acc[income.source] = (acc[income.source] || 0) + 1;
            return acc;
          }, {});
          this.primarySource = Object.keys(sourceCounts).reduce((a, b) => 
            sourceCounts[a] > sourceCounts[b] ? a : b
          );
        } else {
          this.primarySource = 'Salary';
        }
      },
      error: (error) => {
        console.error('Error loading incomes:', error);
      }
    });
  }

  loadIncomeHistory(): void {
    this.financeService.getIncomes().subscribe({
      next: (incomes) => {
        // Filter incomes for selected month/year
        const monthlyIncomes = incomes.filter(income => {
          const incomeDate = new Date(income.date);
          return incomeDate.getMonth() + 1 === this.selectedMonth && 
                 incomeDate.getFullYear() === this.selectedYear;
        });
        
        this.recentIncomes = monthlyIncomes
          .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
          .slice(0, 10); // Show last 10 entries for the selected month
      },
      error: (error) => {
        console.error('Error loading income history:', error);
      }
    });
  }

  savePrimaryIncome(): void {
    if (!this.incomeData.amount || !this.incomeData.date || !this.incomeData.source) {
      this.showFeedback('Please fill in all required fields', 'error');
      return;
    }

    this.isLoading = true;
    this.feedbackMessage = '';

    // Check if we're editing an existing income
    if (this.editingIncome && this.editingIncome.id) {
      // Update existing income
      this.financeService.updateIncome(this.editingIncome.id, this.incomeData).subscribe({
        next: (response) => {
          this.showFeedback('Income updated successfully!', 'success');
          this.resetForm();
          this.loadTotalIncome();
          this.loadIncomeHistory();
          this.dataRefreshService.triggerIncomeRefresh();
          this.isLoading = false;
        },
        error: (error) => {
          this.showFeedback('Error updating income. Please try again.', 'error');
          console.error('Error updating income:', error);
          this.isLoading = false;
        }
      });
    } else {
      // Create new income
      this.financeService.createIncome(this.incomeData).subscribe({
        next: (response) => {
          this.showFeedback('Income saved successfully!', 'success');
          this.resetForm();
          this.loadTotalIncome();
          this.loadIncomeHistory();
          this.dataRefreshService.triggerIncomeRefresh();
          this.isLoading = false;
        },
        error: (error) => {
          this.showFeedback('Error saving income. Please try again.', 'error');
          console.error('Error saving income:', error);
          this.isLoading = false;
        }
      });
    }
  }

  private showFeedback(message: string, type: 'success' | 'error'): void {
    this.feedbackMessage = message;
    this.feedbackType = type;
    setTimeout(() => {
      this.feedbackMessage = '';
    }, 5000);
  }

  resetForm(): void {
    this.incomeData = {
      date: new Date(),
      amount: 0,
      source: 'Salary',
      notes: ''
    };
    this.editingIncome = null;
  }

  getSourceIcon(source: string): string {
    const icons: any = {
      'Salary': 'work',
      'Freelance': 'laptop_mac',
      'Investment': 'trending_up',
      'Bonus': 'card_giftcard',
      'Other': 'more_horiz'
    };
    return icons[source] || 'attach_money';
  }

  editIncome(income: Income): void {
    this.editingIncome = income;
    this.incomeData = { ...income };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  deleteIncome(income: Income): void {
    const confirmMessage = `Are you sure you want to delete this ${income.source} income of $${income.amount.toFixed(2)}?\n\nDate: ${new Date(income.date).toLocaleDateString()}\nThis action cannot be undone.`;
    
    if (confirm(confirmMessage)) {
      if (income.id) {
        this.isLoading = true;
        this.financeService.deleteIncome(income.id).subscribe({
          next: () => {
            this.showFeedback('Income deleted successfully', 'success');
            this.loadTotalIncome();
            this.loadIncomeHistory();
            this.dataRefreshService.triggerIncomeRefresh();
            this.isLoading = false;
            
            // If we were editing this income, clear the form
            if (this.editingIncome && this.editingIncome.id === income.id) {
              this.resetForm();
            }
          },
          error: (error) => {
            this.showFeedback('Error deleting income', 'error');
            console.error('Error deleting income:', error);
            this.isLoading = false;
          }
        });
      }
    }
  }
}
