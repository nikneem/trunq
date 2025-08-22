export interface Environment {
  production: boolean;
  apiBaseUrl: string;
  shortLinkBaseUrl: string;
}

export const environment: Environment = {
  production: false,
  apiBaseUrl: 'https://localhost:7277',
  shortLinkBaseUrl: 'https://localhost:7277'
};
