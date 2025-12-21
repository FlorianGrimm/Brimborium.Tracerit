import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilterAstNameComponent } from './filter-ast-name.component';

describe('FilterAstNameComponent', () => {
  let component: FilterAstNameComponent;
  let fixture: ComponentFixture<FilterAstNameComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FilterAstNameComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FilterAstNameComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
