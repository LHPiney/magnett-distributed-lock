import { inject, Injectable, signal } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { ConfigService } from '../config/config.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly oauthService = inject(OAuthService);
  private readonly configService = inject(ConfigService);

  readonly isAuthenticated = signal<boolean>(false);
  readonly userProfile = signal<any>(null);

  constructor() {
    this.configureOAuth();
  }

  private configureOAuth(): void {
    this.oauthService.configure({
      issuer: this.configService.getOidcIssuer(),
      clientId: this.configService.getOidcClientId(),
      responseType: 'code',
      redirectUri: window.location.origin,
      scope: 'openid profile email roles',
      showDebugInformation: true,
      requireHttps: false
    });

    this.oauthService.loadDiscoveryDocumentAndTryLogin().then(() => {
      if (this.oauthService.hasValidAccessToken()) {
        this.isAuthenticated.set(true);
        this.loadUserProfile();
      }
    });
  }

  login(): void {
    this.oauthService.initCodeFlow();
  }

  logout(): void {
    this.oauthService.logOut();
    this.isAuthenticated.set(false);
    this.userProfile.set(null);
  }

  private loadUserProfile(): void {
    this.oauthService.loadUserProfile().then((profile) => {
      this.userProfile.set(profile);
    });
  }
}

