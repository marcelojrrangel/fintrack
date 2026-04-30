import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

import {
  dashboardSummaryMock,
  transactionsMock,
} from '../../../testing/frontend-test-data';
import { DashboardApiService } from '../../../core/services/dashboard-api.service';
import { TransactionsApiService } from '../../../core/services/transactions-api.service';
import { DashboardPageComponent } from './dashboard-page.component';

describe('DashboardPageComponent', () => {
  it('should load dashboard data and compute balance evolution', async () => {
    const dashboardApi = jasmine.createSpyObj<DashboardApiService>('DashboardApiService', [
      'getDashboard',
    ]);
    const transactionsApi = jasmine.createSpyObj<TransactionsApiService>(
      'TransactionsApiService',
      ['getTransactions'],
    );

    dashboardApi.getDashboard.and.returnValue(of(dashboardSummaryMock));
    transactionsApi.getTransactions.and.returnValue(of(transactionsMock));

    TestBed.configureTestingModule({
      imports: [DashboardPageComponent],
      providers: [
        provideCharts(withDefaultRegisterables()),
        { provide: DashboardApiService, useValue: dashboardApi },
        { provide: TransactionsApiService, useValue: transactionsApi },
      ],
    });

    const fixture = TestBed.createComponent(DashboardPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.componentInstance.dashboard()).toEqual(dashboardSummaryMock);
    expect(fixture.componentInstance.recentTransactions().length).toBe(2);
    expect(fixture.componentInstance.balanceEvolution().at(-1)?.balance).toBe(2400);
  });

  it('should expose error when load fails', async () => {
    const dashboardApi = jasmine.createSpyObj<DashboardApiService>('DashboardApiService', [
      'getDashboard',
    ]);
    const transactionsApi = jasmine.createSpyObj<TransactionsApiService>(
      'TransactionsApiService',
      ['getTransactions'],
    );

    dashboardApi.getDashboard.and.returnValue(
      throwError(() => new Error('dashboard error')),
    );
    transactionsApi.getTransactions.and.returnValue(of(transactionsMock));

    TestBed.configureTestingModule({
      imports: [DashboardPageComponent],
      providers: [
        provideCharts(withDefaultRegisterables()),
        { provide: DashboardApiService, useValue: dashboardApi },
        { provide: TransactionsApiService, useValue: transactionsApi },
      ],
    });

    const fixture = TestBed.createComponent(DashboardPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.error()).toContain('Nao foi possivel carregar os dados do dashboard');
  });
});
