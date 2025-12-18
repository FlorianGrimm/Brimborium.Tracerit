import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ToolWindow } from './tool-window';

describe('ToolWindow', () => {
  let component: ToolWindow;
  let fixture: ComponentFixture<ToolWindow>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToolWindow]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ToolWindow);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
