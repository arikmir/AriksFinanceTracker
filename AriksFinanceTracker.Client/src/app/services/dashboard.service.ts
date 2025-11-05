import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DashboardData } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = 'http://localhost:5291/api/dashboard';

  constructor(private http: HttpClient) { }

  getDashboardData(month?: number, year?: number): Observable<DashboardData> {
    let params = new HttpParams();
    if (month !== undefined) params = params.set('month', month.toString());
    if (year !== undefined) params = params.set('year', year.toString());

    return this.http.get<DashboardData>(this.apiUrl, { params });
  }

  getYearlyDashboard(year?: number): Observable<any> {
    const yearParam = year ? `?year=${year}` : '';
    return this.http.get<any>(`${this.apiUrl}/yearly${yearParam}`);
  }
}
