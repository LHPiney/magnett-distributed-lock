import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  private readonly oidcIssuer = (window as any)['NG_APP_OIDC_ISSUER'] || 'http://localhost:8080/realms/locks';
  private readonly oidcClientId = (window as any)['NG_APP_OIDC_CLIENT_ID'] || 'portal';
  private readonly apiBaseUrl = (window as any)['NG_APP_API_BASE_URL'] || 'http://localhost:8080';

  getOidcIssuer(): string {
    return this.oidcIssuer;
  }

  getOidcClientId(): string {
    return this.oidcClientId;
  }

  getApiBaseUrl(): string {
    return this.apiBaseUrl;
  }
}

