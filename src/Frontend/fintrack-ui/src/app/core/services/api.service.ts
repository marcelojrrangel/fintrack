import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject } from '@angular/core';
import { map, Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { CurrentUserService } from './current-user.service';

export abstract class ApiService {
  protected readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;
  private readonly currentUserService = inject(CurrentUserService);

  protected buildUrl(path: string): string {
    return `${this.baseUrl}/${path}`;
  }

  protected requestOptions() {
    const userId = this.currentUserService.currentUser()?.id ?? environment.defaultUserId;
    return {
      headers: new HttpHeaders({
        'X-User-Id': userId
      })
    };
  }

  protected unwrap<T>(source$: Observable<ApiResponse<T>>): Observable<T> {
    return source$.pipe(map((response) => response.data));
  }
}
