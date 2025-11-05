import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Expense, ExpenseAnalytics, DailyExpense, CategorySummary } from '../models/expense.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ExpenseService {
  private apiUrl = 'http://localhost:5291/api/expense';

  constructor(private http: HttpClient) { }

  getExpenses(month?: number, year?: number): Observable<Expense[]> {
    let params = new HttpParams();
    if (month) params = params.set('month', month.toString());
    if (year) params = params.set('year', year.toString());
    return this.http.get<Expense[]>(this.apiUrl, { params });
  }

  getExpense(id: number): Observable<Expense> {
    return this.http.get<Expense>(`${this.apiUrl}/${id}`);
  }

  createExpense(expense: Expense): Observable<Expense> {
    return this.http.post<Expense>(this.apiUrl, expense);
  }

  updateExpense(id: number, expense: Expense): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, expense);
  }

  deleteExpense(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getDailyAnalytics(startDate?: Date, endDate?: Date): Observable<DailyExpense[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate.toISOString());
    if (endDate) params = params.set('endDate', endDate.toISOString());

    return this.http.get<DailyExpense[]>(`${this.apiUrl}/analytics/daily`, { params });
  }

  getWeeklyAnalytics(date?: Date): Observable<ExpenseAnalytics> {
    let params = new HttpParams();
    if (date) {
      params = params.set('month', (date.getMonth() + 1).toString());
      params = params.set('year', date.getFullYear().toString());
    }
    return this.http.get<ExpenseAnalytics>(`${this.apiUrl}/analytics/weekly`, { params });
  }

  getMonthlyAnalytics(date?: Date): Observable<ExpenseAnalytics> {
    let params = new HttpParams();
    if (date) {
      params = params.set('month', (date.getMonth() + 1).toString());
      params = params.set('year', date.getFullYear().toString());
    }
    return this.http.get<ExpenseAnalytics>(`${this.apiUrl}/analytics/monthly`, { params });
  }

  getCategorySummary(month?: number, year?: number): Observable<CategorySummary[]> {
    let params = new HttpParams();
    if (month) params = params.set('month', month.toString());
    if (year) params = params.set('year', year.toString());

    return this.http.get<CategorySummary[]>(`${this.apiUrl}/categories/summary`, { params });
  }
}
