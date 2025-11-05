import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Income } from '../models/income.model';

@Injectable({
  providedIn: 'root'
})
export class IncomeService {
  private apiUrl = 'http://localhost:5291/api/income';

  constructor(private http: HttpClient) { }

  getIncomes(month?: number, year?: number): Observable<Income[]> {
    let params = new HttpParams();
    if (month) params = params.set('month', month.toString());
    if (year) params = params.set('year', year.toString());
    return this.http.get<Income[]>(this.apiUrl, { params });
  }

  getIncome(id: number): Observable<Income> {
    return this.http.get<Income>(`${this.apiUrl}/${id}`);
  }

  createIncome(income: Income): Observable<Income> {
    return this.http.post<Income>(this.apiUrl, income);
  }

  updateIncome(id: number, income: Income): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, income);
  }

  deleteIncome(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
