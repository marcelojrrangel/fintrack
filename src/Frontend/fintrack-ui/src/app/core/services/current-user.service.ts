import { Injectable, signal } from '@angular/core';

import { AppUser } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class CurrentUserService {
  readonly currentUser = signal<AppUser | null>(null);

  select(user: AppUser): void {
    this.currentUser.set(user);
  }

  clear(): void {
    this.currentUser.set(null);
  }
}
