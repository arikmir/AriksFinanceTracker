import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-sidenav',
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.scss']
})
export class SidenavComponent {
  menuItems = [
    { route: '/dashboard', icon: 'dashboard', label: 'Dashboard' },
    { route: '/expenses', icon: 'receipt', label: 'Expenses' },
    { route: '/income', icon: 'attach_money', label: 'Income' },
    { route: '/budget', icon: 'account_balance_wallet', label: 'Budget' },
    { route: '/reports', icon: 'assessment', label: 'Reports' }
  ];

  constructor(private router: Router) {}

  isActive(route: string): boolean {
    return this.router.url === route;
  }
}
