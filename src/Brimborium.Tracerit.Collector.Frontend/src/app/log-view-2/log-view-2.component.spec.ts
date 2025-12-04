import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LogView2Component } from './log-view-2.component';

describe('LogView2Component', () => {
  let component: LogView2Component;
  let fixture: ComponentFixture<LogView2Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LogView2Component]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LogView2Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
