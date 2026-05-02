import { Component, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

import { CategoryOption } from '../../../core/models/category.model';
import { TransactionFilter, TransactionType } from '../../../core/models/transaction.model';

@Component({
  selector: 'app-transaction-filter-modal',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="modal-backdrop fade show"></div>
    <div class="modal d-block" tabindex="-1" role="dialog" aria-modal="true">
      <div class="modal-dialog modal-dialog-centered modal-lg">
        <div class="modal-content glass-panel border-0 rounded-4">
          <div class="modal-header border-secondary-subtle">
            <div>
              <p class="section-title mb-2">Busca Avançada</p>
              <h2 class="h4 mb-0">Filtros de Transações</h2>
            </div>
            <button type="button" class="btn-close btn-close-white" aria-label="Fechar" (click)="closeRequested.emit()"></button>
          </div>

          <form class="modal-body" [formGroup]="form">
            <div class="row g-3">
              <!-- Descrição -->
              <div class="col-12">
                <label class="form-label">Descrição</label>
                <input type="text" class="form-control" formControlName="description" placeholder="Ex.: Supermercado, Salário..." />
              </div>

              <!-- Categoria e Tipo -->
              <div class="col-12 col-md-6">
                <label class="form-label">Categoria</label>
                <select class="form-select" formControlName="categoryId">
                  <option value="">Todas as categorias</option>
                  @for (category of categories(); track category.id) {
                    <option [value]="category.id">{{ category.name }}</option>
                  }
                </select>
              </div>

              <div class="col-12 col-md-6">
                <label class="form-label">Tipo</label>
                <select class="form-select" formControlName="type">
                  <option value="">Todos os tipos</option>
                  <option value="Income">Entrada</option>
                  <option value="Expense">Saída</option>
                </select>
              </div>

              <!-- Range de Datas -->
              <div class="col-12 col-md-6">
                <label class="form-label">De (Data)</label>
                <input type="date" class="form-control" formControlName="dateFrom" />
              </div>

              <div class="col-12 col-md-6">
                <label class="form-label">Até (Data)</label>
                <input type="date" class="form-control" formControlName="dateTo" />
              </div>

              <!-- Range de Valores -->
              <div class="col-12 col-md-6">
                <label class="form-label">Valor Mínimo</label>
                <div class="input-group">
                  <span class="input-group-text bg-transparent border-secondary-subtle">R$</span>
                  <input type="number" step="0.01" class="form-control" formControlName="minAmount" placeholder="0.00" />
                </div>
              </div>

              <div class="col-12 col-md-6">
                <label class="form-label">Valor Máximo</label>
                <div class="input-group">
                  <span class="input-group-text bg-transparent border-secondary-subtle">R$</span>
                  <input type="number" step="0.01" class="form-control" formControlName="maxAmount" placeholder="9999.99" />
                </div>
              </div>
            </div>
          </form>

          <div class="modal-footer border-secondary-subtle">
            <button type="button" class="btn btn-outline-light" (click)="resetForm()">Limpar Filtros</button>
            <div class="ms-auto d-flex gap-2">
              <button type="button" class="btn btn-soft" (click)="closeRequested.emit()">Cancelar</button>
              <button type="button" class="btn btn-primary px-4" (click)="applyFilters()">
                <i class="bi bi-filter me-2"></i>
                Aplicar Filtros
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: `
    .modal {
      background: rgba(2, 6, 23, 0.4);
    }
  `
})
export class TransactionFilterModalComponent {
  private readonly formBuilder = inject(FormBuilder);

  readonly categories = input<CategoryOption[]>([]);
  readonly currentFilters = input<TransactionFilter | null>(null);
  
  readonly closeRequested = output<void>();
  readonly filtersApplied = output<TransactionFilter>();

  readonly form = this.formBuilder.group({
    description: [''],
    categoryId: [''],
    dateFrom: [''],
    dateTo: [''],
    type: ['' as TransactionType | ''],
    minAmount: [null as number | null],
    maxAmount: [null as number | null]
  });

  constructor() {
    // Inicializar com filtros atuais se existirem
    const filters = this.currentFilters();
    if (filters) {
      this.form.patchValue(filters);
    }
  }

  resetForm(): void {
    this.form.reset({
      description: '',
      categoryId: '',
      dateFrom: '',
      dateTo: '',
      type: '',
      minAmount: null,
      maxAmount: null
    });
  }

  applyFilters(): void {
    const rawValue = this.form.getRawValue();
    
    // Limpar campos vazios para não enviar na query string desnecessariamente
    const filters: TransactionFilter = {};
    if (rawValue.description) filters.description = rawValue.description;
    if (rawValue.categoryId) filters.categoryId = rawValue.categoryId;
    if (rawValue.dateFrom) filters.dateFrom = rawValue.dateFrom;
    if (rawValue.dateTo) filters.dateTo = rawValue.dateTo;
    if (rawValue.type) filters.type = rawValue.type as TransactionType;
    if (rawValue.minAmount !== null) filters.minAmount = rawValue.minAmount;
    if (rawValue.maxAmount !== null) filters.maxAmount = rawValue.maxAmount;

    this.filtersApplied.emit(filters);
  }
}
