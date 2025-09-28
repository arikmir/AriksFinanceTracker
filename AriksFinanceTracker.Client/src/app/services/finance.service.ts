import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Expense, ExpenseAnalytics, DailyExpense, CategorySummary } from '../models/expense.model';
import { Income } from '../models/income.model';
import { DashboardData } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class FinanceService {
  private apiUrl = 'http://localhost:5001/api';

  constructor(private http: HttpClient) { }

  getExpenses(): Observable<Expense[]> {
    return this.http.get<Expense[]>(`${this.apiUrl}/expense`);
  }

  getExpense(id: number): Observable<Expense> {
    return this.http.get<Expense>(`${this.apiUrl}/expense/${id}`);
  }

  createExpense(expense: Expense): Observable<Expense> {
    return this.http.post<Expense>(`${this.apiUrl}/expense`, expense);
  }

  updateExpense(id: number, expense: Expense): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/expense/${id}`, expense);
  }

  deleteExpense(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/expense/${id}`);
  }

  getIncomes(): Observable<Income[]> {
    return this.http.get<Income[]>(`${this.apiUrl}/income`);
  }

  getIncome(id: number): Observable<Income> {
    return this.http.get<Income>(`${this.apiUrl}/income/${id}`);
  }

  createIncome(income: Income): Observable<Income> {
    return this.http.post<Income>(`${this.apiUrl}/income`, income);
  }

  updateIncome(id: number, income: Income): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/income/${id}`, income);
  }

  deleteIncome(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/income/${id}`);
  }

  getDashboardData(): Observable<DashboardData> {
    return this.http.get<DashboardData>(`${this.apiUrl}/dashboard`);
  }

  getYearlyDashboard(year?: number): Observable<any> {
    const yearParam = year ? `?year=${year}` : '';
    return this.http.get<any>(`${this.apiUrl}/dashboard/yearly${yearParam}`);
  }

  getDailyExpenseAnalytics(startDate?: Date, endDate?: Date): Observable<DailyExpense[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate.toISOString());
    if (endDate) params = params.set('endDate', endDate.toISOString());
    
    return this.http.get<DailyExpense[]>(`${this.apiUrl}/expense/analytics/daily`, { params });
  }

  getWeeklyExpenseAnalytics(): Observable<ExpenseAnalytics> {
    return this.http.get<ExpenseAnalytics>(`${this.apiUrl}/expense/analytics/weekly`);
  }

  getMonthlyExpenseAnalytics(): Observable<ExpenseAnalytics> {
    return this.http.get<ExpenseAnalytics>(`${this.apiUrl}/expense/analytics/monthly`);
  }

  getCategorySummary(startDate?: Date, endDate?: Date): Observable<CategorySummary[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate.toISOString());
    if (endDate) params = params.set('endDate', endDate.toISOString());
    
    return this.http.get<CategorySummary[]>(`${this.apiUrl}/expense/categories/summary`, { params });
  }
}
