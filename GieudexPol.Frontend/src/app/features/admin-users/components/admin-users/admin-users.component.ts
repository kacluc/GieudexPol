import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  AdminUser,
  AdminUserRole,
  CreateAdminUser,
} from '../../models/admin-user.model';
import { AdminUsersService } from '../../services/admin-users.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.scss',
})
export class AdminUsersComponent implements OnInit {
  readonly roles: AdminUserRole[] = ['User', 'Admin'];
  users: AdminUser[] = [];
  roleDrafts: Record<number, AdminUserRole> = {};
  resetUser: AdminUser | null = null;
  loading = false;
  saving = false;
  errorMessage = '';
  successMessage = '';

  readonly createForm;
  readonly resetPasswordForm;

  constructor(
    private readonly adminUsersService: AdminUsersService,
    private readonly formBuilder: FormBuilder,
    private readonly changeDetector: ChangeDetectorRef,
  ) {
    this.createForm = this.formBuilder.nonNullable.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['User' as AdminUserRole, Validators.required],
    });
    this.resetPasswordForm = this.formBuilder.nonNullable.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.errorMessage = '';
    this.adminUsersService.getUsers().subscribe({
      next: users => {
        this.users = users.map(user => ({
          ...user,
          role: this.normalizeRole(user.role),
        }));
        this.roleDrafts = Object.fromEntries(
          this.users.map(user => [user.id, user.role]),
        );
        this.loading = false;
        this.changeDetector.markForCheck();
      },
      error: error => this.handleError(error),
    });
  }

  createUser(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    const request = this.createForm.getRawValue() as CreateAdminUser;
    this.saving = true;
    this.clearMessages();
    this.adminUsersService.createUser(request).subscribe({
      next: () => {
        this.successMessage = 'Uzytkownik zostal utworzony.';
        this.createForm.reset({ email: '', password: '', role: 'User' });
        this.saving = false;
        this.loadUsers();
      },
      error: error => this.handleError(error),
    });
  }

  setRoleDraft(userId: number, role: string): void {
    if (role === 'Admin' || role === 'User') {
      this.roleDrafts[userId] = role;
    }
  }

  private normalizeRole(role: string): AdminUserRole {
    return role?.trim().toLowerCase() === 'admin' ? 'Admin' : 'User';
  }

  updateRole(user: AdminUser): void {
    const role = this.roleDrafts[user.id];
    if (!role || role === user.role) {
      return;
    }

    this.saving = true;
    this.clearMessages();
    this.adminUsersService.updateUserRole(user.id, role).subscribe({
      next: updatedUser => {
        this.successMessage = `Rola uzytkownika ${updatedUser.email} zostala zmieniona.`;
        this.saving = false;
        this.loadUsers();
      },
      error: error => {
        this.roleDrafts[user.id] = user.role;
        this.handleError(error);
      },
    });
  }

  openPasswordReset(user: AdminUser): void {
    this.resetUser = user;
    this.resetPasswordForm.reset({ newPassword: '' });
    this.clearMessages();
  }

  cancelPasswordReset(): void {
    this.resetUser = null;
    this.resetPasswordForm.reset({ newPassword: '' });
  }

  resetPassword(): void {
    if (!this.resetUser || this.resetPasswordForm.invalid) {
      this.resetPasswordForm.markAllAsTouched();
      return;
    }

    const confirmed = confirm(
      `Czy na pewno zresetowac haslo uzytkownika ${this.resetUser.email}?`,
    );
    if (!confirmed) {
      return;
    }

    const user = this.resetUser;
    const { newPassword } = this.resetPasswordForm.getRawValue();
    this.saving = true;
    this.clearMessages();
    this.adminUsersService.resetPassword(user.id, newPassword).subscribe({
      next: () => {
        this.successMessage = `Haslo uzytkownika ${user.email} zostalo zresetowane.`;
        this.saving = false;
        this.cancelPasswordReset();
        this.changeDetector.markForCheck();
      },
      error: error => this.handleError(error),
    });
  }

  private clearMessages(): void {
    this.errorMessage = '';
    this.successMessage = '';
  }

  private handleError(error: HttpErrorResponse): void {
    this.loading = false;
    this.saving = false;

    if (error.status === 401 || error.status === 403) {
      this.errorMessage = 'Brak uprawnien administratora.';
    } else if (error.status === 400) {
      this.errorMessage = this.readErrorMessage(error) ?? 'Nieprawidlowe dane formularza.';
    } else if (error.status >= 500) {
      this.errorMessage = 'Wystapil blad serwera. Sprobuj ponownie pozniej.';
    } else {
      this.errorMessage = this.readErrorMessage(error) ?? 'Nie udalo sie wykonac operacji.';
    }

    this.changeDetector.markForCheck();
  }

  private readErrorMessage(error: HttpErrorResponse): string | null {
    if (typeof error.error === 'string') {
      return error.error;
    }

    if (error.error?.message) {
      return error.error.message;
    }

    if (error.error?.errors) {
      const messages = Object.values(error.error.errors).flat();
      return messages.join(' ');
    }

    return null;
  }
}
