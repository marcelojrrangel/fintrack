import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import {
  transactionsMock,
} from '../../../testing/frontend-test-data';
import { TransactionsApiService } from '../../../core/services/transactions-api.service';
import { TransactionsPageComponent } from './transactions-page.component';

describe('TransactionsPageComponent', () => {
  it('should load and filter transactions by category', async () => {
    const transactionsApi = jasmine.createSpyObj<TransactionsApiService>(
      'TransactionsApiService',
      ['getTransactions', 'createTransaction'],
    );

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
    transactionsApi.createTransaction.and.returnValue(of(transactionsMock[0]));

    TestBed.configureTestingModule({
      imports: [TransactionsPageComponent],
      providers: [{ provide: TransactionsApiService, useValue: transactionsApi }],
    });

    const fixture = TestBed.createComponent(TransactionsPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    fixture.componentInstance.selectedCategoryId.set(transactionsMock[1].categoryId);

    expect(fixture.componentInstance.transactions().length).toBe(2);
    expect(fixture.componentInstance.filteredTransactions().length).toBe(1);
    expect(fixture.componentInstance.filteredTransactions()[0].description).toBe('Aluguel');
  });

  it('should close modal and refresh list after successful creation', async () => {
    const transactionsApi = jasmine.createSpyObj<TransactionsApiService>(
      'TransactionsApiService',
      ['getTransactions', 'createTransaction'],
    );

    const pagedResponse = {
      items: transactionsMock,
      pageNumber: 1,
      pageSize: 5,
      totalCount: transactionsMock.length,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    };
    transactionsApi.getTransactions.and.returnValues(of(pagedResponse), of(pagedResponse));
    transactionsApi.createTransaction.and.returnValue(of(transactionsMock[0]));

    TestBed.configureTestingModule({
      imports: [TransactionsPageComponent],
      providers: [{ provide: TransactionsApiService, useValue: transactionsApi }],
    });

    const fixture = TestBed.createComponent(TransactionsPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    fixture.componentInstance.isCreateModalOpen.set(true);

    await fixture.componentInstance.createTransaction({
      categoryId: transactionsMock[0].categoryId,
      amount: 100,
      transactionDateUtc: '2026-04-20T00:00:00Z',
      type: 'Income',
      description: 'Bonus',
    });

    expect(fixture.componentInstance.isCreateModalOpen()).toBeFalse();
    expect(fixture.componentInstance.modalError()).toBeNull();
    expect(transactionsApi.createTransaction).toHaveBeenCalled();
  });

  it('should surface modal error when creation fails', async () => {
    const transactionsApi = jasmine.createSpyObj<TransactionsApiService>(
      'TransactionsApiService',
      ['getTransactions', 'createTransaction'],
    );

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
    transactionsApi.createTransaction.and.returnValue(
      throwError(() => new Error('create error')),
    );

    TestBed.configureTestingModule({
      imports: [TransactionsPageComponent],
      providers: [{ provide: TransactionsApiService, useValue: transactionsApi }],
    });

    const fixture = TestBed.createComponent(TransactionsPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    await fixture.componentInstance.createTransaction({
      categoryId: transactionsMock[0].categoryId,
      amount: 100,
      transactionDateUtc: '2026-04-20T00:00:00Z',
      type: 'Income',
      description: 'Bonus',
    });

    expect(fixture.componentInstance.modalError()).toContain('Nao foi possivel salvar');
    expect(fixture.componentInstance.submitting()).toBeFalse();
  });
});
