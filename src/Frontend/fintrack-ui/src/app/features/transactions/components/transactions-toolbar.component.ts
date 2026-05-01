import { Component, inject, input, output } from '@angular/core';

import { CategoryOption } from '../../../core/models/category.model';
import { CurrentUserService } from '../../../core/services/current-user.service';

@Component({
  selector: 'app-transactions-toolbar',
  standalone: true,
  template: `
    <section class="glass-panel rounded-4 p-4 mb-4">
      <div class="d-flex flex-column gap-4">
        <div>
          <p class="section-title mb-2">Gestão de transações</p>
          <h1 class="h2 mb-1">Lançamentos</h1>
          <p class="text-muted-soft mb-0">
            Filtre por categoria, crie novos registros e navegue ate os detalhes.
          </p>
        </div>

          <div class="d-flex align-items-center gap-3 py-2 px-3 rounded-4 border user-context-bar"
               [class.border-primary]="currentUserService.currentUser()"
               [class.border-secondary-subtle]="!currentUserService.currentUser()">
            @if (currentUserService.currentUser(); as user) {
              <div class="avatar-circle-sm flex-shrink-0 d-flex align-items-center justify-content-center rounded-circle">
                <span class="fw-semibold small">{{ initials(user.fullName) }}</span>
              </div>
              <div class="overflow-hidden flex-grow-1">
                <div class="fw-semibold text-truncate small">{{ user.fullName }}</div>
                <small class="text-muted-soft text-truncate d-block" style="font-size: 0.75rem;">{{ user.email }}</small>
              </div>
            } @else {
              <i class="bi bi-person-circle text-muted-soft fs-5"></i>
              <span class="text-muted-soft small">Nenhum usuário selecionado. Selecione um usuário para visualizar os dados.</span>
            }
            <button
              type="button"
              class="btn btn-sm flex-shrink-0 ms-auto"
              [class.btn-outline-primary]="currentUserService.currentUser()"
              [class.btn-outline-light]="!currentUserService.currentUser()"
              (click)="selectUserRequested.emit()"
            >
              <i class="bi bi-person-gear me-1"></i>
              {{ currentUserService.currentUser() ? 'Trocar usuário' : 'Selecionar usuário' }}
            </button>
          </div>

          <div class="d-flex flex-wrap gap-2 align-items-center">
          <div class="position-relative" style="min-width: 280px;">
            <i class="bi bi-search position-absolute top-50 start-0 translate-middle-y ms-3 text-muted"></i>
            <input
              type="text"
              class="form-control bg-transparent border-secondary-subtle ps-5"
              placeholder="Buscar por descrição..."
              [value]="searchTerm()"
              (input)="onSearchChange($event)"
            />
          </div>

          <input
            type="date"
            class="form-control bg-transparent border-secondary-subtle"
            style="width: 160px;"
            [value]="selectedDate()"
            (change)="onDateChange($event)"
          />

          <select class="form-select border-secondary-subtle" style="width: 180px;" [value]="selectedCategoryId()" (change)="onCategoryChange($event)">
            <option value="all">Categorias</option>
            @for (category of categories(); track category.id) {
              <option [value]="category.id">{{ category.name }}</option>
            }
          </select>
        </div>

        <div class="d-flex gap-2">
          <button type="button" class="btn btn-outline-light" (click)="refreshRequested.emit()" title="Atualizar">
            <i class="bi bi-arrow-repeat"></i>
          </button>

          <button type="button" class="btn btn-soft px-3" (click)="filterRequested.emit()" [class.btn-primary]="hasActiveFilters()">
            <i class="bi bi-filter me-2"></i>
            Filtros
          </button>

          <button type="button" class="btn btn-primary px-4" (click)="createRequested.emit()">
            <i class="bi bi-plus-circle me-2"></i>
            Nova Transação
          </button>
        </div>
      </div>
    </section>
  `,
  styles: `
    .user-context-bar {
      background: rgba(15, 23, 42, 0.4);
      transition: border-color 0.2s;
    }

    .avatar-circle-sm {
      width: 36px;
      height: 36px;
      background: rgba(129, 140, 248, 0.2);
      color: #818cf8;
    }
  `
})
export class TransactionsToolbarComponent {
  readonly currentUserService = inject(CurrentUserService);

  readonly categories = input<CategoryOption[]>([]);
  readonly selectedCategoryId = input('all');
  readonly searchTerm = input('');
  readonly selectedDate = input('');
  readonly hasActiveFilters = input(false);

  readonly categoryChanged = output<string>();
  readonly searchChanged = output<string>();
  readonly dateChanged = output<string>();
  readonly createRequested = output<void>();
  readonly filterRequested = output<void>();
  readonly refreshRequested = output<void>();
  readonly selectUserRequested = output<void>();

  initials(fullName: string): string {
    return fullName
      .split(' ')
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  onCategoryChange(event: Event): void {
    const categoryId = (event.target as HTMLSelectElement).value;
    this.categoryChanged.emit(categoryId);
  }

  onSearchChange(event: Event): void {
    const term = (event.target as HTMLInputElement).value;
    this.searchChanged.emit(term);
  }

  onDateChange(event: Event): void {
    const date = (event.target as HTMLInputElement).value;
    this.dateChanged.emit(date);
  }
}
