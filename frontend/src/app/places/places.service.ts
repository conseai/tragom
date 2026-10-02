import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { HealthStatus, Place } from './place.model';

/**
 * All calls go to relative /api/... URLs.
 * In development the Angular dev server proxies them to the .NET API (see proxy.conf.json),
 * in production nginx does the same (see nginx.conf). No CORS setup needed.
 */
@Injectable({ providedIn: 'root' })
export class PlacesService {
  private readonly http = inject(HttpClient);

  health(): Observable<HealthStatus> {
    return this.http.get<HealthStatus>('/api/health');
  }

  all(): Observable<Place[]> {
    return this.http.get<Place[]>('/api/places');
  }

  nearby(lat: number, lng: number, radiusKm: number): Observable<Place[]> {
    const params = new HttpParams()
      .set('lat', lat)
      .set('lng', lng)
      .set('radiusKm', radiusKm);
    return this.http.get<Place[]>('/api/places/nearby', { params });
  }
}
