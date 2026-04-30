import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiResponse } from '../models/api-response.model';
import { DashboardSummary } from '../models/dashboard.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class DashboardApiService extends ApiService {
  getDashboard(): Observable<DashboardSummary> {
    return this.unwrap(
      this.http.get<ApiResponse<DashboardSummary>>(this.buildUrl('dashboard'), this.requestOptions())
    );
  }
}
