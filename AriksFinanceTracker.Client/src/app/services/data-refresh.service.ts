import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DataRefreshService {
  private incomeRefreshSource = new Subject<void>();
  private expenseRefreshSource = new Subject<void>();
  private budgetRefreshSource = new Subject<void>();
  private dashboardRefreshSource = new Subject<void>();

  // Observable streams
  incomeRefresh$ = this.incomeRefreshSource.asObservable();
  expenseRefresh$ = this.expenseRefreshSource.asObservable();
  budgetRefresh$ = this.budgetRefreshSource.asObservable();
  dashboardRefresh$ = this.dashboardRefreshSource.asObservable();

  // Legacy observables for backward compatibility
  incomeUpdated$ = this.incomeRefreshSource.asObservable();
  expenseUpdated$ = this.expenseRefreshSource.asObservable();

  // Trigger methods
  triggerIncomeRefresh(): void {
    this.incomeRefreshSource.next();
    this.triggerDashboardRefresh(); // Income changes affect dashboard
    this.triggerBudgetRefresh(); // Income changes affect budget
  }

  triggerExpenseRefresh(): void {
    this.expenseRefreshSource.next();
    this.triggerDashboardRefresh(); // Expense changes affect dashboard
    this.triggerBudgetRefresh(); // Expense changes affect budget
  }

  triggerBudgetRefresh(): void {
    this.budgetRefreshSource.next();
  }

  triggerDashboardRefresh(): void {
    this.dashboardRefreshSource.next();
  }

  // Trigger all data refresh (useful for major updates)
  triggerFullRefresh(): void {
    this.triggerIncomeRefresh();
    this.triggerExpenseRefresh();
    this.triggerBudgetRefresh();
    this.triggerDashboardRefresh();
  }
}