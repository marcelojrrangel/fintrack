import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiResponse } from '../models/api-response.model';
import { TransactionHistoryEntry } from '../models/transaction-history.model';
import { FinTransaction, TransactionMutationPayload } from '../models/transaction.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class TransactionsApiService extends ApiService {
  getTransactions(): Observable<FinTransaction[]> {
    return this.unwrap(
      this.http.get<ApiResponse<FinTransaction[]>>(
        this.buildUrl('transactions'),
        this.requestOptions()
      )
    );
  }

  getTransactionById(id: string): Observable<FinTransaction> {
    return this.unwrap(
      this.http.get<ApiResponse<FinTransaction>>(
        this.buildUrl(`transactions/${id}`),
        this.requestOptions()
      )
    );
  }

  getTransactionHistory(id: string): Observable<TransactionHistoryEntry[]> {
    return this.unwrap(
      this.http.get<ApiResponse<TransactionHistoryEntry[]>>(
        this.buildUrl(`transactions/${id}/history`),
        this.requestOptions()
      )
    );
  }

  createTransaction(payload: TransactionMutationPayload): Observable<FinTransaction> {
    return this.unwrap(
      this.http.post<ApiResponse<FinTransaction>>(
        this.buildUrl('transactions'),
        payload,
        this.requestOptions()
      )
    );
  }
}
