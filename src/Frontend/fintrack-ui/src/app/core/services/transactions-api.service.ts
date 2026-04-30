import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiResponse } from '../models/api-response.model';
import { PagedResponse, PaginationParams } from '../models/pagination.model';
import { TransactionHistoryEntry } from '../models/transaction-history.model';
import { FinTransaction, TransactionMutationPayload } from '../models/transaction.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class TransactionsApiService extends ApiService {
  getTransactions(params: PaginationParams = { pageNumber: 1, pageSize: 5 }): Observable<PagedResponse<FinTransaction>> {
    const httpParams = {
      ...this.requestOptions(),
      params: {
        pageNumber: params.pageNumber.toString(),
        pageSize: params.pageSize.toString()
      }
    };

    return this.unwrap(
      this.http.get<ApiResponse<PagedResponse<FinTransaction>>>(
        this.buildUrl('transactions'),
        httpParams
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
