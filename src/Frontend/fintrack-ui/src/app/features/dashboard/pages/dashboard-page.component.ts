import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { DashboardSummaryCardsComponent } from '../components/dashboard-summary-cards.component';
import { BalanceEvolutionChartComponent } from '../components/balance-evolution-chart.component';
import { DashboardSummary, BalanceEvolutionPoint } from '../../../core/models/dashboard.model';
import { FinTransaction } from '../../../core/models/transaction.model';
import { DashboardApiService } from '../../../core/services/dashboard-api.service';
import { TransactionsApiService } from '../../../core/services/transactions-api.service';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [BalanceEvolutionChartComponent, CurrencyPipe, DashboardSummaryCardsComponent, DatePipe],
  template: `
    <app-dashboard-summary-cards
      [dashboard]="dashboard()"
      [lastUpdated]="lastUpdated()"
      (refreshRequested)="refresh()"
    />

    @if (error(); as errorMessage) {
      <div class="alert alert-danger border-0 shadow-sm mb-4">{{ errorMessage }}</div>
    }

    <div class="row g-4">
      <div class="col-12 col-xl-8">
        <app-balance-evolution-chart [points]="balanceEvolution()" />
      </div>

      <div class="col-12 col-xl-4">
        <section class="glass-panel rounded-4 p-4 h-100">
          <p class="section-title mb-2">Ultimas movimentacoes</p>
          <h2 class="h4 mb-4">Atividade recente</h2>

          @if (recentTransactions().length > 0) {
            <div class="d-flex flex-column gap-3">
              @for (transaction of recentTransactions(); track transaction.id) {
                <article class="rounded-4 border border-secondary-subtle p-3">
                  <div class="d-flex justify-content-between align-items-start gap-3">
                    <div>
                      <div class="fw-semibold">{{ transaction.description }}</div>
                      <small class="text-muted-soft">
                        {{ transaction.categoryName }} · {{ transaction.transactionDateUtc | date: 'dd/MM/yyyy' }}
                      </small>
                    </div>
                    <div class="text-end">
                      <div class="fw-semibold" [class.text-success]="transaction.type === 'Income'" [class.text-warning]="transaction.type === 'Expense'">
                        {{ transaction.type === 'Income' ? '+' : '-' }}{{ transaction.amount | currency: 'BRL':'symbol':'1.2-2' }}
                      </div>
                    </div>
                  </div>
                </article>
              }
            </div>
          } @else {
            <div class="rounded-4 border border-secondary-subtle p-5 text-center text-muted-soft">
              Nenhuma transacao encontrada para exibir a atividade recente.
            </div>
          }
        </section>
      </div>
    </div>
  `
})
export class DashboardPageComponent {
  private readonly dashboardApi = inject(DashboardApiService);
  private readonly transactionsApi = inject(TransactionsApiService);
  private readonly refreshTick = signal(0);

  readonly dashboard = signal<DashboardSummary | null>(null);
  readonly transactions = signal<FinTransaction[]>([]);
  readonly error = signal<string | null>(null);
  readonly lastUpdated = signal<Date | null>(null);

  readonly recentTransactions = computed(() => this.transactions().slice(0, 5));
  readonly balanceEvolution = computed(() => this.toBalanceEvolution(this.transactions()));

  constructor() {
    effect(() => {
      this.refreshTick();
      void this.loadDashboard();
    });
  }

  refresh(): void {
    this.refreshTick.update((current) => current + 1);
  }

  private async loadDashboard(): Promise<void> {
    this.error.set(null);

    try {
      const [dashboard, transactionsPaged] = await Promise.all([
        firstValueFrom(this.dashboardApi.getDashboard()),
        firstValueFrom(this.transactionsApi.getTransactions({ pageNumber: 1, pageSize: 50 }))
      ]);

      this.dashboard.set(dashboard);
      this.transactions.set(transactionsPaged.items);
      this.lastUpdated.set(new Date());
    } catch {
      this.error.set('Nao foi possivel carregar os dados do dashboard. Verifique se a API esta em execucao.');
    }
  }

  private toBalanceEvolution(transactions: FinTransaction[]): BalanceEvolutionPoint[] {
    const sortedTransactions = [...transactions].sort(
      (left, right) =>
        new Date(left.transactionDateUtc).getTime() - new Date(right.transactionDateUtc).getTime()
    );

    let runningBalance = 0;

    return sortedTransactions.map((transaction) => {
      const income = transaction.type === 'Income' ? transaction.amount : 0;
      const expense = transaction.type === 'Expense' ? transaction.amount : 0;

      runningBalance += income - expense;

      return {
        label: new Intl.DateTimeFormat('pt-BR', {
          day: '2-digit',
          month: '2-digit'
        }).format(new Date(transaction.transactionDateUtc)),
        balance: runningBalance,
        income,
        expense
      };
    });
  }
}
