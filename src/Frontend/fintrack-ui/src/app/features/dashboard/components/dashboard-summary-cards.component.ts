import { CurrencyPipe, DatePipe, NgClass } from '@angular/common';
import { Component, input, output } from '@angular/core';

import { DashboardSummary } from '../../../core/models/dashboard.model';

@Component({
  selector: 'app-dashboard-summary-cards',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, NgClass],
  template: `
    <section class="glass-panel rounded-4 p-4 mb-4">
      <div class="d-flex flex-column flex-lg-row justify-content-between gap-3 mb-4">
        <div>
          <p class="section-title mb-2">Visao geral</p>
          <h1 class="h2 mb-1">Resumo financeiro</h1>
          <p class="text-muted-soft mb-0">
            @if (lastUpdated(); as updatedAt) {
              Atualizado em {{ updatedAt | date: 'dd/MM/yyyy HH:mm' }}
            } @else {
              Sincronize os dados para atualizar os indicadores.
            }
          </p>
        </div>

        <button type="button" class="btn btn-outline-light align-self-start" (click)="refreshRequested.emit()">
          <i class="bi bi-arrow-clockwise me-2"></i>
          Atualizar
        </button>
      </div>

      <div class="row g-3">
        @if (dashboard(); as summary) {
          <div class="col-12 col-md-4">
            <article class="summary-card rounded-4 p-4 h-100" [ngClass]="summary.cardColor === 'red' ? 'summary-card--negative' : 'summary-card--positive'">
              <p class="section-title mb-3">Saldo atual</p>
              <h2 class="display-6 fw-semibold mb-2">{{ summary.currentBalance | currency: 'BRL':'symbol':'1.2-2' }}</h2>
              <span class="badge rounded-pill" [ngClass]="summary.cardColor === 'red' ? 'text-bg-danger' : 'text-bg-success'">
                {{ summary.cardColor === 'red' ? 'Em alerta' : 'Saudavel' }}
              </span>
            </article>
          </div>

          <div class="col-12 col-md-4">
            <article class="summary-card summary-card--income rounded-4 p-4 h-100">
              <p class="section-title mb-3">Entradas do mes</p>
              <h2 class="display-6 fw-semibold mb-2">{{ summary.totalIncomeMonth | currency: 'BRL':'symbol':'1.2-2' }}</h2>
              <p class="text-muted-soft mb-0">Valores confirmados no periodo atual.</p>
            </article>
          </div>

          <div class="col-12 col-md-4">
            <article class="summary-card summary-card--expense rounded-4 p-4 h-100">
              <p class="section-title mb-3">Saidas do mes</p>
              <h2 class="display-6 fw-semibold mb-2">{{ summary.totalExpenseMonth | currency: 'BRL':'symbol':'1.2-2' }}</h2>
              <p class="text-muted-soft mb-0">Despesas registradas no periodo atual.</p>
            </article>
          </div>
        } @else {
          @for (placeholder of [1, 2, 3]; track placeholder) {
            <div class="col-12 col-md-4">
              <div class="summary-card rounded-4 p-4 h-100 placeholder-glow">
                <span class="placeholder col-4 mb-4"></span>
                <span class="placeholder col-8 placeholder-lg d-block mb-3"></span>
                <span class="placeholder col-5"></span>
              </div>
            </div>
          }
        }
      </div>
    </section>
  `,
  styles: `
    .summary-card {
      background: rgba(15, 23, 42, 0.72);
      border: 1px solid rgba(148, 163, 184, 0.16);
    }

    .summary-card--positive {
      box-shadow: inset 0 0 0 1px rgba(34, 197, 94, 0.22);
      background: linear-gradient(180deg, rgba(34, 197, 94, 0.16), rgba(15, 23, 42, 0.78));
    }

    .summary-card--negative {
      box-shadow: inset 0 0 0 1px rgba(239, 68, 68, 0.22);
      background: linear-gradient(180deg, rgba(239, 68, 68, 0.18), rgba(15, 23, 42, 0.78));
    }

    .summary-card--income {
      background: linear-gradient(180deg, rgba(56, 189, 248, 0.16), rgba(15, 23, 42, 0.78));
    }

    .summary-card--expense {
      background: linear-gradient(180deg, rgba(245, 158, 11, 0.16), rgba(15, 23, 42, 0.78));
    }
  `
})
export class DashboardSummaryCardsComponent {
  readonly dashboard = input<DashboardSummary | null>(null);
  readonly lastUpdated = input<Date | null>(null);
  readonly refreshRequested = output<void>();
}
