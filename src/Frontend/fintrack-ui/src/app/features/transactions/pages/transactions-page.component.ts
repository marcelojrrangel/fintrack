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
      [searchTerm]="searchTerm()"
      [selectedDate]="selectedDate()"
      (categoryChanged)="selectedCategoryId.set($event)"
      (searchChanged)="searchTerm.set($event)"
      (dateChanged)="selectedDate.set($event)"
      (createRequested)="isCreateModalOpen.set(true)"
      (refreshRequested)="refresh()"
    />

    @if (error(); as errorMessage) {
      <div class="alert alert-danger border-0 shadow-sm mb-4">{{ errorMessage }}</div>
    }

    <app-transaction-table [transactions]="filteredTransactions()" />
    
    <nav class="d-flex justify-content-between align-items-center mt-4 glass-panel p-3 rounded-4">
      <div class="text-muted-soft small">
        Mostrando {{ transactions().length }} de {{ totalCount() }} transações 
        (Página {{ pageNumber() }} de {{ totalPages() }})
      </div>
      
      <ul class="pagination mb-0">
        <li class="page-item" [class.disabled]="!hasPreviousPage()">
          <button class="page-link bg-transparent border-secondary-subtle text-light" (click)="goToPage(pageNumber() - 1)">
            <i class="bi bi-chevron-left me-1"></i> Anterior
          </button>
        </li>
        <li class="page-item" [class.disabled]="!hasNextPage()">
          <button class="page-link bg-transparent border-secondary-subtle text-light ms-2" (click)="goToPage(pageNumber() + 1)">
            Próximo <i class="bi bi-chevron-right ms-1"></i>
          </button>
        </li>
      </ul>
    </nav>

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
  readonly searchTerm = signal('');
  readonly selectedDate = signal('');
  readonly isCreateModalOpen = signal(false);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly modalError = signal<string | null>(null);

  readonly pageNumber = signal(1);
  readonly pageSize = signal(5);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);
  readonly hasNextPage = signal(false);
  readonly hasPreviousPage = signal(false);

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
    const transactions = this.transactions();
    const selectedCategoryId = this.selectedCategoryId();
    const searchTerm = this.searchTerm().toLowerCase();
    const selectedDate = this.selectedDate();

    return transactions.filter((transaction) => {
      const matchesCategory =
        selectedCategoryId === 'all' || transaction.categoryId === selectedCategoryId;

      const matchesDescription =
        !searchTerm || transaction.description.toLowerCase().includes(searchTerm);

      const transactionDate = new Date(transaction.transactionDateUtc).toISOString().split('T')[0];
      const matchesDate = !selectedDate || transactionDate === selectedDate;

      return matchesCategory && matchesDescription && matchesDate;
    });
  });

  constructor() {
    effect(() => {
      this.refreshTick();
      this.pageNumber();
      void this.loadTransactions();
    });
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.pageNumber.set(page);
    }
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
        'Não foi possível salvar a nova transação. Confira se a API está ativa e tente novamente.'
      );
    } finally {
      this.submitting.set(false);
    }
  }

  private async loadTransactions(): Promise<void> {
    this.error.set(null);

    try {
      const response = await firstValueFrom(
        this.transactionsApi.getTransactions({
          pageNumber: this.pageNumber(),
          pageSize: this.pageSize()
        })
      );
      
      this.transactions.set(response.items);
      this.totalCount.set(response.totalCount);
      this.totalPages.set(response.totalPages);
      this.hasNextPage.set(response.hasNextPage);
      this.hasPreviousPage.set(response.hasPreviousPage);
    } catch {
      this.error.set('Não foi possível carregar a listagem de transações.');
    }
  }
}
