import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DEFAULT_CATEGORIES } from '../../../core/constants/default-categories';
import { TransactionFormModalComponent } from './transaction-form-modal.component';

describe('TransactionFormModalComponent', () => {
  let fixture: ComponentFixture<TransactionFormModalComponent>;
  let component: TransactionFormModalComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TransactionFormModalComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TransactionFormModalComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('categories', DEFAULT_CATEGORIES);
    fixture.detectChanges();
  });

  it('should select the first category when categories are provided', () => {
    expect(component.form.controls.categoryId.value).toBe(DEFAULT_CATEGORIES[0].id);
  });

  it('should emit normalized payload when form is valid', () => {
    spyOn(component.saveRequested, 'emit');

    component.form.setValue({
      categoryId: DEFAULT_CATEGORIES[1].id,
      amount: 250,
      transactionDateUtc: '2026-04-30',
      type: 'Expense',
      description: '  Conta de luz  ',
    });

    component.submit();

    expect(component.saveRequested.emit).toHaveBeenCalledWith({
      categoryId: DEFAULT_CATEGORIES[1].id,
      amount: 250,
      transactionDateUtc: '2026-04-30T00:00:00Z',
      type: 'Expense',
      description: 'Conta de luz',
    });
  });

  it('should not emit when form is invalid', () => {
    spyOn(component.saveRequested, 'emit');

    component.form.patchValue({
      description: '',
      amount: 0,
    });

    component.submit();

    expect(component.saveRequested.emit).not.toHaveBeenCalled();
    expect(component.form.touched).toBeTrue();
  });
});
