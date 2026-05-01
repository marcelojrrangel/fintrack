import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { DashboardSummaryCardsComponent } from '../components/dashboard-summary-cards.component';
import { BalanceEvolutionChartComponent } from '../components/balance-evolution-chart.component';
import { DashboardSummary, BalanceEvolutionPoint } from '../../../core/models/dashboard.model';
import { FinTransaction } from '../../../core/models/transaction.model';
import { DashboardApiService } from '../../../core/services/dashboard-api.service';
import { TransactionsApiService } from '../../../core/services/transactions-api.service';
import { CurrentUserService } from '../../../core/services/current-user.service';
import { UserPickerModalComponent } from '../../../core/components/user-picker-modal.component';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [BalanceEvolutionChartComponent, CurrencyPipe, DashboardSummaryCardsComponent, DatePipe, UserPickerModalComponent],
  template: `
      @if (!currentUserService.currentUser()) {
        <div class="glass-panel rounded-4 p-5 text-center mb-4">
          <i class="bi bi-person-circle display-4 text-muted-soft d-block mb-3"></i>
          <h2 class="h4 mb-2">Selecione um usuário</h2>
          <p class="text-muted-soft mb-0">
            Acesse a página de <strong>Transações</strong> e selecione um usuário para visualizar o dashboard.
          </p>
        </div>
      }

      @if (currentUserService.currentUser(); as activeUser) {
        <section class="glass-panel rounded-4 p-3 mb-4 border border-primary-subtle">
          <div class="d-flex align-items-center gap-3 flex-wrap">
            <div class="d-flex align-items-center justify-content-center rounded-circle bg-primary-subtle text-primary-emphasis" style="width: 40px; height: 40px;">
              <i class="bi bi-person-check"></i>
            </div>
            <div>
              <p class="section-title mb-1">Usuário ativo</p>
              <div class="fw-semibold">{{ activeUser.fullName }}</div>
              <small class="text-muted-soft">{{ activeUser.email }}</small>
            </div>
            <button type="button" class="btn btn-sm btn-outline-light ms-auto" (click)="isUserPickerOpen.set(true)">
              <i class="bi bi-person-gear me-2"></i>
              Trocar usuário
            </button>
          </div>
        </section>
      }

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
          <p class="section-title mb-2">Últimas movimentações</p>
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
               Nenhuma transação encontrada para exibir a atividade recente.
            </div>
          }
        </section>
      </div>
    </div>

    @if (isUserPickerOpen()) {
      <app-user-picker-modal
        (closeRequested)="isUserPickerOpen.set(false)"
        (userSelected)="isUserPickerOpen.set(false)"
      />
    }
  `
})
export class DashboardPageComponent {
  private readonly dashboardApi = inject(DashboardApiService);
  private readonly transactionsApi = inject(TransactionsApiService);
  readonly currentUserService = inject(CurrentUserService);
  private readonly refreshTick = signal(0);

  readonly dashboard = signal<DashboardSummary | null>(null);
  readonly transactions = signal<FinTransaction[]>([]);
  readonly error = signal<string | null>(null);
  readonly lastUpdated = signal<Date | null>(null);
  readonly isUserPickerOpen = signal(false);

  readonly recentTransactions = computed(() => this.transactions().slice(0, 5));
  readonly balanceEvolution = computed(() => this.toBalanceEvolution(this.transactions()));

  constructor() {
    effect(() => {
      this.refreshTick();
        // React to user changes
        this.currentUserService.currentUser();
        if (this.currentUserService.currentUser()) {
          void this.loadDashboard();
        } else {
          this.dashboard.set(null);
          this.transactions.set([]);
          this.lastUpdated.set(null);
          this.error.set(null);
        }
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
      this.error.set('Não foi possível carregar os dados do dashboard. Verifique se a API está em execução.');
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
