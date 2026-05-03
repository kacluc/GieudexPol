import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Orderbook } from './orderbook';

describe('Orderbook', () => {
  let component: Orderbook;
  let fixture: ComponentFixture<Orderbook>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Orderbook],
    }).compileComponents();

    fixture = TestBed.createComponent(Orderbook);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
