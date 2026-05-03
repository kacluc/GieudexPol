import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TradingForm } from './trading-form';

describe('TradingForm', () => {
  let component: TradingForm;
  let fixture: ComponentFixture<TradingForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TradingForm],
    }).compileComponents();

    fixture = TestBed.createComponent(TradingForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
