export interface UserDto {
  id: number;
  username: string;
  email: string;
  displayName: string;
  role: 'Admin' | 'User';
}
