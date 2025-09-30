import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-month-picker-dialog',
  template: `
    <h2 mat-dialog-title>Select Month & Year</h2>
    <mat-dialog-content>
      <div class="month-year-picker">
        <mat-form-field appearance="outline">
          <mat-label>Month</mat-label>
          <mat-select [(value)]="selectedMonth">
            <mat-option *ngFor="let month of months; let i = index" [value]="i">
              {{month}}
            </mat-option>
          </mat-select>
        </mat-form-field>
        
        <mat-form-field appearance="outline">
          <mat-label>Year</mat-label>
          <mat-select [(value)]="selectedYear">
            <mat-option *ngFor="let year of years" [value]="year">
              {{year}}
            </mat-option>
          </mat-select>
        </mat-form-field>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onConfirm()">Select</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .month-year-picker {
      display: flex;
      gap: 16px;
      padding: 16px 0;
    }
    mat-form-field {
      flex: 1;
    }
  `]
})
export class MonthPickerDialogComponent {
  months = ['January', 'February', 'March', 'April', 'May', 'June', 
            'July', 'August', 'September', 'October', 'November', 'December'];
  years: number[] = [];
  selectedMonth: number;
  selectedYear: number;

  constructor(
    public dialogRef: MatDialogRef<MonthPickerDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { month: number, year: number }
  ) {
    this.selectedMonth = data.month;
    this.selectedYear = data.year;
    
    // Generate years from 2020 to 2030
    const currentYear = new Date().getFullYear();
    for (let year = currentYear - 5; year <= currentYear + 5; year++) {
      this.years.push(year);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    this.dialogRef.close({ month: this.selectedMonth, year: this.selectedYear });
  }
}