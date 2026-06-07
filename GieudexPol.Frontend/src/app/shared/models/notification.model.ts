export interface NotificationDto {
  id: number;
  userId: number;
  message: string;
  createdDate: Date;
  isRead: boolean;
}
