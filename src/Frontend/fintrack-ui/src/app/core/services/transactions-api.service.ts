import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiResponse } from '../models/api-response.model';
import { PagedResponse, PaginationParams } from '../models/pagination.model';
import { TransactionHistoryEntry } from '../models/transaction-history.model';
import { FinTransaction, TransactionMutationPayload, TransactionFilter } from '../models/transaction.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class TransactionsApiService extends ApiService {
  getTransactions(
    params: PaginationParams = { pageNumber: 1, pageSize: 5 },
    filters?: TransactionFilter
  ): Observable<PagedResponse<FinTransaction>> {
    let queryParams: any = {
      pageNumber: params.pageNumber.toString(),
      pageSize: params.pageSize.toString()
    };

    if (filters) {
      if (filters.description) queryParams.description = filters.description;
      if (filters.categoryId) queryParams.categoryId = filters.categoryId;
      if (filters.dateFrom) queryParams.dateFrom = filters.dateFrom;
      if (filters.dateTo) queryParams.dateTo = filters.dateTo;
      if (filters.type) queryParams.type = filters.type;
      if (filters.minAmount !== undefined && filters.minAmount !== null) queryParams.minAmount = filters.minAmount.toString();
      if (filters.maxAmount !== undefined && filters.maxAmount !== null) queryParams.maxAmount = filters.maxAmount.toString();
    }

    return this.unwrap(
      this.http.get<ApiResponse<PagedResponse<FinTransaction>>>(
        this.buildUrl('transactions'),
        { ...this.requestOptions(), params: queryParams }
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

  deleteTransaction(id: string): Observable<void> {
    return this.unwrap(
      this.http.delete<ApiResponse<void>>(
        this.buildUrl(`transactions/${id}`),
        this.requestOptions()
      )
    );
  }
}
