import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DataRefreshService {
  private incomeUpdatedSource = new Subject<void>();
  private expenseUpdatedSource = new Subject<void>();

  incomeUpdated$ = this.incomeUpdatedSource.asObservable();
  expenseUpdated$ = this.expenseUpdatedSource.asObservable();

  triggerIncomeRefresh(): void {
    this.incomeUpdatedSource.next();
  }

  triggerExpenseRefresh(): void {
    this.expenseUpdatedSource.next();
  }
}