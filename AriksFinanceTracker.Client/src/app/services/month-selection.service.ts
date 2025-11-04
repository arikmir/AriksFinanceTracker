import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { DateUtils } from '../utils/date.utils';

@Injectable({
  providedIn: 'root'
})
export class MonthSelectionService {
  private selectedDateSubject = new BehaviorSubject<Date>(DateUtils.getCurrentAustralianDate());
  public selectedDate$ = this.selectedDateSubject.asObservable();

  constructor() { }

  setSelectedDate(date: Date): void {
    this.selectedDateSubject.next(date);
  }

  getCurrentSelectedDate(): Date {
    return this.selectedDateSubject.value;
  }

  getCurrentMonth(): number {
    const australianMonthYear = DateUtils.getAustralianMonthYear(this.selectedDateSubject.value);
    return australianMonthYear.month;
  }

  getCurrentYear(): number {
    const australianMonthYear = DateUtils.getAustralianMonthYear(this.selectedDateSubject.value);
    return australianMonthYear.year;
  }
}