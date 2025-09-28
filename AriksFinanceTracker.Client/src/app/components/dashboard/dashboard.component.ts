import { Component, OnInit } from '@angular/core';
import { FinanceService } from '../../services/finance.service';
import { DashboardData } from '../../models/dashboard.model';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  dashboardData: DashboardData = {
    totalIncome: 8000,
    totalExpenses: 6464,
    netSavings: 1536,
    savingsRate: 19.2,
    expensesByCategory: []
  };

  budgetCategories = [
    { name: 'Mortgage', amount: 1746, icon: 'home' },
    { name: 'Rent', amount: 2340, icon: 'apartment' },
    { name: 'Groceries', amount: 450, icon: 'shopping_cart' },
    { name: 'Transport', amount: 350, icon: 'directions_car' },
    { name: 'Utilities', amount: 375, icon: 'power' },
    { name: 'Subscriptions', amount: 63, icon: 'subscriptions' }
  ];

  constructor(private financeService: FinanceService) { }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.financeService.getDashboardData().subscribe({
      next: (data) => {
        this.dashboardData = data;
      },
      error: (error) => {
        console.log('Using default data due to API error:', error);
      }
    });
  }

  getSavingsColor(): string {
    if (this.dashboardData.savingsRate >= 20) return 'var(--kiwi-medium-green)';
    if (this.dashboardData.savingsRate >= 10) return 'var(--kiwi-warning)';
    return 'var(--kiwi-error)';
  }
}
