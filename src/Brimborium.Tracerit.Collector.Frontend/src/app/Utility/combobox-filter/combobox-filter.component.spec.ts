import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ComboboxFilterComponent } from './combobox-filter.component';

describe('ComboboxFilterComponent', () => {
  let component: ComboboxFilterComponent;
  let fixture: ComponentFixture<ComboboxFilterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ComboboxFilterComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ComboboxFilterComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
