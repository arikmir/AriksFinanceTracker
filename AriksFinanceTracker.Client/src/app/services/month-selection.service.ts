import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MonthSelectionService {
  private selectedDateSubject = new BehaviorSubject<Date>(new Date());
  public selectedDate$ = this.selectedDateSubject.asObservable();

  constructor() { }

  setSelectedDate(date: Date): void {
    this.selectedDateSubject.next(date);
  }

  getCurrentSelectedDate(): Date {
    return this.selectedDateSubject.value;
  }

  getCurrentMonth(): number {
    return this.selectedDateSubject.value.getMonth() + 1; // JavaScript months are 0-based
  }

  getCurrentYear(): number {
    return this.selectedDateSubject.value.getFullYear();
  }
}