export interface Transaction {
    id: number;
    senderId: number;
    receiverId: number;
    amount: number;
    currencyId: number;
    status: string; // e.g., "Pending", "Completed", "Failed"
    transactionType: string; // e.g., "Transfer", "Buy", "Sell"
    appliedFee: number;
    transactionFeeId?: string; // Optional, as it can be null
    timestamp: Date;
}
