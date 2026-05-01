import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';

import { TransactionHistoryEntry } from '../../../core/models/transaction-history.model';

@Component({
  selector: 'app-transaction-history-list',
  standalone: true,
  imports: [DatePipe],
  template: `
    <section class="glass-panel rounded-4 p-4 h-100">
      <p class="section-title mb-2">Auditoria</p>
      <h2 class="h4 mb-4">Histórico da transação</h2>

      @if (history().length > 0) {
        <div class="d-flex flex-column gap-3">
          @for (entry of history(); track entry.id) {
            <article class="rounded-4 border border-secondary-subtle p-3">
              <div class="d-flex justify-content-between gap-3 flex-wrap">
                <div>
                  <div class="fw-semibold">{{ entry.description }}</div>
                  <small class="text-muted-soft">{{ entry.occurredAtUtc | date: 'dd/MM/yyyy HH:mm' }}</small>
                </div>

                <span class="badge rounded-pill text-bg-dark border border-secondary-subtle">
                  {{ entry.action }}
                </span>
              </div>

              @if (entry.previousValues || entry.currentValues) {
                <div class="row g-2 mt-3">
                  <div class="col-12 col-lg-6">
                    <div class="rounded-3 bg-black bg-opacity-25 p-3 h-100">
                      <small class="text-muted-soft d-block mb-2">Estado anterior</small>
                      <pre class="mb-0 small text-wrap text-break">{{ entry.previousValues || 'Sem alterações anteriores.' }}</pre>
                    </div>
                  </div>
                  <div class="col-12 col-lg-6">
                    <div class="rounded-3 bg-black bg-opacity-25 p-3 h-100">
                      <small class="text-muted-soft d-block mb-2">Estado atual</small>
                      <pre class="mb-0 small text-wrap text-break">{{ entry.currentValues || 'Sem snapshot atual.' }}</pre>
                    </div>
                  </div>
                </div>
              }
            </article>
          }
        </div>
      } @else {
        <div class="rounded-4 border border-secondary-subtle p-5 text-center text-muted-soft">
          Nenhum evento de auditoria encontrado para esta transação.
        </div>
      }
    </section>
  `
})
export class TransactionHistoryListComponent {
  readonly history = input<TransactionHistoryEntry[]>([]);
}
