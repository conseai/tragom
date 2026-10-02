import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { PlacesService } from './places/places.service';
import { HealthStatus, Place } from './places/place.model';

interface Origin {
  label: string;
  lat: number;
  lng: number;
}

@Component({
  selector: 'app-root',
  imports: [DecimalPipe],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  private readonly places = inject(PlacesService);

  protected readonly origins: Origin[] = [
    { label: 'Novi Sad', lat: 45.2671, lng: 19.8335 },
    { label: 'Belgrade', lat: 44.8125, lng: 20.4612 },
    { label: 'Niš', lat: 43.3209, lng: 21.8958 },
    { label: 'Kraljevo', lat: 43.7258, lng: 20.6894 },
  ];
  protected readonly radii = [25, 50, 100, 200];

  protected readonly health = signal<HealthStatus | null>(null);
  protected readonly healthError = signal<string | null>(null);

  protected readonly origin = signal<Origin>(this.origins[0]);
  protected readonly radiusKm = signal(50);
  protected readonly results = signal<Place[]>([]);
  protected readonly loading = signal(false);
  protected readonly searchError = signal<string | null>(null);

  ngOnInit(): void {
    this.places.health().subscribe({
      next: (h) => this.health.set(h),
      error: (err: { status?: number }) =>
        this.healthError.set(
          err.status === 503
            ? 'The API is running but cannot reach the database. Check the db container with "docker compose logs db".'
            : 'The API is not reachable. Run "docker compose ps" and check that the api container is running.',
        ),
    });
    this.search();
  }

  protected selectOrigin(o: Origin): void {
    this.origin.set(o);
    this.search();
  }

  protected selectRadius(value: string): void {
    this.radiusKm.set(Number(value));
    this.search();
  }

  protected search(): void {
    const o = this.origin();
    this.loading.set(true);
    this.searchError.set(null);
    this.places.nearby(o.lat, o.lng, this.radiusKm()).subscribe({
      next: (list) => {
        this.results.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.results.set([]);
        this.loading.set(false);
        this.searchError.set(
          'Search failed. If this is a fresh setup, the database may have no tables yet: see "Create the first migration" in the tutorial.',
        );
      },
    });
  }
}
