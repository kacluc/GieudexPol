export interface Wallet {
    id: number;
    userId: number;
    currencyId: number;
    balance: number;
    currency: {
        id: number;
        symbol: string;
        name: string;
        isActive: boolean; // Add isActive property
    };
}
