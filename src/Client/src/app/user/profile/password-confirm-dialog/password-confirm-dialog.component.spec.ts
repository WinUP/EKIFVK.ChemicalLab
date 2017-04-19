import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PasswordConfirmDialogComponent } from './password-confirm-dialog.component';

describe('PasswordConfirmDialogComponent', () => {
  let component: PasswordConfirmDialogComponent;
  let fixture: ComponentFixture<PasswordConfirmDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PasswordConfirmDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PasswordConfirmDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
