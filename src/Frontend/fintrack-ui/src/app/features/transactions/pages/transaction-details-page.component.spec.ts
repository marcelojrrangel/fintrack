import { TestBed } from '@angular/core/testing';
import { convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

import {
  historyMock,
  transactionsMock,
} from '../../../testing/frontend-test-data';
import { TransactionsApiService } from '../../../core/services/transactions-api.service';
import { TransactionDetailsPageComponent } from './transaction-details-page.component';

describe('TransactionDetailsPageComponent', () => {
  it('should load selected transaction and history', async () => {
    const transactionsApi = jasmine.createSpyObj<TransactionsApiService>(
      'TransactionsApiService',
      ['getTransactionById', 'getTransactionHistory', 'getTransactions'],
    );

    transactionsApi.getTransactionById.and.returnValue(of(transactionsMock[0]));
    transactionsApi.getTransactionHistory.and.returnValue(of(historyMock));
    transactionsApi.getTransactions.and.returnValue(
      of({
        items: transactionsMock,
        pageNumber: 1,
        pageSize: 5,
        totalCount: transactionsMock.length,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
      }),
    );

    TestBed.configureTestingModule({
      imports: [TransactionDetailsPageComponent],
      providers: [
        provideCharts(withDefaultRegisterables()),
        provideRouter([]),
        {
          provide: TransactionsApiService,
          useValue: transactionsApi,
        },
        {
          provide: (await import('@angular/router')).ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ id: 'tx-001' }),
            },
          },
        },
      ],
    });

    const fixture = TestBed.createComponent(TransactionDetailsPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.transaction()?.id).toBe('tx-001');
    expect(fixture.componentInstance.history().length).toBe(1);
    expect(fixture.componentInstance.allTransactions().length).toBe(2);
  });

  it('should expose error when details cannot be loaded', async () => {
    const transactionsApi = jasmine.createSpyObj<TransactionsApiService>(
      'TransactionsApiService',
      ['getTransactionById', 'getTransactionHistory', 'getTransactions'],
    );

    transactionsApi.getTransactionById.and.returnValue(
      throwError(() => new Error('detail error')),
    );
    transactionsApi.getTransactionHistory.and.returnValue(of(historyMock));
    transactionsApi.getTransactions.and.returnValue(
      of({
        items: transactionsMock,
        pageNumber: 1,
        pageSize: 5,
        totalCount: transactionsMock.length,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
      }),
    );

    TestBed.configureTestingModule({
      imports: [TransactionDetailsPageComponent],
      providers: [
        provideCharts(withDefaultRegisterables()),
        provideRouter([]),
        {
          provide: TransactionsApiService,
          useValue: transactionsApi,
        },
        {
          provide: (await import('@angular/router')).ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ id: 'tx-001' }),
            },
          },
        },
      ],
    });

    const fixture = TestBed.createComponent(TransactionDetailsPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.error()).toContain('Nao foi possivel carregar os detalhes');
    expect(fixture.componentInstance.transaction()).toBeNull();
  });
});
