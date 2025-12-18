import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilterEditComponent } from './filter-edit.component';
import { provideZonelessChangeDetection } from '@angular/core';

describe('FilterEditComponent', () => {
  let component: FilterEditComponent;
  let fixture: ComponentFixture<FilterEditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
      imports: [FilterEditComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FilterEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
