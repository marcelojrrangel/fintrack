import { Component, computed, effect, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { DEFAULT_CATEGORIES } from '../../../core/constants/default-categories';
import { CategoryOption } from '../../../core/models/category.model';
import { FinTransaction, TransactionMutationPayload } from '../../../core/models/transaction.model';
import { TransactionsApiService } from '../../../core/services/transactions-api.service';
import { TransactionFormModalComponent } from '../components/transaction-form-modal.component';
import { TransactionTableComponent } from '../components/transaction-table.component';
import { TransactionsToolbarComponent } from '../components/transactions-toolbar.component';

@Component({
  selector: 'app-transactions-page',
  standalone: true,
  imports: [TransactionFormModalComponent, TransactionTableComponent, TransactionsToolbarComponent],
  template: `
    <app-transactions-toolbar
      [categories]="categories()"
      [selectedCategoryId]="selectedCategoryId()"
      (categoryChanged)="selectedCategoryId.set($event)"
      (createRequested)="isCreateModalOpen.set(true)"
      (refreshRequested)="refresh()"
    />

    @if (error(); as errorMessage) {
      <div class="alert alert-danger border-0 shadow-sm mb-4">{{ errorMessage }}</div>
    }

    <app-transaction-table [transactions]="filteredTransactions()" />

    @if (isCreateModalOpen()) {
      <app-transaction-form-modal
        [categories]="categories()"
        [submitting]="submitting()"
        [errorMessage]="modalError()"
        (closeRequested)="closeModal()"
        (saveRequested)="createTransaction($event)"
      />
    }
  `
})
export class TransactionsPageComponent {
  private readonly transactionsApi = inject(TransactionsApiService);
  private readonly refreshTick = signal(0);

  readonly transactions = signal<FinTransaction[]>([]);
  readonly selectedCategoryId = signal('all');
  readonly isCreateModalOpen = signal(false);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly modalError = signal<string | null>(null);

  readonly categories = computed<CategoryOption[]>(() => {
    const categoriesMap = new Map(DEFAULT_CATEGORIES.map((category) => [category.id, category]));

    for (const transaction of this.transactions()) {
      if (!categoriesMap.has(transaction.categoryId)) {
        categoriesMap.set(transaction.categoryId, {
          id: transaction.categoryId,
          name: transaction.categoryName
        });
      }
    }

    return [...categoriesMap.values()].sort((left, right) => left.name.localeCompare(right.name));
  });

  readonly filteredTransactions = computed(() => {
    const selectedCategoryId = this.selectedCategoryId();

    if (selectedCategoryId === 'all') {
      return this.transactions();
    }

    return this.transactions().filter((transaction) => transaction.categoryId === selectedCategoryId);
  });

  constructor() {
    effect(() => {
      this.refreshTick();
      void this.loadTransactions();
    });
  }

  refresh(): void {
    this.refreshTick.update((current) => current + 1);
  }

  closeModal(): void {
    this.isCreateModalOpen.set(false);
    this.modalError.set(null);
  }

  async createTransaction(payload: TransactionMutationPayload): Promise<void> {
    this.submitting.set(true);
    this.modalError.set(null);

    try {
      await firstValueFrom(this.transactionsApi.createTransaction(payload));
      this.closeModal();
      this.refresh();
    } catch {
      this.modalError.set(
        'Nao foi possivel salvar a nova transacao. Confira se a API esta ativa e tente novamente.'
      );
    } finally {
      this.submitting.set(false);
    }
  }

  private async loadTransactions(): Promise<void> {
    this.error.set(null);

    try {
      const transactions = await firstValueFrom(this.transactionsApi.getTransactions());
      this.transactions.set(transactions);
    } catch {
      this.error.set('Nao foi possivel carregar a listagem de transacoes.');
    }
  }
}
