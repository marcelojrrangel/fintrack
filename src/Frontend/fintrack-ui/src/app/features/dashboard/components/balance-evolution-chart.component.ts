import { Component, computed, input } from '@angular/core';
import { ChartConfiguration } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';

import { BalanceEvolutionPoint } from '../../../core/models/dashboard.model';

@Component({
  selector: 'app-balance-evolution-chart',
  standalone: true,
  imports: [BaseChartDirective],
  template: `
    <section class="glass-panel rounded-4 p-4 chart-panel h-100">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <p class="section-title mb-2">Evolucao do saldo</p>
          <h2 class="h4 mb-0">Saldo por periodo</h2>
        </div>
        <span class="badge rounded-pill text-bg-dark border border-secondary-subtle">Chart.js</span>
      </div>

      @if (points().length > 0) {
        <canvas baseChart [type]="'line'" [data]="chartData()" [options]="chartOptions"></canvas>
      } @else {
        <div class="rounded-4 border border-secondary-subtle p-5 text-center text-muted-soft">
          Nenhum dado suficiente para desenhar a evolucao do saldo.
        </div>
      }
    </section>
  `
})
export class BalanceEvolutionChartComponent {
  readonly points = input<BalanceEvolutionPoint[]>([]);

  readonly chartData = computed<ChartConfiguration<'line'>['data']>(() => ({
    labels: this.points().map((point) => point.label),
    datasets: [
      {
        label: 'Saldo acumulado',
        data: this.points().map((point) => point.balance),
        borderColor: '#818cf8',
        backgroundColor: 'rgba(129, 140, 248, 0.18)',
        fill: true,
        tension: 0.32,
        pointBackgroundColor: '#818cf8',
        pointBorderColor: '#1e1b4b',
        pointRadius: 4
      },
      {
        label: 'Entradas',
        data: this.points().map((point) => point.income),
        borderColor: '#22c55e',
        backgroundColor: 'rgba(34, 197, 94, 0.12)',
        fill: false,
        tension: 0.32,
        pointBackgroundColor: '#22c55e',
        pointBorderColor: '#1e1b4b',
        pointRadius: 4
      },
      {
        label: 'Saídas',
        data: this.points().map((point) => point.expense),
        borderColor: '#f59e0b',
        backgroundColor: 'rgba(245, 158, 11, 0.12)',
        fill: false,
        tension: 0.32,
        pointBackgroundColor: '#f59e0b',
        pointBorderColor: '#1e1b4b',
        pointRadius: 4
      }
    ]
  }));

  readonly chartOptions: ChartConfiguration<'line'>['options'] = {
    maintainAspectRatio: false,
    responsive: true,
    plugins: {
      legend: {
        display: true,
        labels: {
          color: '#94a3b8',
          usePointStyle: true,
          padding: 20
        }
      }
    },
    scales: {
      x: {
        ticks: { color: '#94a3b8' },
        grid: { display: false }
      },
      y: {
        ticks: { color: '#94a3b8' },
        grid: { color: 'rgba(148, 163, 184, 0.12)' }
      }
    }
  };
}
