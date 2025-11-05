import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { BudgetStatus, FinancialHealth, SpendingCheck, CheckSpendingRequest, SavingsCelebration } from '../models/budget.model';

@Injectable({
  providedIn: 'root'
})
export class BudgetService {
  private apiUrl = 'http://localhost:5291/api/budget';

  constructor(private http: HttpClient) { }

  initializeBudget(): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/initialize`, {});
  }

  getBudgetStatus(): Observable<BudgetStatus> {
    return this.http.get<BudgetStatus>(`${this.apiUrl}/status`);
  }

  checkSpending(request: CheckSpendingRequest): Observable<SpendingCheck> {
    return this.http.post<SpendingCheck>(`${this.apiUrl}/check-spending`, request);
  }

  getFinancialHealth(): Observable<FinancialHealth> {
    return this.http.get<FinancialHealth>(`${this.apiUrl}/financial-health`);
  }

  getSavingsCelebration(): Observable<SavingsCelebration> {
    return this.http.get<SavingsCelebration>(`${this.apiUrl}/savings-celebration`);
  }

  getAlerts(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/alerts`);
  }

  getBudgetLimits(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/limits`);
  }

  updateCategoryLimit(category: number, newLimit: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/category/${category}/limit`, { newLimit });
  }
}
