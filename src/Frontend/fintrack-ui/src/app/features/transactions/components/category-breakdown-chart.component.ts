import { Component, computed, input } from '@angular/core';
import { ChartConfiguration } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';

import { FinTransaction } from '../../../core/models/transaction.model';

@Component({
  selector: 'app-category-breakdown-chart',
  standalone: true,
  imports: [BaseChartDirective],
  template: `
    <section class="glass-panel rounded-4 p-4 chart-panel h-100">
      <p class="section-title mb-2">Analise por categoria</p>
      <h2 class="h4 mb-4">Distribuicao de valores</h2>

      @if (transactions().length > 0) {
        <canvas baseChart [type]="'doughnut'" [data]="chartData()" [options]="chartOptions"></canvas>
      } @else {
        <div class="rounded-4 border border-secondary-subtle p-5 text-center text-muted-soft">
          Nenhum dado disponivel para montar o grafico por categoria.
        </div>
      }
    </section>
  `
})
export class CategoryBreakdownChartComponent {
  readonly transactions = input<FinTransaction[]>([]);
  readonly activeCategoryId = input<string | null>(null);

  readonly chartData = computed<ChartConfiguration<'doughnut'>['data']>(() => {
    const grouped = new Map<string, { label: string; total: number; highlighted: boolean }>();

    for (const transaction of this.transactions()) {
      const entry = grouped.get(transaction.categoryId) ?? {
        label: transaction.categoryName,
        total: 0,
        highlighted: false
      };

      entry.total += transaction.amount;
      entry.highlighted ||= transaction.categoryId === this.activeCategoryId();
      grouped.set(transaction.categoryId, entry);
    }

    const entries = [...grouped.values()];

    return {
      labels: entries.map((entry) => entry.label),
      datasets: [
        {
          data: entries.map((entry) => entry.total),
          backgroundColor: entries.map((entry) => (entry.highlighted ? '#818cf8' : '#334155')),
          borderColor: entries.map((entry) => (entry.highlighted ? '#c7d2fe' : '#0f172a')),
          borderWidth: 2
        }
      ]
    };
  });

  readonly chartOptions: ChartConfiguration<'doughnut'>['options'] = {
    maintainAspectRatio: false,
    responsive: true,
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          color: '#cbd5e1'
        }
      }
    }
  };
}
