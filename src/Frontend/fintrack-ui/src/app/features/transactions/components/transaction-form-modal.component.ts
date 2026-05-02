import { Component, effect, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { CategoryOption } from '../../../core/models/category.model';
import { TransactionMutationPayload, TransactionType } from '../../../core/models/transaction.model';

@Component({
  selector: 'app-transaction-form-modal',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="modal-backdrop fade show"></div>
    <div class="modal d-block" tabindex="-1" role="dialog" aria-modal="true">
      <div class="modal-dialog modal-dialog-centered modal-lg">
        <div class="modal-content glass-panel border-0 rounded-4">
          <div class="modal-header border-secondary-subtle">
            <div>
              <p class="section-title mb-2">Nova transação</p>
              <h2 class="h4 mb-0">Registrar movimentação</h2>
            </div>
            <button type="button" class="btn-close btn-close-white" aria-label="Fechar" (click)="closeRequested.emit()"></button>
          </div>

          <form class="modal-body" [formGroup]="form" (ngSubmit)="submit()">
            <div class="row g-3">
              <div class="col-12 col-md-6">
                <label class="form-label">Categoria</label>
                <select class="form-select" formControlName="categoryId">
                  @for (category of categories(); track category.id) {
                    <option [value]="category.id">{{ category.name }}</option>
                  }
                </select>
              </div>

              <div class="col-12 col-md-6">
                <label class="form-label">Tipo</label>
                <select class="form-select" formControlName="type">
                  @for (option of transactionTypes; track option) {
                    <option [value]="option">{{ option === 'Income' ? 'Entrada' : 'Saida' }}</option>
                  }
                </select>
              </div>

              <div class="col-12 col-md-6">
                <label class="form-label">Valor</label>
                <input type="number" min="0.01" step="0.01" class="form-control" formControlName="amount" />
              </div>

              <div class="col-12 col-md-6">
                <label class="form-label">Data</label>
                <input type="date" class="form-control" formControlName="transactionDateUtc" />
              </div>

              <div class="col-12">
                <label class="form-label">Descrição</label>
                <input type="text" class="form-control" maxlength="250" formControlName="description" placeholder="Ex.: Salário de abril" />
              </div>
            </div>

            @if (errorMessage(); as errorMessageText) {
              <div class="alert alert-danger border-0 mt-4 mb-0">{{ errorMessageText }}</div>
            }
          </form>

          <div class="modal-footer border-secondary-subtle">
            <button type="button" class="btn btn-outline-light" (click)="closeRequested.emit()">Cancelar</button>
            <button type="button" class="btn btn-primary" [disabled]="form.invalid || submitting()" (click)="submit()">
              @if (submitting()) {
                <span class="spinner-border spinner-border-sm me-2"></span>
              } @else {
                <i class="bi bi-check2-circle me-2"></i>
              }
              Salvar transação
            </button>
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
export class TransactionFormModalComponent {
  private readonly formBuilder = inject(FormBuilder);

  readonly categories = input<CategoryOption[]>([]);
  readonly errorMessage = input<string | null>(null);
  readonly submitting = input(false);
  readonly closeRequested = output<void>();
  readonly saveRequested = output<TransactionMutationPayload>();

  readonly transactionTypes: TransactionType[] = ['Income', 'Expense'];

  readonly form = this.formBuilder.nonNullable.group({
    categoryId: ['', Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    transactionDateUtc: [new Date().toISOString().slice(0, 10), Validators.required],
    type: ['Income' as TransactionType, Validators.required],
    description: ['', [Validators.required, Validators.maxLength(250)]]
  });

  constructor() {
    effect(() => {
      const categories = this.categories();
      if (categories.length > 0 && !this.form.controls.categoryId.value) {
        this.form.controls.categoryId.setValue(categories[0].id);
      }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const rawValue = this.form.getRawValue();
    this.saveRequested.emit({
      categoryId: rawValue.categoryId,
      amount: Number(rawValue.amount),
      transactionDateUtc: `${rawValue.transactionDateUtc}T00:00:00Z`,
      type: rawValue.type,
      description: rawValue.description.trim()
    });
  }
}
