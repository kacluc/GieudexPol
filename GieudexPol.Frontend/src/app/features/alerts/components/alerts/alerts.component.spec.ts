import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, Subject } from 'rxjs';
import { AuthService } from '../../../auth/services/auth.service';
import { CurrencyService } from '../../../../services/currency.service';
import { UserAlertService } from '../../services/user-alert.service';
import { AlertsComponent } from './alerts.component';

describe('AlertsComponent', () => {
  let fixture: ComponentFixture<AlertsComponent>;
  const currencies$ = new Subject<Array<{ id: number; symbol: string; name: string; isActive: boolean }>>();

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AlertsComponent],
      providers: [
        { provide: AuthService, useValue: { user$: of({ id: 1 }) } },
        {
          provide: CurrencyService,
          useValue: { getAllCurrencies: () => currencies$.asObservable() },
        },
        {
          provide: UserAlertService,
          useValue: { getUserAlerts: () => of([]) },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AlertsComponent);
    fixture.detectChanges();
  });

  it('renders currencies immediately when the API response arrives', async () => {
    currencies$.next([
      { id: 1, symbol: 'PLN', name: 'Polski złoty', isActive: true },
      { id: 2, symbol: 'EUR', name: 'Euro', isActive: true },
    ]);
    await fixture.whenStable();

    const options = Array.from(
      fixture.nativeElement.querySelectorAll('#currency option') as NodeListOf<HTMLOptionElement>
    ).map(option => option.textContent?.trim());

    expect(options).toEqual(['Wybierz walutę', 'PLN', 'EUR']);
  });
});
