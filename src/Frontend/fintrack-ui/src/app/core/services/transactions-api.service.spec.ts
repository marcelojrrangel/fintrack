import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import {
  historyMock,
  transactionsMock,
} from '../../testing/frontend-test-data';
import { TransactionHistoryEntry } from '../models/transaction-history.model';
import { FinTransaction, TransactionMutationPayload } from '../models/transaction.model';
import { TransactionsApiService } from './transactions-api.service';

describe('TransactionsApiService', () => {
  let service: TransactionsApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), TransactionsApiService],
    });

    service = TestBed.inject(TransactionsApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should get all transactions', () => {
    let responseValue: FinTransaction[] = [];

    service.getTransactions().subscribe((response) => {
      responseValue = response;
    });

    const request = httpMock.expectOne('/api/transactions');
    expect(request.request.method).toBe('GET');

    request.flush({
      success: true,
      message: 'ok',
      data: transactionsMock,
      errors: [],
    });

    expect(responseValue).toEqual(transactionsMock);
  });

  it('should create a transaction', () => {
    const payload: TransactionMutationPayload = {
      categoryId: transactionsMock[0].categoryId,
      amount: 99.5,
      transactionDateUtc: '2026-04-12T00:00:00Z',
      type: 'Expense',
      description: 'Cinema',
    };

    let responseValue = transactionsMock[0];

    service.createTransaction(payload).subscribe((response) => {
      responseValue = response;
    });

    const request = httpMock.expectOne('/api/transactions');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);

    request.flush({
      success: true,
      message: 'ok',
      data: transactionsMock[0],
      errors: [],
    });

    expect(responseValue).toEqual(transactionsMock[0]);
  });

  it('should get transaction history', () => {
    let responseValue: TransactionHistoryEntry[] = [];

    service.getTransactionHistory('tx-001').subscribe((response) => {
      responseValue = response;
    });

    const request = httpMock.expectOne('/api/transactions/tx-001/history');
    expect(request.request.method).toBe('GET');

    request.flush({
      success: true,
      message: 'ok',
      data: historyMock,
      errors: [],
    });

    expect(responseValue).toEqual(historyMock);
  });
});
