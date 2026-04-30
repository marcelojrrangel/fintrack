import { Routes } from '@angular/router';

import { DashboardPageComponent } from './features/dashboard/pages/dashboard-page.component';
import { TransactionDetailsPageComponent } from './features/transactions/pages/transaction-details-page.component';
import { TransactionsPageComponent } from './features/transactions/pages/transactions-page.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'dashboard', component: DashboardPageComponent },
  { path: 'transactions', component: TransactionsPageComponent },
  { path: 'transactions/:id', component: TransactionDetailsPageComponent },
  { path: '**', redirectTo: 'dashboard' }
];
