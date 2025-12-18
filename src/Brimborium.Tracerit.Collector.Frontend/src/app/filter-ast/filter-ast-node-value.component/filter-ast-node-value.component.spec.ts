import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilterAstNodeValue } from './filter-ast-node-value.component';
import { provideZonelessChangeDetection } from '@angular/core';

describe('FilterAstNodeValue', () => {
  let component: FilterAstNodeValue;
  let fixture: ComponentFixture<FilterAstNodeValue>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
      imports: [FilterAstNodeValue]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FilterAstNodeValue);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
