import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TraceViewComponent } from './trace-view.component';

describe('TraceViewComponent', () => {
  let component: TraceViewComponent;
  let fixture: ComponentFixture<TraceViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TraceViewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TraceViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
