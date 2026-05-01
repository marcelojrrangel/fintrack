import { CurrencyPipe, DatePipe, NgClass } from '@angular/common';
import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { FinTransaction } from '../../../core/models/transaction.model';

@Component({
  selector: 'app-transaction-table',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, NgClass, RouterLink],
  template: `
    <section class="glass-panel rounded-4 p-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <p class="section-title mb-2">Lista</p>
          <h2 class="h4 mb-0">Transações registradas</h2>
        </div>
        <span class="badge rounded-pill text-bg-dark border border-secondary-subtle">{{ transactions().length }} itens</span>
      </div>

      @if (transactions().length > 0) {
        <div class="table-responsive">
          <table class="table table-dark align-middle mb-0">
            <thead>
              <tr>
                <th>Descrição</th>
                <th>Categoria</th>
                <th>Data</th>
                <th>Tipo</th>
                <th class="text-end">Valor</th>
                <th class="text-end">Ações</th>
              </tr>
            </thead>
            <tbody>
              @for (transaction of transactions(); track transaction.id) {
                <tr>
                  <td>
                    <div class="fw-semibold">{{ transaction.description }}</div>
                    <small class="text-muted-soft">ID {{ transaction.id.slice(0, 8) }}</small>
                  </td>
                  <td>{{ transaction.categoryName }}</td>
                  <td>{{ transaction.transactionDateUtc | date: 'dd/MM/yyyy' }}</td>
                  <td>
                    <span class="badge rounded-pill" [ngClass]="transaction.type === 'Income' ? 'text-bg-success' : 'text-bg-warning'">
                      {{ transaction.type === 'Income' ? 'Entrada' : 'Saida' }}
                    </span>
                  </td>
                  <td class="text-end fw-semibold" [class.text-success]="transaction.type === 'Income'" [class.text-warning]="transaction.type === 'Expense'">
                    {{ transaction.type === 'Income' ? '+' : '-' }}{{ transaction.amount | currency: 'BRL':'symbol':'1.2-2' }}
                  </td>
                  <td class="text-end">
                    <a class="btn btn-sm btn-soft" [routerLink]="['/transactions', transaction.id]">
                      <i class="bi bi-box-arrow-up-right me-2"></i>
                      Detalhes
                    </a>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      } @else {
        <div class="rounded-4 border border-secondary-subtle p-5 text-center text-muted-soft">
          Nenhuma transação corresponde ao filtro selecionado.
        </div>
      }
    </section>
  `
})
export class TransactionTableComponent {
  readonly transactions = input<FinTransaction[]>([]);
}
