import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewDepDataLogComponent } from './view-dep-data-log.component';

describe('ViewDepDataLogComponent', () => {
  let component: ViewDepDataLogComponent;
  let fixture: ComponentFixture<ViewDepDataLogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViewDepDataLogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViewDepDataLogComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
