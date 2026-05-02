import { Component, computed, effect, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { DEFAULT_CATEGORIES } from '../../../core/constants/default-categories';
import { CategoryOption } from '../../../core/models/category.model';
import { FinTransaction, TransactionMutationPayload, TransactionFilter } from '../../../core/models/transaction.model';
import { TransactionsApiService } from '../../../core/services/transactions-api.service';
import { TransactionFormModalComponent } from '../components/transaction-form-modal.component';
import { TransactionFilterModalComponent } from '../components/transaction-filter-modal.component';
import { TransactionTableComponent } from '../components/transaction-table.component';
import { TransactionsToolbarComponent } from '../components/transactions-toolbar.component';
import { UserPickerModalComponent } from '../../../core/components/user-picker-modal.component';
import { CurrentUserService } from '../../../core/services/current-user.service';
import { TransactionStateService } from './transaction-state.service';

@Component({
  selector: 'app-transactions-page',
  standalone: true,
  imports: [
    TransactionFormModalComponent,
    TransactionFilterModalComponent,
    TransactionTableComponent,
    TransactionsToolbarComponent,
    UserPickerModalComponent
  ],
  template: `
    <app-transactions-toolbar
      [categories]="categories()"
      [selectedCategoryId]="state.selectedCategoryId()"
      [searchTerm]="state.searchTerm()"
      [selectedDate]="state.selectedDate()"
      [hasActiveFilters]="hasActiveFilters()"
      (categoryChanged)="state.selectedCategoryId.set($event)"
      (searchChanged)="state.searchTerm.set($event)"
      (dateChanged)="state.selectedDate.set($event)"
      (createRequested)="isCreateModalOpen.set(true)"
      (filterRequested)="isFilterModalOpen.set(true)"
      (refreshRequested)="refresh()"
      (selectUserRequested)="isUserPickerOpen.set(true)"
    />

    @if (error(); as errorMessage) {
      <div class="alert alert-danger border-0 shadow-sm mb-4">{{ errorMessage }}</div>
    }

     <app-transaction-table [transactions]="filteredTransactions()" />
      
    <nav class="d-flex justify-content-between align-items-center mt-4 glass-panel p-3 rounded-4">
      <div class="text-muted-soft small">
        Mostrando {{ state.transactions().length }} de {{ totalCount() }} transações
        (Página {{ state.pageNumber() }} de {{ totalPages() }})
      </div>
      
      <ul class="pagination mb-0">
        <li class="page-item" [class.disabled]="!hasPreviousPage()">
          <button class="page-link bg-transparent border-secondary-subtle text-light" (click)="goToPage(state.pageNumber() - 1)">
            <i class="bi bi-chevron-left me-1"></i> Anterior
          </button>
        </li>
        <li class="page-item" [class.disabled]="!hasNextPage()">
          <button class="page-link bg-transparent border-secondary-subtle text-light ms-2" (click)="goToPage(state.pageNumber() + 1)">
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

    @if (isFilterModalOpen()) {
      <app-transaction-filter-modal
        [categories]="categories()"
        [currentFilters]="state.activeFilters()"
        (closeRequested)="isFilterModalOpen.set(false)"
        (filtersApplied)="applyFilters($event)"
      />
    }

    @if (isUserPickerOpen()) {
      <app-user-picker-modal
        (closeRequested)="isUserPickerOpen.set(false)"
        (userSelected)="onUserSelected()"
      />
    }
  `
})
export class TransactionsPageComponent {
  private readonly transactionsApi = inject(TransactionsApiService);
  private readonly currentUserService = inject(CurrentUserService);
  readonly state = inject(TransactionStateService);
  private readonly router = inject(Router);
  private readonly refreshTick = signal(0);

  readonly isCreateModalOpen = signal(false);
  readonly isFilterModalOpen = signal(false);
  readonly isUserPickerOpen = signal(false);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly modalError = signal<string | null>(null);

  readonly totalCount = signal(0);
  readonly totalPages = signal(0);
  readonly hasNextPage = signal(false);
  readonly hasPreviousPage = signal(false);

  readonly hasActiveFilters = computed(() => {
    const filters = this.state.activeFilters();
    return !!filters && Object.keys(filters).length > 0;
  });

  readonly categories = computed<CategoryOption[]>(() => {
    const categoriesMap = new Map(DEFAULT_CATEGORIES.map((category) => [category.id, category]));

    for (const transaction of this.state.transactions()) {
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
    const transactions = this.state.transactions();
    const selectedCategoryId = this.state.selectedCategoryId();
    const searchTerm = this.state.searchTerm().toLowerCase();
    const selectedDate = this.state.selectedDate();

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
      this.state.pageNumber();
      this.state.searchTerm();
      this.state.selectedCategoryId();
      this.state.selectedDate();
      this.currentUserService.currentUser();

      if (!this.currentUserService.currentUser()) {
        this.state.transactions.set([]);
        this.totalCount.set(0);
        this.totalPages.set(0);
        this.hasNextPage.set(false);
        this.hasPreviousPage.set(false);
        return;
      }

      const deletedId = this.state.consumePendingDeleteId();
      if (deletedId) {
        const updated = this.state.transactions().filter(t => t.id !== deletedId);
        this.state.transactions.set(updated);
        this.totalCount.update(c => Math.max(0, c - 1));
        const newTotalPages = Math.max(1, Math.ceil(this.totalCount() / this.state.pageSize()));
        this.totalPages.set(newTotalPages);
        if (this.state.pageNumber() > newTotalPages) {
          this.state.pageNumber.set(newTotalPages);
        }
        this.hasNextPage.set(this.state.pageNumber() < this.totalPages());
        this.hasPreviousPage.set(this.state.pageNumber() > 1);
        return;
      }

      void this.loadTransactions();
    });
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.state.pageNumber.set(page);
    }
  }

  refresh(): void {
    this.refreshTick.update((current) => current + 1);
  }

    onUserSelected(): void {
      this.state.reset();
      this.refresh();
      void this.router.navigateByUrl('/dashboard');
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

  applyFilters(filters: TransactionFilter): void {
    this.state.activeFilters.set(filters);
    this.isFilterModalOpen.set(false);
    this.state.pageNumber.set(1); // Resetar para primeira página ao filtrar
    this.refresh();
  }

  private async loadTransactions(): Promise<void> {
    this.error.set(null);

    try {
      const response = await firstValueFrom(
        this.transactionsApi.getTransactions(
          {
            pageNumber: this.state.pageNumber(),
            pageSize: this.state.pageSize()
          },
          this.state.activeFilters() ?? undefined
        )
      );
      
      this.state.transactions.set(response.items);
      this.totalCount.set(response.totalCount);
      this.totalPages.set(response.totalPages);
      this.hasNextPage.set(response.hasNextPage);
      this.hasPreviousPage.set(response.hasPreviousPage);
    } catch {
      this.error.set('Não foi possível carregar a listagem de transações.');
    }
  }
}
