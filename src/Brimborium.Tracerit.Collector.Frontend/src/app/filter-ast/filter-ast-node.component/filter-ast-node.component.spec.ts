import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilterAstNodeComponent } from './filter-ast-node.component';
import { provideZonelessChangeDetection } from '@angular/core';

describe('FilterAstNodeComponent', () => {
  let component: FilterAstNodeComponent;
  let fixture: ComponentFixture<FilterAstNodeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
      imports: [FilterAstNodeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FilterAstNodeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    fixture.destroy();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
