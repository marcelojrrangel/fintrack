import { TestBed } from '@angular/core/testing';

import { DEFAULT_CATEGORIES } from '../../../core/constants/default-categories';
import { TransactionsToolbarComponent } from './transactions-toolbar.component';

describe('TransactionsToolbarComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TransactionsToolbarComponent],
    }).compileComponents();
  });

  it('should emit category changes', () => {
    const fixture = TestBed.createComponent(TransactionsToolbarComponent);
    const component = fixture.componentInstance;
    spyOn(component.categoryChanged, 'emit');

    fixture.componentRef.setInput('categories', DEFAULT_CATEGORIES);
    fixture.detectChanges();

    const select = fixture.nativeElement.querySelector('select') as HTMLSelectElement;
    select.value = DEFAULT_CATEGORIES[1].id;
    select.dispatchEvent(new Event('change'));

    expect(component.categoryChanged.emit).toHaveBeenCalledWith(DEFAULT_CATEGORIES[1].id);
  });

  it('should emit refresh and create actions', () => {
    const fixture = TestBed.createComponent(TransactionsToolbarComponent);
    const component = fixture.componentInstance;
    spyOn(component.refreshRequested, 'emit');
    spyOn(component.createRequested, 'emit');

    fixture.detectChanges();
    const buttons = fixture.nativeElement.querySelectorAll('button');
    (buttons[0] as HTMLButtonElement).click();
    (buttons[1] as HTMLButtonElement).click();

    expect(component.refreshRequested.emit).toHaveBeenCalled();
    expect(component.createRequested.emit).toHaveBeenCalled();
  });
});
