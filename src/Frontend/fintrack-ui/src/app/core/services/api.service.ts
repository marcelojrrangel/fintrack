import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject } from '@angular/core';
import { map, Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

export abstract class ApiService {
  protected readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;
  private readonly defaultUserId = environment.defaultUserId;

  protected buildUrl(path: string): string {
    return `${this.baseUrl}/${path}`;
  }

  protected requestOptions() {
    return {
      headers: new HttpHeaders({
        'X-User-Id': this.defaultUserId
      })
    };
  }

  protected unwrap<T>(source$: Observable<ApiResponse<T>>): Observable<T> {
    return source$.pipe(map((response) => response.data));
  }
}
