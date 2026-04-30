import { CurrencyPipe, DatePipe, NgClass } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { TransactionHistoryEntry } from '../../../core/models/transaction-history.model';
import { FinTransaction } from '../../../core/models/transaction.model';
import { TransactionsApiService } from '../../../core/services/transactions-api.service';
import { CategoryBreakdownChartComponent } from '../components/category-breakdown-chart.component';
import { TransactionHistoryListComponent } from '../components/transaction-history-list.component';

@Component({
  selector: 'app-transaction-details-page',
  standalone: true,
  imports: [
    CategoryBreakdownChartComponent,
    CurrencyPipe,
    DatePipe,
    NgClass,
    RouterLink,
    TransactionHistoryListComponent
  ],
  template: `
    <div class="d-flex flex-column flex-lg-row justify-content-between gap-3 align-items-lg-center mb-4">
      <div>
        <p class="section-title mb-2">Detalhes da transacao</p>
        <h1 class="h2 mb-1">Auditoria e distribuicao por categoria</h1>
        <p class="text-muted-soft mb-0">
          Acompanhe o contexto do lancamento e sua evolucao historica.
        </p>
      </div>

      <div class="d-flex gap-2 flex-wrap">
        <button type="button" class="btn btn-outline-light" (click)="refresh()">
          <i class="bi bi-arrow-clockwise me-2"></i>
          Atualizar
        </button>
        <a routerLink="/transactions" class="btn btn-soft">
          <i class="bi bi-arrow-left me-2"></i>
          Voltar
        </a>
      </div>
    </div>

    @if (error(); as errorMessage) {
      <div class="alert alert-danger border-0 shadow-sm mb-4">{{ errorMessage }}</div>
    }

    @if (transaction(); as transactionData) {
      <section class="glass-panel rounded-4 p-4 mb-4">
        <div class="row g-4">
          <div class="col-12 col-lg-8">
            <p class="section-title mb-2">Transacao selecionada</p>
            <h2 class="h3 mb-2">{{ transactionData.description }}</h2>
            <p class="text-muted-soft mb-4">
              {{ transactionData.categoryName }} · {{ transactionData.transactionDateUtc | date: 'dd/MM/yyyy' }}
            </p>

            <div class="d-flex gap-3 flex-wrap">
              <div class="rounded-4 border border-secondary-subtle px-3 py-2">
                <small class="text-muted-soft d-block">Valor</small>
                <span class="fw-semibold" [ngClass]="transactionData.type === 'Income' ? 'text-success' : 'text-warning'">
                  {{ transactionData.amount | currency: 'BRL':'symbol':'1.2-2' }}
                </span>
              </div>
              <div class="rounded-4 border border-secondary-subtle px-3 py-2">
                <small class="text-muted-soft d-block">Tipo</small>
                <span class="fw-semibold">{{ transactionData.type === 'Income' ? 'Entrada' : 'Saida' }}</span>
              </div>
              <div class="rounded-4 border border-secondary-subtle px-3 py-2">
                <small class="text-muted-soft d-block">Criado em</small>
                <span class="fw-semibold">{{ transactionData.createdAtUtc | date: 'dd/MM/yyyy HH:mm' }}</span>
              </div>
            </div>
          </div>

          <div class="col-12 col-lg-4">
            <div class="rounded-4 border border-secondary-subtle p-4 h-100">
              <p class="section-title mb-2">Contexto</p>
              <div class="small text-muted-soft">
                Este painel destaca a categoria do lancamento dentro da distribuicao total das transacoes cadastradas.
              </div>
            </div>
          </div>
        </div>
      </section>

      <div class="row g-4">
        <div class="col-12 col-xl-5">
          <app-category-breakdown-chart
            [transactions]="allTransactions()"
            [activeCategoryId]="transactionData.categoryId"
          />
        </div>

        <div class="col-12 col-xl-7">
          <app-transaction-history-list [history]="history()" />
        </div>
      </div>
    } @else {
      <section class="glass-panel rounded-4 p-5 text-center text-muted-soft">
        Nenhuma transacao foi localizada para este identificador.
      </section>
    }
  `
})
export class TransactionDetailsPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly transactionsApi = inject(TransactionsApiService);
  private readonly refreshTick = signal(0);

  readonly transaction = signal<FinTransaction | null>(null);
  readonly allTransactions = signal<FinTransaction[]>([]);
  readonly history = signal<TransactionHistoryEntry[]>([]);
  readonly error = signal<string | null>(null);

  private readonly transactionId = computed(() => this.route.snapshot.paramMap.get('id') ?? '');

  constructor() {
    effect(() => {
      this.refreshTick();
      const transactionId = this.transactionId();
      if (transactionId) {
        void this.loadTransactionDetails(transactionId);
      }
    });
  }

  refresh(): void {
    this.refreshTick.update((current) => current + 1);
  }

  private async loadTransactionDetails(transactionId: string): Promise<void> {
    this.error.set(null);

    try {
      const [transaction, history, allTransactionsPaged] = await Promise.all([
        firstValueFrom(this.transactionsApi.getTransactionById(transactionId)),
        firstValueFrom(this.transactionsApi.getTransactionHistory(transactionId)),
        firstValueFrom(this.transactionsApi.getTransactions({ pageNumber: 1, pageSize: 100 }))
      ]);

      this.transaction.set(transaction);
      this.history.set(history);
      this.allTransactions.set(allTransactionsPaged.items);
    } catch {
      this.error.set('Nao foi possivel carregar os detalhes da transacao solicitada.');
      this.transaction.set(null);
      this.history.set([]);
      this.allTransactions.set([]);
    }
  }
}
