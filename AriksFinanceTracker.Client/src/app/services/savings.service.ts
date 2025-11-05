import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TotalSavings {
  id?: number;
  date: string;
  amount: number;
  description: string;
  category: string;
  createdAt?: Date;
  updatedAt?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class SavingsService {
  private apiUrl = 'http://localhost:5291/api/totalsavings';

  constructor(private http: HttpClient) { }

  getTotalSavings(): Observable<TotalSavings[]> {
    return this.http.get<TotalSavings[]>(this.apiUrl);
  }

  getTotalSavingsById(id: number): Observable<TotalSavings> {
    return this.http.get<TotalSavings>(`${this.apiUrl}/${id}`);
  }

  getMonthlySavings(month: number, year: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/monthly/${year}/${month}`);
  }

  createTotalSavings(savings: TotalSavings): Observable<TotalSavings> {
    return this.http.post<TotalSavings>(this.apiUrl, savings);
  }

  updateTotalSavings(id: number, savings: TotalSavings): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, savings);
  }

  deleteTotalSavings(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
