import { Injectable, signal } from '@angular/core';
import { TransactionFilter } from '../../../core/models/transaction.model';
import { FinTransaction } from '../../../core/models/transaction.model';

@Injectable({ providedIn: 'root' })
export class TransactionStateService {
  readonly pageNumber = signal(1);
  readonly pageSize = signal(5);
  readonly searchTerm = signal('');
  readonly selectedCategoryId = signal('all');
  readonly selectedDate = signal('');
  readonly activeFilters = signal<TransactionFilter | null>(null);
  readonly transactions = signal<FinTransaction[]>([]);
  private _pendingDeleteId: string | null = null;

  setPendingDeleteId(id: string): void {
    this._pendingDeleteId = id;
  }

  consumePendingDeleteId(): string | null {
    const id = this._pendingDeleteId;
    this._pendingDeleteId = null;
    return id;
  }

  reset() {
    this.pageNumber.set(1);
    this.searchTerm.set('');
    this.selectedCategoryId.set('all');
    this.selectedDate.set('');
    this.activeFilters.set(null);
    this.transactions.set([]);
    this._pendingDeleteId = null;
  }
}