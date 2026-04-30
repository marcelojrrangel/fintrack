import { Component, input, output } from '@angular/core';

import { CategoryOption } from '../../../core/models/category.model';

@Component({
  selector: 'app-transactions-toolbar',
  standalone: true,
  template: `
    <section class="glass-panel rounded-4 p-4 mb-4">
      <div class="d-flex flex-column gap-4">
        <div>
          <p class="section-title mb-2">Gestao de transacoes</p>
          <h1 class="h2 mb-1">Lancamentos</h1>
          <p class="text-muted-soft mb-0">
            Filtre por categoria, crie novos registros e navegue ate os detalhes.
          </p>
        </div>

        <div class="d-flex flex-wrap gap-2 align-items-center">
          <div class="position-relative" style="min-width: 280px;">
            <i class="bi bi-search position-absolute top-50 start-0 translate-middle-y ms-3 text-muted"></i>
            <input
              type="text"
              class="form-control bg-transparent border-secondary-subtle ps-5"
              placeholder="Buscar por descricao..."
              [value]="searchTerm()"
              (input)="onSearchChange($event)"
            />
          </div>

          <input
            type="date"
            class="form-control bg-transparent border-secondary-subtle"
            style="width: 160px;"
            [value]="selectedDate()"
            (change)="onDateChange($event)"
          />

          <select class="form-select border-secondary-subtle" style="width: 180px;" [value]="selectedCategoryId()" (change)="onCategoryChange($event)">
            <option value="all">Categorias</option>
            @for (category of categories(); track category.id) {
              <option [value]="category.id">{{ category.name }}</option>
            }
          </select>

          <div class="d-flex gap-2 ms-auto">
            <button type="button" class="btn btn-outline-light" (click)="refreshRequested.emit()" title="Atualizar">
              <i class="bi bi-arrow-repeat"></i>
            </button>

            <button type="button" class="btn btn-primary px-4" (click)="createRequested.emit()">
              <i class="bi bi-plus-circle me-2"></i>
              Nova Transação
            </button>
          </div>
        </div>
      </div>
    </section>
  `
})
export class TransactionsToolbarComponent {
  readonly categories = input<CategoryOption[]>([]);
  readonly selectedCategoryId = input('all');
  readonly searchTerm = input('');
  readonly selectedDate = input('');

  readonly categoryChanged = output<string>();
  readonly searchChanged = output<string>();
  readonly dateChanged = output<string>();
  readonly createRequested = output<void>();
  readonly refreshRequested = output<void>();

  onCategoryChange(event: Event): void {
    const categoryId = (event.target as HTMLSelectElement).value;
    this.categoryChanged.emit(categoryId);
  }

  onSearchChange(event: Event): void {
    const term = (event.target as HTMLInputElement).value;
    this.searchChanged.emit(term);
  }

  onDateChange(event: Event): void {
    const date = (event.target as HTMLInputElement).value;
    this.dateChanged.emit(date);
  }
}
