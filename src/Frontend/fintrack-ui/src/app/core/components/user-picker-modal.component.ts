import { Component, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

import { AppUser } from '../models/user.model';
import { CurrentUserService } from '../services/current-user.service';
import { UsersApiService } from '../services/users-api.service';

@Component({
  selector: 'app-user-picker-modal',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="modal-backdrop fade show"></div>
    <div class="modal d-block" tabindex="-1" role="dialog" aria-modal="true">
      <div class="modal-dialog modal-dialog-centered modal-md">
        <div class="modal-content glass-panel border-0 rounded-4">

          <div class="modal-header border-secondary-subtle">
            <div>
              <p class="section-title mb-2">Contexto de usuário</p>
              <h2 class="h4 mb-0">Selecionar usuário</h2>
            </div>
            <button
              type="button"
              class="btn-close btn-close-white"
              aria-label="Fechar"
              (click)="closeRequested.emit()"
            ></button>
          </div>

          <div class="modal-body">
            <div class="mb-3 position-relative">
              <i class="bi bi-search position-absolute top-50 start-0 translate-middle-y ms-3 text-muted"></i>
              <input
                type="text"
                class="form-control bg-transparent border-secondary-subtle ps-5"
                placeholder="Buscar por nome ou e-mail..."
                [(ngModel)]="searchTerm"
                (ngModelChange)="onSearchChange($event)"
              />
            </div>

            @if (loading()) {
              <div class="text-center py-4 text-muted-soft">
                <span class="spinner-border spinner-border-sm me-2"></span>
                Buscando usuários...
              </div>
            } @else if (!hasSearched()) {
              <div class="rounded-4 border border-secondary-subtle p-4 text-center text-muted-soft">
                Digite um nome ou e-mail para buscar usuários.
              </div>
            } @else if (users().length === 0) {
              <div class="rounded-4 border border-secondary-subtle p-4 text-center text-muted-soft">
                @if (searchTerm.length > 0) {
                  Nenhum usuário encontrado para <strong>"{{ searchTerm }}"</strong>.
                } @else {
                  Nenhum usuário disponível.
                }
              </div>
            } @else {
              <div class="d-flex flex-column gap-2">
                @for (user of users(); track user.id) {
                  <button
                    type="button"
                    class="btn text-start rounded-4 border px-4 py-3 user-option"
                    [class.user-option--selected]="selectedUser()?.id === user.id"
                    (click)="selectUser(user)"
                  >
                    <div class="d-flex align-items-center gap-3">
                      <div
                        class="avatar-circle flex-shrink-0 d-flex align-items-center justify-content-center rounded-circle"
                      >
                        <span class="fw-semibold">{{ initials(user.fullName) }}</span>
                      </div>
                      <div class="overflow-hidden">
                        <div class="fw-semibold text-truncate">{{ user.fullName }}</div>
                        <small class="text-muted-soft text-truncate d-block">{{ user.email }}</small>
                      </div>
                      @if (selectedUser()?.id === user.id) {
                        <i class="bi bi-check-circle-fill text-primary ms-auto flex-shrink-0"></i>
                      }
                    </div>
                  </button>
                }
              </div>
            }
          </div>

          <div class="modal-footer border-secondary-subtle">
            <button type="button" class="btn btn-outline-light" (click)="closeRequested.emit()">Cancelar</button>
            <button
              type="button"
              class="btn btn-primary px-4"
              [disabled]="!selectedUser()"
              (click)="confirm()"
            >
              <i class="bi bi-person-check me-2"></i>
              Confirmar seleção
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

    .user-option {
      background: rgba(15, 23, 42, 0.5);
      border-color: rgba(148, 163, 184, 0.16) !important;
      color: inherit;
      transition: border-color 0.15s, background 0.15s;
    }

    .user-option:hover {
      background: rgba(129, 140, 248, 0.1);
      border-color: rgba(129, 140, 248, 0.4) !important;
    }

    .user-option--selected {
      background: rgba(129, 140, 248, 0.15) !important;
      border-color: rgba(129, 140, 248, 0.6) !important;
    }

    .avatar-circle {
      width: 40px;
      height: 40px;
      background: rgba(129, 140, 248, 0.2);
      color: #818cf8;
      font-size: 0.85rem;
    }
  `
})
export class UserPickerModalComponent {
  private readonly usersApi = inject(UsersApiService);
  private readonly currentUserService = inject(CurrentUserService);

  readonly closeRequested = output<void>();
  readonly userSelected = output<AppUser>();

  readonly users = signal<AppUser[]>([]);
  readonly loading = signal(false);
  readonly selectedUser = signal<AppUser | null>(null);
  readonly hasSearched = signal(false);

  searchTerm = '';
  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    // Pre-select current user if already set
    const current = this.currentUserService.currentUser();
    if (current) {
      this.selectedUser.set(current);
    }
  }

  onSearchChange(value: string): void {
    if (this.searchTimer) clearTimeout(this.searchTimer);

    const term = value.trim();
    if (!term) {
      this.hasSearched.set(false);
      this.users.set([]);
      return;
    }

    this.searchTimer = setTimeout(() => void this.loadUsers(term), 300);
  }

  selectUser(user: AppUser): void {
    this.selectedUser.set(user);
  }

  confirm(): void {
    const user = this.selectedUser();
    if (!user) return;
    this.currentUserService.select(user);
    this.userSelected.emit(user);
    this.closeRequested.emit();
  }

  initials(fullName: string): string {
    return fullName
      .split(' ')
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  private async loadUsers(search?: string): Promise<void> {
    this.hasSearched.set(true);
    this.loading.set(true);
    try {
      const users = await firstValueFrom(this.usersApi.getUsers(search));
      this.users.set(users);
    } finally {
      this.loading.set(false);
    }
  }
}
