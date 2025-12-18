import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TraceViewComponent } from './trace-view.component';
import { provideZonelessChangeDetection } from '@angular/core';

describe('TraceViewComponent', () => {
  let component: TraceViewComponent;
  let fixture: ComponentFixture<TraceViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
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
