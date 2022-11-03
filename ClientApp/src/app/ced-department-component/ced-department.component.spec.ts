import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CedDepartmentComponent } from './ced-department.component';

describe('CeDepartmentComponent', () => {
  let component: CedDepartmentComponent;
  let fixture: ComponentFixture<CedDepartmentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CedDepartmentComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CedDepartmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
