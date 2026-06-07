export interface UserAlert {
  id: number;
  userId: number;
  currencyId: number;
  targetPrice: number;
  createdAt: string;
  isActive: boolean;
}
