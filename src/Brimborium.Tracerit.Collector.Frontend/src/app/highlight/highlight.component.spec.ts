import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HighlightComponent } from './highlight.component';
import { provideZonelessChangeDetection } from '@angular/core';

describe('HighlightComponent', () => {
  let component: HighlightComponent;
  let fixture: ComponentFixture<HighlightComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
      imports: [HighlightComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HighlightComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
