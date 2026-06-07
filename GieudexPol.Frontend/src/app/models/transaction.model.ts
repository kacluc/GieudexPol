export interface Transaction {
    id: number;
    senderId: number;
    receiverId: number;
    senderUsername?: string; // Add senderUsername
    receiverUsername?: string; // Add receiverUsername
    amount: number;
    currencyId: number;
    currencySymbol?: string; // Add currencySymbol
    status: string; // e.g., "Pending", "Completed", "Failed"
    transactionType: string; // e.g., "Transfer", "Buy", "Sell"
    appliedFee: number;
    transactionFeeId?: string; // Optional, as it can be null
    timestamp: Date;
}
