import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { dashboardSummaryMock } from '../../testing/frontend-test-data';
import { DashboardApiService } from './dashboard-api.service';

describe('DashboardApiService', () => {
  let service: DashboardApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), DashboardApiService],
    });

    service = TestBed.inject(DashboardApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should request dashboard summary with user header', () => {
    let responseValue = dashboardSummaryMock;

    service.getDashboard().subscribe((response) => {
      responseValue = response;
    });

    const request = httpMock.expectOne('/api/dashboard');
    expect(request.request.method).toBe('GET');
    expect(request.request.headers.get('X-User-Id')).toBe(
      '11111111-1111-1111-1111-111111111111',
    );

    request.flush({
      success: true,
      message: 'ok',
      data: dashboardSummaryMock,
      errors: [],
    });

    expect(responseValue).toEqual(dashboardSummaryMock);
  });
});
