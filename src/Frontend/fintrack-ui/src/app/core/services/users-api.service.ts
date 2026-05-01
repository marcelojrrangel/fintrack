import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AppUser } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UsersApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  getUsers(search?: string): Observable<AppUser[]> {
    const params: Record<string, string> = {};
    if (search?.trim()) {
      params['search'] = search.trim();
    }

    return this.http
      .get<ApiResponse<AppUser[]>>(`${this.baseUrl}/users`, { params })
      .pipe(map((response) => response.data ?? []));
  }
}
