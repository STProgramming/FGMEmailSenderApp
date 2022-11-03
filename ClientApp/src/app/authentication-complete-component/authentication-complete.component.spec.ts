import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AuthenticationCompleteComponent } from './authentication-complete.component';

describe('AuthenticationComponent', () => {
  let component: AuthenticationCompleteComponent;
  let fixture: ComponentFixture<AuthenticationCompleteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AuthenticationCompleteComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AuthenticationCompleteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
