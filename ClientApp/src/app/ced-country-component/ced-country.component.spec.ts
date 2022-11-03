import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CedCountryComponent } from './ced-country.component';

describe('CeCountryComponent', () => {
  let component: CedCountryComponent;
  let fixture: ComponentFixture<CedCountryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CedCountryComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CedCountryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
