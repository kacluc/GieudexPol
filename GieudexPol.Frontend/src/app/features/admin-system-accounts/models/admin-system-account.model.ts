export interface AdminSystemWallet {
  currencyId: number;
  currencyCode: string;
  currencyName: string;
  balance: number;
  reservedBalance: number;
  availableBalance: number;
}

export interface AdminSystemAccount {
  userId: number;
  username: string;
  displayName: string;
  accountType: 'RateSourceSystem' | 'PlatformTreasury';
  rateSourceCode?: string | null;
  rateSourceName?: string | null;
  rateSourceIsActive?: boolean | null;
  wallets: AdminSystemWallet[];
}
