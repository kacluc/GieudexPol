export type AdminUserRole = 'Admin' | 'User';

export interface AdminUser {
  id: number;
  email: string;
  username: string;
  displayName: string;
  role: AdminUserRole;
}

export interface CreateAdminUser {
  email: string;
  password: string;
  role: AdminUserRole;
}
