import { Component, input, output } from '@angular/core';

import { CategoryOption } from '../../../core/models/category.model';

@Component({
  selector: 'app-transactions-toolbar',
  standalone: true,
  template: `
    <section class="glass-panel rounded-4 p-4 mb-4">
      <div class="d-flex flex-column flex-lg-row justify-content-between gap-3 align-items-lg-center">
        <div>
          <p class="section-title mb-2">Gestao de transacoes</p>
          <h1 class="h2 mb-1">Lancamentos</h1>
          <p class="text-muted-soft mb-0">
            Filtre por categoria, crie novos registros e navegue ate os detalhes.
          </p>
        </div>

        <div class="d-flex flex-column flex-md-row gap-2">
          <select class="form-select" [value]="selectedCategoryId()" (change)="onCategoryChange($event)">
            <option value="all">Todas as categorias</option>
            @for (category of categories(); track category.id) {
              <option [value]="category.id">{{ category.name }}</option>
            }
          </select>

          <button type="button" class="btn btn-outline-light" (click)="refreshRequested.emit()">
            <i class="bi bi-arrow-repeat me-2"></i>
            Atualizar
          </button>

          <button type="button" class="btn btn-primary" (click)="createRequested.emit()">
            <i class="bi bi-plus-circle me-2"></i>
            Nova transacao
          </button>
        </div>
      </div>
    </section>
  `
})
export class TransactionsToolbarComponent {
  readonly categories = input<CategoryOption[]>([]);
  readonly selectedCategoryId = input('all');
  readonly categoryChanged = output<string>();
  readonly createRequested = output<void>();
  readonly refreshRequested = output<void>();

  onCategoryChange(event: Event): void {
    const categoryId = (event.target as HTMLSelectElement).value;
    this.categoryChanged.emit(categoryId);
  }
}
