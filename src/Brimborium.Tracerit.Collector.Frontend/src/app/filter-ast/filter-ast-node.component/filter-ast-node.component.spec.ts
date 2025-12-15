import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilterAstNodeComponent } from './filter-ast-node.component';

describe('FilterAstNodeComponent', () => {
  let component: FilterAstNodeComponent;
  let fixture: ComponentFixture<FilterAstNodeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FilterAstNodeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FilterAstNodeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
