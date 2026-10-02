export interface Place {
  id: number;
  name: string;
  category: string;
  region: string;
  description: string;
  latitude: number;
  longitude: number;
  /** Only present on "nearby" results. */
  distanceKm?: number | null;
}

export interface HealthStatus {
  api: string;
  database: string;
  postgis: string | null;
}
